using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    internal static class AnimationHelperToggleBuilder
    {
        internal static AnimatorController Build(PATAnimationHelper component, Transform avatarRoot,
            string parameterName)
        {
            var bindings = new List<EditorCurveBinding>();
            var start = new List<float>();
            var end = new List<float>();
            foreach (PATAnimationHelperEntry entry in component.entries)
            {
                AnimationHelperCurves.Collect(entry, avatarRoot, bindings);
                AnimationHelperCurves.ReadValues(entry, AnimationHelperValueKind.Start, start);
                AnimationHelperCurves.ReadValues(entry, AnimationHelperValueKind.End, end);
            }
            var controller = new AnimatorController { name = "PAT Animation Helper" };
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = parameterName, type = AnimatorControllerParameterType.Bool,
                defaultBool = component.toggleDefaultOn
            });
            controller.AddLayer("PAT Animation Helper");
            AnimatorControllerLayer[] layers = controller.layers;
            layers[0].defaultWeight = 1f;
            controller.layers = layers;
            AnimatorStateMachine machine = layers[0].stateMachine;
            AnimationClip offClip = AnimationHelperCurves.CreateConstantClip("Off", bindings, start);
            AnimatorState init = State(machine, "Init", new Vector3(0f, -160f), offClip);
            AnimatorState off = State(machine, "Off", Vector3.zero, offClip);
            AnimatorState playing = State(machine, "Playing", new Vector3(250f, -80f),
                AnimationHelperCurves.CreateMovingClip("Playing", bindings, start, end,
                    component.toggleDuration, component.toggleEasing, false));
            AnimatorState on = State(machine, "On", new Vector3(500f, 0f),
                AnimationHelperCurves.CreateConstantClip("On", bindings, end));
            // NOTE: Entry の遷移は開始時に評価されないため、Init で値を判定し、読み込み時の動きを省く。
            machine.defaultState = init;
            Transition(init, on, parameterName, true);
            Transition(init, off, parameterName, false);
            Transition(off, playing, parameterName, true);
            // NOTE: 途中で割り込むと次の動きが端から始まり値が飛ぶため、再生は最後まで待つ。
            Transition(playing, on, null, false);
            if (component.toggleReverse)
            {
                AnimatorState reversing = State(machine, "Reversing", new Vector3(250f, 80f),
                    AnimationHelperCurves.CreateMovingClip("Reversing", bindings, end, start,
                        component.toggleDuration, component.toggleEasing, true));
                Transition(on, reversing, parameterName, false);
                Transition(reversing, off, null, false);
            }
            else
            {
                Transition(on, off, parameterName, false);
            }
            return controller;
        }

        private static AnimatorState State(AnimatorStateMachine machine, string name, Vector3 position,
            Motion motion)
        {
            AnimatorState state = machine.AddState(name, position);
            state.writeDefaultValues = false;
            state.motion = motion;
            return state;
        }

        private static void Transition(AnimatorState from, AnimatorState to, string parameter, bool value)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = parameter == null;
            transition.exitTime = 1f;
            transition.duration = 0f;
            transition.hasFixedDuration = true;
            if (parameter != null)
            {
                transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                    0f, parameter);
            }
        }
    }
}
