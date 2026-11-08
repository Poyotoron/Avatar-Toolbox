using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    [CustomEditor(typeof(PATFaceKeeper))]
    internal sealed class PATFaceKeeperEditor : UnityEditor.Editor
    {
        private static readonly GUIContent Meshes = new GUIContent("顔のメッシュ");
        private static readonly GUIContent Manual = new GUIContent("自分で指定する");
        private static readonly GUIContent Excluded = new GUIContent("引き上げないブレンドシェイプ");
        private static readonly GUIContent On = new GUIContent("ON");
        private static readonly GUIContent Off = new GUIContent("OFF");
        private static readonly GUILayoutOption[] GroupButtonOptions = { GUILayout.Width(40f) };
        private static readonly GUILayoutOption[] CountOptions = { GUILayout.Width(90f) };
        private readonly List<SkinnedMeshRenderer> _meshes = new List<SkinnedMeshRenderer>();
        private readonly HashSet<string> _excluded = new HashSet<string>(System.StringComparer.Ordinal);
        private readonly HashSet<string> _pendingOn = new HashSet<string>(System.StringComparer.Ordinal);
        private readonly HashSet<string> _pendingOff = new HashSet<string>(System.StringComparer.Ordinal);
        private List<FaceKeeperExclusionMesh> _exclusionViews = new List<FaceKeeperExclusionMesh>();
        private ReorderableList _meshList;
        private string _error;
        private string _warning;
        private bool _multiple;
        private static readonly GUIContent ListingTitle = new GUIContent("表情の一覧");
        private static readonly GUIContent MakeListing = new GUIContent("一覧を作る");
        private static readonly Color SelectedColor = new Color(0.45f, 0.65f, 1f, 1f);
        private FaceKeeperListing _listing;
        private FaceKeeperExpression _selectedExpression;
        private string _listingError;
        private bool _listingQueued;
        private static readonly GUIContent Preview = new GUIContent("プレビュー");
        private float _previewWidth;


        private void OnEnable()
        {
            SerializedProperty meshes = serializedObject.FindProperty("meshes");
            _meshList = new ReorderableList(serializedObject, meshes, true, true, true, true);
            _meshList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, Meshes);
            _meshList.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, meshes.GetArrayElementAtIndex(index), GUIContent.none);
            };
            _meshList.onAddCallback = list =>
            {
                int index = meshes.arraySize++;
                meshes.GetArrayElementAtIndex(index).objectReferenceValue = null;
            };
            Undo.undoRedoPerformed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Refresh;
            EditorApplication.delayCall -= BuildListing;
            _listingQueued = false;
            DiscardListing();
        }

        private void Refresh()
        {
            if (target == null)
            {
                return;
            }
            serializedObject.Update();
            var component = (PATFaceKeeper)target;
            VRCAvatarDescriptor descriptor = component.GetComponentInParent<VRCAvatarDescriptor>(true);
            Transform avatarRoot = descriptor != null ? descriptor.transform : null;
            _warning = descriptor == null ? "アバターの中に置いてください。"
                : component.transform != avatarRoot
                    ? "アバタールート（VRC Avatar Descriptor のある GameObject）に付けることをおすすめします。"
                    : null;
            _multiple = descriptor != null && descriptor.GetComponentsInChildren<PATFaceKeeper>(true).Length > 1;
            FaceKeeperMeshes.Resolve(component, avatarRoot, _meshes, out _error);
            _excluded.Clear();
            foreach (string name in component.excludedBlendShapes)
            {
                _excluded.Add(name);
            }
            _exclusionViews = FaceKeeperExclusionView.Build(_meshes, _excluded, _exclusionViews);
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var component = (PATFaceKeeper)target;
            if (!_excluded.SetEquals(component.excludedBlendShapes))
            {
                Refresh();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(Meshes, EditorStyles.boldLabel);
            SerializedProperty manual = serializedObject.FindProperty("manualMeshes");
            EditorGUILayout.PropertyField(manual, Manual);
            if (!manual.boolValue)
            {
                EditorGUILayout.LabelField("VRC Avatar Descriptor の口と、まぶたの設定から見つけています。",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUI.BeginDisabledGroup(true);
                foreach (SkinnedMeshRenderer mesh in _meshes)
                {
                    EditorGUILayout.ObjectField(mesh, typeof(SkinnedMeshRenderer), true);
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                _meshList.DoLayoutList();
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Excluded, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("チェックを入れたブレンドシェイプは、表情で下げられてもそのままにします。",
                EditorStyles.wordWrappedMiniLabel);
            _pendingOn.Clear();
            _pendingOff.Clear();
            DrawExclusions();
            ApplyExclusions();
            bool changed = EditorGUI.EndChangeCheck();
            bool applied = serializedObject.ApplyModifiedProperties();
            if (changed || applied)
            {
                Refresh();
            }
            DrawListing();
            DrawIssues();
        }

        private void DrawExclusions()
        {
            foreach (FaceKeeperExclusionMesh mesh in _exclusionViews)
            {
                mesh.Expanded = EditorGUILayout.Foldout(mesh.Expanded, mesh.Label, true);
                if (!mesh.Expanded)
                {
                    continue;
                }
                EditorGUI.indentLevel++;
                DrawMembers(mesh, mesh.Ungrouped);
                foreach (FaceKeeperExclusionGroup group in mesh.Groups)
                {
                    EditorGUILayout.BeginHorizontal();
                    group.Expanded = EditorGUILayout.Foldout(group.Expanded, group.Label, true);
                    EditorGUILayout.LabelField(group.Count, EditorStyles.miniLabel, CountOptions);
                    bool on = GUILayout.Button(On, GroupButtonOptions);
                    bool off = GUILayout.Button(Off, GroupButtonOptions);
                    EditorGUILayout.EndHorizontal();
                    if (on || off)
                    {
                        foreach (int index in group.Members)
                        {
                            QueueExclusion(mesh.Names[index], on);
                        }
                    }
                    if (group.Expanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawMembers(mesh, group.Members);
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawMembers(FaceKeeperExclusionMesh mesh, int[] members)
        {
            foreach (int index in members)
            {
                bool before = _excluded.Contains(mesh.Names[index]);
                bool after = EditorGUILayout.ToggleLeft(mesh.Labels[index], before);
                if (before != after)
                {
                    QueueExclusion(mesh.Names[index], after);
                }
            }
        }

        private void QueueExclusion(string name, bool value)
        {
            if (value)
            {
                _pendingOn.Add(name);
                _pendingOff.Remove(name);
            }
            else
            {
                _pendingOff.Add(name);
                _pendingOn.Remove(name);
            }
        }

        private void ApplyExclusions()
        {
            SerializedProperty excluded = serializedObject.FindProperty("excludedBlendShapes");
            for (int i = excluded.arraySize - 1; i >= 0; i--)
            {
                if (_pendingOff.Contains(excluded.GetArrayElementAtIndex(i).stringValue))
                {
                    excluded.DeleteArrayElementAtIndex(i);
                }
            }
            foreach (string name in _pendingOn)
            {
                if (!_excluded.Contains(name))
                {
                    int index = excluded.arraySize++;
                    excluded.GetArrayElementAtIndex(index).stringValue = name;
                }
            }
        }

        private void DiscardListing()
        {
            // NOTE: クリップを消す前に再生先を閉じ、消えたクリップを描画させない。
            FaceKeeperPreviewWindow.CloseIfOpen();
            FaceKeeperListingBuilder.DestroyClips(_listing);
            _listing = null;
            _selectedExpression = null;
        }

        private void QueueListing()
        {
            if (_listingQueued)
            {
                return;
            }
            _listingQueued = true;
            EditorApplication.delayCall += BuildListing;
        }

        private void BuildListing()
        {
            _listingQueued = false;
            if (this == null || target == null)
            {
                return;
            }
            DiscardListing();
            _listing = FaceKeeperListingRunner.Run((PATFaceKeeper)target, out _listingError);
            Repaint();
        }

        private void DrawListing()
        {
            if (_previewWidth <= 0f)
            {
                _previewWidth = GUI.skin.button.CalcSize(Preview).x + 8f;
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(ListingTitle, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "このコンポーネントで値が変わる表情を、メニューの階層に沿って並べます。一覧を作るときに、アバターのビルドを 1 回流します。",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying || EditorApplication.isCompiling || _listingQueued);
            if (GUILayout.Button(MakeListing))
            {
                QueueListing();
            }
            EditorGUI.EndDisabledGroup();
            if (_listing != null)
            {
                EditorGUILayout.LabelField(_listing.CreatedLabel, EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            if (_listingError != null)
            {
                EditorGUILayout.HelpBox(_listingError, MessageType.Error);
            }
            if (_listing == null)
            {
                return;
            }
            if (_listing.ExpressionCount == 0)
            {
                EditorGUILayout.HelpBox("このコンポーネントで値が変わる表情はありません。", MessageType.Info);
                return;
            }
            DrawNodes(_listing.MenuRoots);
            if (_listing.UnmatchedGroups.Count > 0)
            {
                EditorGUILayout.LabelField("メニューにない表情", EditorStyles.boldLabel);
                DrawNodes(_listing.UnmatchedGroups);
            }
            if (_selectedExpression != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (string line in _selectedExpression.ChangeLines)
                {
                    GUILayout.Label(line, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawNodes(List<FaceKeeperMenuNode> nodes)
        {
            foreach (FaceKeeperMenuNode node in nodes)
            {
                node.Expanded = EditorGUILayout.Foldout(node.Expanded, node.Name, true);
                if (!node.Expanded)
                {
                    continue;
                }
                EditorGUI.indentLevel++;
                DrawNodes(node.Children);
                for (int i = 0; i < node.Expressions.Count; i++)
                {
                    FaceKeeperExpression expression = node.Expressions[i];
                    Color previous = GUI.backgroundColor;
                    if (_selectedExpression == expression)
                    {
                        GUI.backgroundColor = SelectedColor;
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUI.backgroundColor = previous;
                    EditorGUILayout.LabelField(expression.Label);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(expression.ChangeCountLabel, EditorStyles.miniLabel);
                    if (GUILayout.Button(Preview, GUILayout.Width(_previewWidth)))
                    {
                        var component = (PATFaceKeeper)target;
                        VRCAvatarDescriptor descriptor = component.GetComponentInParent<VRCAvatarDescriptor>(true);
                        if (descriptor != null)
                        {
                            FaceKeeperPreviewWindow.Open(descriptor.gameObject,
                                node.Expressions, i, _listing.TargetPaths);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    Rect row = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition))
                    {
                        _selectedExpression = expression;
                        Event.current.Use();
                        Repaint();
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawIssues()
        {
            if (_multiple)
            {
                EditorGUILayout.HelpBox(FaceKeeperErrors.MultipleText, MessageType.Error);
            }
            if (_error != null)
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
            if (_warning != null)
            {
                EditorGUILayout.HelpBox(_warning, MessageType.Warning);
            }
        }
    }
}
