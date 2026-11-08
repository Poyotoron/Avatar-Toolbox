using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal static class FaceKeeperListingBuilder
    {
        private const int MaximumCombinations = 32;

        private sealed class StateEntry
        {
            internal VirtualState State;
            internal VirtualStateMachine Machine;
        }

        internal static FaceKeeperListing Build(BuildContext ctx, List<FaceKeeperTarget> targets)
        {
            var listing = new FaceKeeperListing
            {
                CreatedLabel = "作った時刻: " + DateTime.Now.ToString("HH:mm:ss")
            };
            try
            {
                VRCAvatarDescriptor descriptor = ctx.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
                var leaves = new List<FaceKeeperMenuNode>();
                // NOTE: MA のメニュー構築 API は公開されていないため、組み込み後のメニューを読む。
                ReadMenu(descriptor != null ? descriptor.expressionsMenu : null, listing.MenuRoots,
                    leaves, new HashSet<VRCExpressionsMenu>());
                var values = new Dictionary<EditorCurveBinding, FaceKeeperTarget>();
                foreach (FaceKeeperTarget target in targets)
                {
                    values[target.Binding] = target;
                }
                foreach (var pair in ctx.Extension<AnimatorServicesContext>().ControllerContext.Controllers)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    foreach (VirtualLayer layer in pair.Value.Layers)
                    {
                        if (layer.StateMachine != null)
                        {
                            ReadLayer(pair.Key.ToString(), layer, values, leaves, listing);
                        }
                    }
                }
                Prune(listing.MenuRoots);
                return listing;
            }
            catch
            {
                DestroyClips(listing);
                throw;
            }
        }

        internal static void DestroyClips(FaceKeeperListing listing)
        {
            if (listing == null)
            {
                return;
            }
            var clips = new HashSet<AnimationClip>();
            CollectOwnedClips(listing.MenuRoots, clips);
            CollectOwnedClips(listing.UnmatchedGroups, clips);
            foreach (AnimationClip clip in clips)
            {
                if (clip != null)
                {
                    UnityEngine.Object.DestroyImmediate(clip);
                }
            }
        }

        private static void CollectOwnedClips(List<FaceKeeperMenuNode> nodes, HashSet<AnimationClip> clips)
        {
            foreach (FaceKeeperMenuNode node in nodes)
            {
                foreach (FaceKeeperExpression expression in node.Expressions)
                {
                    clips.Add(expression.BeforeClip);
                    clips.Add(expression.AfterClip);
                }
                CollectOwnedClips(node.Children, clips);
            }
        }

        private static void ReadMenu(VRCExpressionsMenu menu, List<FaceKeeperMenuNode> nodes,
            List<FaceKeeperMenuNode> leaves, HashSet<VRCExpressionsMenu> visited)
        {
            if (menu == null || !visited.Add(menu))
            {
                return;
            }
            foreach (VRCExpressionsMenu.Control control in menu.controls)
            {
                if (control == null)
                {
                    continue;
                }
                if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu && control.subMenu != null)
                {
                    var node = new FaceKeeperMenuNode { Name = control.name };
                    nodes.Add(node);
                    ReadMenu(control.subMenu, node.Children, leaves, visited);
                }
                else if ((control.type == VRCExpressionsMenu.Control.ControlType.Toggle ||
                    control.type == VRCExpressionsMenu.Control.ControlType.Button) &&
                    control.parameter != null && !string.IsNullOrEmpty(control.parameter.name))
                {
                    var node = new FaceKeeperMenuNode
                    {
                        Name = control.name, IsLeaf = true,
                        ParameterName = control.parameter.name, Value = control.value
                    };
                    nodes.Add(node);
                    leaves.Add(node);
                }
            }
        }

        private static void ReadLayer(string controller, VirtualLayer layer,
            Dictionary<EditorCurveBinding, FaceKeeperTarget> targets,
            List<FaceKeeperMenuNode> leaves, FaceKeeperListing listing)
        {
            var states = new List<StateEntry>();
            var transitions = new List<VirtualTransitionBase>();
            var parents = new Dictionary<VirtualStateMachine, VirtualStateMachine>();
            CollectMachine(layer.StateMachine, null, states, transitions, parents);
            FaceKeeperMenuNode unmatched = null;
            foreach (StateEntry entry in states)
            {
                var clips = new List<VirtualClip>();
                CollectMotion(entry.State.Motion, clips, new HashSet<VirtualMotion>());
                FaceKeeperExpression expression = BuildExpression(entry.State.Name,
                    controller + " / " + layer.Name + " / " + entry.State.Name, clips, targets);
                if (expression == null)
                {
                    continue;
                }
                listing.ExpressionCount++;
                List<List<AnimatorCondition>> combinations = Incoming(transitions, entry.State, null);
                VirtualStateMachine current = entry.Machine;
                while (parents.TryGetValue(current, out VirtualStateMachine parent) && parent != null)
                {
                    combinations = Prepend(Incoming(transitions, null, current), combinations);
                    current = parent;
                }
                bool matched = false;
                foreach (FaceKeeperMenuNode leaf in leaves)
                {
                    List<AnimatorCondition> conditions = combinations.FirstOrDefault(
                        sequence => Matches(sequence, leaf.ParameterName, leaf.Value));
                    if (conditions == null)
                    {
                        continue;
                    }
                    matched = true;
                    leaf.Expressions.Add(WithLabel(expression, entry.State.Name,
                        FormatConditions(conditions, leaf.ParameterName)));
                }
                if (!matched)
                {
                    if (unmatched == null)
                    {
                        unmatched = new FaceKeeperMenuNode { Name = controller + " / " + layer.Name };
                        listing.UnmatchedGroups.Add(unmatched);
                    }
                    unmatched.Expressions.Add(WithLabel(expression, entry.State.Name,
                        FormatConditions(combinations[0], null)));
                }
            }
        }

        private static void CollectMachine(VirtualStateMachine machine, VirtualStateMachine parent,
            List<StateEntry> states, List<VirtualTransitionBase> transitions,
            Dictionary<VirtualStateMachine, VirtualStateMachine> parents)
        {
            if (machine == null || parents.ContainsKey(machine))
            {
                return;
            }
            parents.Add(machine, parent);
            transitions.AddRange(machine.AnyStateTransitions);
            // NOTE: 表情はサブステートマシンの Entry の条件でも選ばれるため、必ず含める。
            transitions.AddRange(machine.EntryTransitions);
            foreach (var pair in machine.StateMachineTransitions)
            {
                transitions.AddRange(pair.Value);
            }
            foreach (var child in machine.States)
            {
                states.Add(new StateEntry { State = child.State, Machine = machine });
                transitions.AddRange(child.State.Transitions);
            }
            foreach (var child in machine.StateMachines)
            {
                CollectMachine(child.StateMachine, machine, states, transitions, parents);
            }
        }

        private static List<List<AnimatorCondition>> Incoming(List<VirtualTransitionBase> transitions,
            VirtualState state, VirtualStateMachine machine)
        {
            var result = new List<List<AnimatorCondition>>();
            foreach (VirtualTransitionBase transition in transitions)
            {
                if ((state != null && transition.DestinationState == state) ||
                    (machine != null && transition.DestinationStateMachine == machine))
                {
                    result.Add(transition.Conditions.ToList());
                    if (result.Count == MaximumCombinations)
                    {
                        break;
                    }
                }
            }
            if (result.Count == 0)
            {
                result.Add(new List<AnimatorCondition>());
            }
            return result;
        }

        private static List<List<AnimatorCondition>> Prepend(List<List<AnimatorCondition>> prefixes,
            List<List<AnimatorCondition>> suffixes)
        {
            var result = new List<List<AnimatorCondition>>();
            foreach (List<AnimatorCondition> prefix in prefixes)
            {
                foreach (List<AnimatorCondition> suffix in suffixes)
                {
                    var combined = new List<AnimatorCondition>(prefix);
                    combined.AddRange(suffix);
                    result.Add(combined);
                    if (result.Count == MaximumCombinations)
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        private static bool Matches(List<AnimatorCondition> conditions, string parameter, float value)
        {
            bool found = false;
            foreach (AnimatorCondition condition in conditions)
            {
                if (condition.parameter != parameter)
                {
                    continue;
                }
                found = true;
                bool satisfied;
                switch (condition.mode)
                {
                    case AnimatorConditionMode.If:
                        satisfied = value != 0f;
                        break;
                    case AnimatorConditionMode.IfNot:
                        satisfied = value == 0f;
                        break;
                    case AnimatorConditionMode.Equals:
                        satisfied = Mathf.Approximately(value, condition.threshold);
                        break;
                    case AnimatorConditionMode.NotEqual:
                        satisfied = !Mathf.Approximately(value, condition.threshold);
                        break;
                    case AnimatorConditionMode.Greater:
                        satisfied = value > condition.threshold;
                        break;
                    case AnimatorConditionMode.Less:
                        satisfied = value < condition.threshold;
                        break;
                    default:
                        satisfied = false;
                        break;
                }
                if (!satisfied)
                {
                    return false;
                }
            }
            return found;
        }

        private static string FormatConditions(List<AnimatorCondition> conditions, string excludedParameter)
        {
            var parts = new List<string>();
            foreach (AnimatorCondition condition in conditions)
            {
                if (condition.parameter == excludedParameter)
                {
                    continue;
                }
                string value = condition.threshold.ToString("0.###", CultureInfo.InvariantCulture);
                switch (condition.mode)
                {
                    case AnimatorConditionMode.If:
                        parts.Add(condition.parameter);
                        break;
                    case AnimatorConditionMode.IfNot:
                        parts.Add(condition.parameter + " = OFF");
                        break;
                    case AnimatorConditionMode.Equals:
                        parts.Add(condition.parameter + " = " + value);
                        break;
                    case AnimatorConditionMode.NotEqual:
                        parts.Add(condition.parameter + " ≠ " + value);
                        break;
                    case AnimatorConditionMode.Greater:
                        parts.Add(condition.parameter + " > " + value);
                        break;
                    case AnimatorConditionMode.Less:
                        parts.Add(condition.parameter + " < " + value);
                        break;
                }
            }
            return string.Join("、", parts);
        }

        private static void CollectMotion(VirtualMotion motion, List<VirtualClip> clips,
            HashSet<VirtualMotion> visited)
        {
            if (motion == null || !visited.Add(motion))
            {
                return;
            }
            if (motion is VirtualClip clip)
            {
                clips.Add(clip);
            }
            else if (motion is VirtualBlendTree tree)
            {
                foreach (var child in tree.Children)
                {
                    CollectMotion(child.Motion, clips, visited);
                }
            }
        }

        private static FaceKeeperExpression BuildExpression(string name, string previewLabel,
            List<VirtualClip> clips, Dictionary<EditorCurveBinding, FaceKeeperTarget> targets)
        {
            var changes = new List<string>();
            VirtualClip best = null;
            int bestCount = 0;
            List<float> bestTimes = null;
            foreach (VirtualClip clip in clips)
            {
                int count = 0;
                var times = new List<float>();
                foreach (var pair in targets)
                {
                    AnimationCurve curve = clip.GetFloatCurve(pair.Key);
                    if (curve == null)
                    {
                        continue;
                    }
                    AnimationCurve raised = CopyCurve(curve);
                    int raisedCount = FaceKeeperRule.Raise(raised, pair.Value.CurrentValue);
                    count += raisedCount;
                    if (raisedCount == 0)
                    {
                        continue;
                    }
                    for (int i = 0; i < curve.length; i++)
                    {
                        if (curve[i].value == raised[i].value)
                        {
                            continue;
                        }
                        changes.Add((clips.Count > 1 ? clip.Name + ": " : "") +
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} / {1} / t={2:0.###} : {3:0.##} → {4:0.##}",
                                pair.Value.MeshName, pair.Value.BlendShapeName,
                                curve[i].time, curve[i].value, raised[i].value));
                        times.Add(curve[i].time);
                    }
                }
                if (count > bestCount)
                {
                    bestCount = count;
                    best = clip;
                    bestTimes = times;
                }
            }
            if (best == null)
            {
                return null;
            }
            var distinctTimes = new List<float>();
            bestTimes.Sort();
            foreach (float time in bestTimes)
            {
                if (distinctTimes.Count == 0 || time - distinctTimes[distinctTimes.Count - 1] >= 0.0001f)
                {
                    distinctTimes.Add(time);
                }
            }
            var before = new AnimationClip { name = best.Name, hideFlags = HideFlags.HideAndDontSave };
            var after = new AnimationClip { name = best.Name, hideFlags = HideFlags.HideAndDontSave };
            try
            {
                foreach (EditorCurveBinding binding in best.GetFloatCurveBindings())
                {
                    AnimationCurve original = best.GetFloatCurve(binding);
                    if (original == null)
                    {
                        continue;
                    }
                    AnimationUtility.SetEditorCurve(before, binding, CopyCurve(original));
                    AnimationCurve modified = CopyCurve(original);
                    if (targets.TryGetValue(binding, out FaceKeeperTarget target))
                    {
                        FaceKeeperRule.Raise(modified, target.CurrentValue);
                    }
                    AnimationUtility.SetEditorCurve(after, binding, modified);
                }
                return new FaceKeeperExpression
                {
                    Label = name, PreviewLabel = previewLabel, ChangeLines = changes,
                    ChangeCountLabel = string.Format("変更 {0} 件", changes.Count),
                    BeforeClip = before, AfterClip = after, Times = distinctTimes.ToArray()
                };
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(before);
                UnityEngine.Object.DestroyImmediate(after);
                throw;
            }
        }

        private static AnimationCurve CopyCurve(AnimationCurve source)
        {
            return new AnimationCurve(source.keys) { preWrapMode = source.preWrapMode, postWrapMode = source.postWrapMode };
        }

        private static FaceKeeperExpression WithLabel(FaceKeeperExpression source, string stateName, string conditions)
        {
            return new FaceKeeperExpression
            {
                Label = string.IsNullOrEmpty(conditions) ? stateName : stateName + "（" + conditions + "）",
                PreviewLabel = source.PreviewLabel, ChangeLines = source.ChangeLines,
                ChangeCountLabel = source.ChangeCountLabel, BeforeClip = source.BeforeClip,
                AfterClip = source.AfterClip, Times = source.Times
            };
        }

        private static void Prune(List<FaceKeeperMenuNode> nodes)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                FaceKeeperMenuNode node = nodes[i];
                Prune(node.Children);
                if (node.Expressions.Count == 0 && node.Children.Count == 0)
                {
                    nodes.RemoveAt(i);
                }
            }
        }
    }
}
