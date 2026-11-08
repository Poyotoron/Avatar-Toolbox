using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    [CustomEditor(typeof(PATBoneWeightTransfer))]
    internal sealed class PATBoneWeightTransferEditor : UnityEditor.Editor
    {
        private static readonly GUIContent Mode = new GUIContent("転移元の指定");
        private static readonly GUIContent SourceRoot = new GUIContent("起点");
        private static readonly GUIContent IncludeRoot = new GUIContent("起点自身も含める");
        private static readonly GUIContent Destination = new GUIContent("転移先");
        private static readonly GUIContent Ratio = new GUIContent("移す割合");
        private static readonly GUIContent Normalize = new GUIContent("正規化");
        private static readonly GUIContent Rules = new GUIContent("規則");
        private static readonly GUIContent Sources = new GUIContent("転移元");
        private static readonly GUIContent AddRule = new GUIContent("空の規則を追加");
        private static readonly GUIContent DuplicateRule = new GUIContent("選んでいる規則を複製");
        private static readonly GUIContent Check = new GUIContent("影響を確認");
        private static readonly GUIContent Visual = new GUIContent("見た目で確かめる");
        private static readonly GUIContent Vertices = new GUIContent("影響する頂点を表示");
        private static readonly GUIContent Bend = new GUIContent("曲げて確かめる");
        private static readonly GUIContent After = new GUIContent("移したあとの形で表示");
        private BoneWeightTransferBend _bend;
        private BoneWeightTransferVertexOverlay _overlay;
        private BoneWeightTransferResult _validation;
        private readonly List<bool> _expanded = new List<bool>();
        private string[] _headers;
        private string _error;
        private BoneWeightTransferResult _result;
        private SerializedProperty _rules;
        private ReorderableList _list;
        private readonly Dictionary<string, ReorderableList> _sourceLists =
            new Dictionary<string, ReorderableList>();

        private void OnEnable()
        {
            _rules = serializedObject.FindProperty("rules");
            _overlay = new BoneWeightTransferVertexOverlay((PATBoneWeightTransfer)target);
            _bend = new BoneWeightTransferBend();
            _bend.Changed += OnBendChanged;
            _list = new ReorderableList(serializedObject, _rules, true, true, true, true);
            _list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, Rules);
            _list.drawElementCallback = DrawRule;
            _list.elementHeightCallback = RuleHeight;
            _list.onCanRemoveCallback = list => _rules.arraySize > 1;
            _list.onRemoveCallback = list =>
            {
                int index = list.index;
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (index >= 0 && index < _expanded.Count)
                {
                    _expanded.RemoveAt(index);
                }
                ApplyRules();
            };
            _list.onReorderCallbackWithDetails = (list, from, to) =>
            {
                bool expanded = _expanded[from];
                _expanded.RemoveAt(from);
                _expanded.Insert(to, expanded);
                ApplyRules();
            };
            _list.onAddDropdownCallback = (rect, list) =>
            {
                var menu = new GenericMenu();
                menu.AddItem(AddRule, false, AddEmptyRule);
                if (list.index >= 0 && list.index < _rules.arraySize)
                {
                    menu.AddItem(DuplicateRule, false, DuplicateSelectedRule);
                }
                else
                {
                    menu.AddDisabledItem(DuplicateRule);
                }
                menu.DropDown(rect);
            };
            Undo.undoRedoPerformed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Refresh;
            _bend.Changed -= OnBendChanged;
            _bend.Dispose();
            _overlay.Dispose();
        }

        private void Refresh()
        {
            if (target == null)
            {
                return;
            }
            serializedObject.Update();
            _result = null;
            // NOTE: 並べ替え後の同じ propertyPath は別の規則を指すため、入れ子のリストを捨てる。
            _sourceLists.Clear();
            _validation = BoneWeightTransferProcessor.Process((PATBoneWeightTransfer)target, false, false);
            _error = BoneWeightTransferErrors.InspectorText(_validation.ErrorKey);
            if (_validation.ErrorKey != null)
            {
                _overlay.SetEnabled(false);
            }
            _overlay.Refresh();
            _bend.SetSources(_validation);
            _headers = new string[_rules.arraySize];
            while (_expanded.Count < _rules.arraySize)
            {
                _expanded.Add(true);
            }
            while (_expanded.Count > _rules.arraySize)
            {
                _expanded.RemoveAt(_expanded.Count - 1);
            }
            var component = (PATBoneWeightTransfer)target;
            for (int i = 0; i < _headers.Length; i++)
            {
                PATBoneWeightTransferRule rule = component.rules[i];
                Transform firstSource = null;
                int sourceCount = 0;
                if (rule.sources != null)
                {
                    foreach (Transform bone in rule.sources)
                    {
                        if (bone == null)
                        {
                            continue;
                        }
                        if (firstSource == null)
                        {
                            firstSource = bone;
                        }
                        sourceCount++;
                    }
                }
                bool descendants = rule.sourceMode == PATBoneWeightTransferSourceMode.Descendants;
                bool hasSource = descendants ? rule.sourceRoot != null : sourceCount > 0;
                if (rule.destination == null || !hasSource)
                {
                    _headers[i] = string.Format("規則 {0}: 未設定", i + 1);
                }
                else if (descendants)
                {
                    _headers[i] = string.Format("規則 {0}: {1} の子孫 → {2}（比率 {3:0.00}）",
                        i + 1, rule.sourceRoot.name, rule.destination.name, rule.ratio);
                }
                else if (sourceCount == 1)
                {
                    _headers[i] = string.Format("規則 {0}: {1} → {2}（比率 {3:0.00}）",
                        i + 1, firstSource.name, rule.destination.name, rule.ratio);
                }
                else
                {
                    _headers[i] = string.Format("規則 {0}: {1} ほか {2} 件 → {3}（比率 {4:0.00}）",
                        i + 1, firstSource.name, sourceCount - 1, rule.destination.name, rule.ratio);
                }
            }
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // NOTE: 貼り付けや Revert は Undo 通知なしで規則の数を変える。
            if (_headers.Length != _rules.arraySize || _expanded.Count != _rules.arraySize)
            {
                Refresh();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("上から順に処理します。", EditorStyles.miniLabel);
            _list.DoLayoutList();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("normalize"), Normalize);
            bool changed = EditorGUI.EndChangeCheck();
            bool applied = serializedObject.ApplyModifiedProperties();
            if (changed || applied)
            {
                Refresh();
            }
            if (!string.IsNullOrEmpty(_error))
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
            if (GUILayout.Button(Check))
            {
                _result = BoneWeightTransferProcessor.Process((PATBoneWeightTransfer)target, false);
            }
            if (_result != null)
            {
                if (_result.ErrorKey != null)
                {
                    EditorGUILayout.HelpBox(BoneWeightTransferErrors.InspectorText(_result.ErrorKey), MessageType.Error);
                }
                foreach (string warning in _result.RuleWarnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
                foreach (string line in _result.Lines)
                {
                    EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
                }
            }
            DrawVisual();
        }
        private void OnBendChanged()
        {
            _overlay.RefreshPositions();
            Repaint();
        }

        private void DrawVisual()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Visual, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "「曲げて確かめる」で転移元のボーンを回し、「移したあとの形で表示」を切り替えて見比べます。ON のときにメッシュが付いてこなければ、ウェイトが移っています。",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUI.BeginDisabledGroup(_validation.ErrorKey != null || EditorApplication.isPlayingOrWillChangePlaymode);
            bool vertices = EditorGUILayout.ToggleLeft(Vertices, _overlay.Enabled);
            if (vertices != _overlay.Enabled)
            {
                _overlay.SetEnabled(vertices);
            }
            if (_overlay.Enabled)
            {
                EditorGUILayout.LabelField(_overlay.CountLabel, EditorStyles.miniLabel);
            }
            bool blocked = _bend.Blocked;
            EditorGUI.BeginDisabledGroup(blocked);
            float angle = EditorGUILayout.Slider(Bend, _bend.Angle, 0f, 90f);
            if (angle != _bend.Angle)
            {
                _bend.SetAngle(angle);
            }
            EditorGUI.EndDisabledGroup();
            if (blocked)
            {
                EditorGUILayout.LabelField("Animation ウィンドウのプレビュー中は使えません。",
                    EditorStyles.wordWrappedMiniLabel);
            }
            bool after = BoneWeightTransferPreview.EnableNode.IsEnabled.Value;
            bool updated = EditorGUILayout.ToggleLeft(After, after);
            if (after != updated)
            {
                BoneWeightTransferPreview.EnableNode.IsEnabled.Value = updated;
                SceneView.RepaintAll();
            }
            EditorGUILayout.LabelField("NDMF のプレビューが有効なときに働きます。", EditorStyles.miniLabel);
            EditorGUI.EndDisabledGroup();
        }

        private void ApplyRules()
        {
            serializedObject.ApplyModifiedProperties();
            Refresh();
        }

        private void AddEmptyRule()
        {
            serializedObject.Update();
            int index = _rules.arraySize++;
            SerializedProperty rule = _rules.GetArrayElementAtIndex(index);
            rule.FindPropertyRelative("sourceMode").enumValueIndex = 0;
            rule.FindPropertyRelative("sources").ClearArray();
            rule.FindPropertyRelative("sourceRoot").objectReferenceValue = null;
            rule.FindPropertyRelative("includeSourceRoot").boolValue = false;
            rule.FindPropertyRelative("destination").objectReferenceValue = null;
            rule.FindPropertyRelative("ratio").floatValue = 1f;
            _expanded.Add(true);
            _list.index = index;
            ApplyRules();
        }

        private void DuplicateSelectedRule()
        {
            serializedObject.Update();
            int index = _list.index;
            if (index < 0 || index >= _rules.arraySize)
            {
                return;
            }
            _rules.InsertArrayElementAtIndex(index);
            _expanded.Insert(index + 1, true);
            _list.index = index + 1;
            ApplyRules();
        }

        private ReorderableList SourceList(SerializedProperty sources)
        {
            if (_sourceLists.TryGetValue(sources.propertyPath, out ReorderableList list))
            {
                return list;
            }
            list = new ReorderableList(serializedObject, sources, true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, Sources);
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                if (index < sources.arraySize)
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, sources.GetArrayElementAtIndex(index), GUIContent.none);
                }
            };
            list.onAddCallback = nested =>
            {
                int index = sources.arraySize++;
                sources.GetArrayElementAtIndex(index).objectReferenceValue = null;
            };
            _sourceLists.Add(sources.propertyPath, list);
            return list;
        }

        private float RuleHeight(int index)
        {
            if (index < 0 || index >= _headers.Length || index >= _expanded.Count ||
                index >= _rules.arraySize)
            {
                return 0f;
            }
            float line = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (!_expanded[index])
            {
                return line;
            }
            SerializedProperty rule = _rules.GetArrayElementAtIndex(index);
            float sourcesHeight = rule.FindPropertyRelative("sourceMode").enumValueIndex == 0
                ? SourceList(rule.FindPropertyRelative("sources")).GetHeight() +
                    EditorGUIUtility.standardVerticalSpacing
                : line * 2f;
            return line * 4f + sourcesHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private static Rect NextLine(ref Rect rect)
        {
            Rect line = rect;
            line.height = EditorGUIUtility.singleLineHeight;
            rect.y += line.height + EditorGUIUtility.standardVerticalSpacing;
            return line;
        }

        private void DrawRule(Rect rect, int index, bool active, bool focused)
        {
            if (index < 0 || index >= _headers.Length || index >= _expanded.Count ||
                index >= _rules.arraySize)
            {
                return;
            }
            _expanded[index] = EditorGUI.Foldout(NextLine(ref rect), _expanded[index], _headers[index], true);
            if (!_expanded[index])
            {
                return;
            }
            SerializedProperty rule = _rules.GetArrayElementAtIndex(index);
            SerializedProperty mode = rule.FindPropertyRelative("sourceMode");
            EditorGUI.PropertyField(NextLine(ref rect), mode, Mode);
            if (mode.enumValueIndex == 0)
            {
                ReorderableList sources = SourceList(rule.FindPropertyRelative("sources"));
                Rect listRect = rect;
                listRect.height = sources.GetHeight();
                sources.DoList(listRect);
                rect.y += listRect.height + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                EditorGUI.PropertyField(NextLine(ref rect), rule.FindPropertyRelative("sourceRoot"), SourceRoot);
                EditorGUI.PropertyField(NextLine(ref rect), rule.FindPropertyRelative("includeSourceRoot"), IncludeRoot);
            }
            EditorGUI.PropertyField(NextLine(ref rect), rule.FindPropertyRelative("destination"), Destination);
            EditorGUI.PropertyField(NextLine(ref rect), rule.FindPropertyRelative("ratio"), Ratio);
        }
    }
}
