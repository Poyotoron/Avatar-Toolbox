using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    [CustomEditor(typeof(PATAnimationHelper))]
    internal sealed class PATAnimationHelperEditor : UnityEditor.Editor
    {
        private static readonly GUIContent Menu = new GUIContent("メニュー");
        private static readonly GUIContent Parameters = new GUIContent("パラメーター");
        private static readonly GUIContent Toggle = new GUIContent("トグル");
        private static readonly GUIContent Entries = new GUIContent("動かすもの");
        private static readonly GUIContent Mode = new GUIContent("種類");
        private static readonly GUIContent Name = new GUIContent("名前");
        private static readonly GUIContent Icon = new GUIContent("アイコン");
        private static readonly GUIContent Parent = new GUIContent("メニューの挿入先");
        private static readonly GUIContent Saved = new GUIContent("値を保存する");
        private static readonly GUIContent Synced = new GUIContent("他の人に同期する");
        private static readonly GUIContent Default = new GUIContent("初期状態");
        private static readonly GUIContent Duration = new GUIContent("時間（秒）");
        private static readonly GUIContent Easing = new GUIContent("動き方");
        private static readonly GUIContent Reverse = new GUIContent("逆も有効");
        private static readonly GUIContent Target = new GUIContent("対象");
        private static readonly GUIContent Property = new GUIContent("動かすもの");
        private static readonly GUIContent Shape = new GUIContent("ブレンドシェイプ");
        private static readonly GUIContent Start = new GUIContent("開始値");
        private static readonly GUIContent End = new GUIContent("終了値");
        private static readonly GUIContent Min = new GUIContent("下限");
        private static readonly GUIContent Max = new GUIContent("上限");
        private static readonly GUIContent Initial = new GUIContent("初期値");
        private static readonly GUIContent Current = new GUIContent("今の値");
        private static readonly GUIContent[] DefaultOptions = { new GUIContent("OFF"), new GUIContent("ON") };
        private const string ParentHint = "空ならメニューの最上位に入ります。サブメニューの MA Menu Item（子のメニューを使う設定）を入れると、その中に入ります。ビルド時に MA Menu Item（挿入先が空なら MA Menu Installer も）を自動で付けます。";
        private const string ParameterHint = "空なら自動で決めます。";
        private const string SavedHint = "保存した値で始まるとき、読み込んだ直後に 1 回動くことがあります。";
        private const string ReverseHint = "OFF にしたとき、逆向きに動かします。";
        private const string RadialHint = "パラメーターの初期値は、先頭の行の初期値で決まります。";
        private readonly List<AnimationHelperIssue> _issues = new List<AnimationHelperIssue>();
        private readonly List<ShapeOptions> _shapes = new List<ShapeOptions>();
        private string[] _headers;
        private string _objectName;
        private float _currentWidth;
        private SerializedProperty _entries;
        private ReorderableList _list;
        private int _reset = -1;

        private sealed class ShapeOptions
        {
            internal Transform Target;
            internal Mesh Mesh;
            internal string MissingName;
            internal GUIContent[] Labels;
            internal string[] Names;
            internal int Selected;
        }

        private void OnEnable()
        {
            _entries = serializedObject.FindProperty("entries");
            _list = new ReorderableList(serializedObject, _entries, true, true, true, true);
            _list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, Entries);
            _list.drawElementCallback = DrawEntry;
            _list.elementHeightCallback = EntryHeight;
            _list.onAddCallback = list =>
            {
                serializedObject.ApplyModifiedProperties();
                AddEntry((PATAnimationHelper)target);
                Refresh();
            };
            _list.onRemoveCallback = list =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.ApplyModifiedProperties();
                Refresh();
            };
            _list.onReorderCallback = list =>
            {
                serializedObject.ApplyModifiedProperties();
                Refresh();
            };
            Undo.undoRedoPerformed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Refresh;
        }

        private void Refresh()
        {
            if (target == null)
            {
                return;
            }
            serializedObject.Update();
            var component = (PATAnimationHelper)target;
            _objectName = component.gameObject.name;
            _headers = new string[component.entries.Count];
            while (_shapes.Count < component.entries.Count)
            {
                _shapes.Add(null);
            }
            while (_shapes.Count > component.entries.Count)
            {
                _shapes.RemoveAt(_shapes.Count - 1);
            }
            for (int i = 0; i < component.entries.Count; i++)
            {
                _headers[i] = string.Format("行 {0}", i + 1);
                PATAnimationHelperEntry entry = component.entries[i];
                SkinnedMeshRenderer renderer = entry.target != null
                    ? entry.target.GetComponent<SkinnedMeshRenderer>() : null;
                Mesh mesh = renderer != null ? renderer.sharedMesh : null;
                bool missing = mesh == null || mesh.GetBlendShapeIndex(entry.blendShapeName) < 0;
                string missingName = missing ? entry.blendShapeName : null;
                ShapeOptions cache = _shapes[i];
                if (cache == null || cache.Target != entry.target || cache.Mesh != mesh ||
                    cache.MissingName != missingName)
                {
                    cache = new ShapeOptions { Target = entry.target, Mesh = mesh, MissingName = missingName };
                    int offset = missing ? 1 : 0;
                    int count = mesh != null ? mesh.blendShapeCount : 0;
                    cache.Labels = new GUIContent[count + offset];
                    cache.Names = new string[count + offset];
                    if (missing)
                    {
                        cache.Labels[0] = new GUIContent("（見つかりません）" + entry.blendShapeName);
                        cache.Names[0] = entry.blendShapeName;
                    }
                    for (int j = 0; j < count; j++)
                    {
                        cache.Names[j + offset] = mesh.GetBlendShapeName(j);
                        cache.Labels[j + offset] = new GUIContent(cache.Names[j + offset]);
                    }
                    _shapes[i] = cache;
                }
                cache.Selected = System.Array.IndexOf(cache.Names, entry.blendShapeName);
            }
            VRCAvatarDescriptor descriptor = component.GetComponentInParent<VRCAvatarDescriptor>(true);
            AnimationHelperValidator.Validate(component, descriptor != null ? descriptor.transform : null, _issues);
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // NOTE: 貼り付けや Prefab の Revert は Undo 通知なしでも行数を変える。
            if (_headers.Length != _entries.arraySize || _shapes.Count != _entries.arraySize)
            {
                Refresh();
            }
            if (_currentWidth <= 0f)
            {
                _currentWidth = GUI.skin.button.CalcSize(Current).x + 8f;
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(Menu, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"), Mode);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("menuName"), Name);
            if (string.IsNullOrEmpty(serializedObject.FindProperty("menuName").stringValue))
            {
                EditorGUILayout.LabelField(_objectName, EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("menuIcon"), Icon);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("menuParent"), Parent);
            EditorGUILayout.LabelField(ParentHint, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Parameters, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("parameterName"), Name);
            if (string.IsNullOrEmpty(serializedObject.FindProperty("parameterName").stringValue))
            {
                EditorGUILayout.LabelField(ParameterHint, EditorStyles.miniLabel);
            }
            bool radial = serializedObject.FindProperty("mode").enumValueIndex == 1;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("parameterSaved"), Saved);
            if (!radial)
            {
                EditorGUILayout.LabelField(SavedHint, EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("parameterSynced"), Synced);
            if (!radial)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Toggle, EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(Default);
                SerializedProperty defaultOn = serializedObject.FindProperty("toggleDefaultOn");
                defaultOn.boolValue = GUILayout.Toolbar(defaultOn.boolValue ? 1 : 0, DefaultOptions) == 1;
                EditorGUILayout.EndHorizontal();
                SerializedProperty duration = serializedObject.FindProperty("toggleDuration");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(duration, Duration);
                if (EditorGUI.EndChangeCheck())
                {
                    duration.floatValue = Mathf.Clamp(duration.floatValue, 0.01f, 10f);
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toggleEasing"), Easing);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toggleReverse"), Reverse);
                EditorGUILayout.LabelField(ReverseHint, EditorStyles.miniLabel);
            }
            EditorGUILayout.Space();
            if (radial)
            {
                EditorGUILayout.LabelField(RadialHint, EditorStyles.wordWrappedMiniLabel);
            }
            _reset = -1;
            _list.DoLayoutList();
            bool changed = EditorGUI.EndChangeCheck();
            bool applied = serializedObject.ApplyModifiedProperties();
            if (_reset >= 0 && _reset < _entries.arraySize)
            {
                var component = (PATAnimationHelper)target;
                Undo.RecordObject(component, "PAT Animation Helper");
                AnimationHelperValues.ApplyDefaults(component.entries[_reset]);
                EditorUtility.SetDirty(component);
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                serializedObject.Update();
            }
            if (changed || applied || _reset >= 0)
            {
                Refresh();
            }
            foreach (AnimationHelperIssue issue in _issues)
            {
                EditorGUILayout.HelpBox(issue.Message, issue.IsError ? MessageType.Error : MessageType.Warning);
            }
        }

        private void AddEntry(PATAnimationHelper component)
        {
            Undo.RecordObject(component, "PAT Animation Helper");
            var entry = new PATAnimationHelperEntry { target = component.transform };
            AnimationHelperValues.ApplyDefaults(entry);
            component.entries.Add(entry);
            EditorUtility.SetDirty(component);
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            serializedObject.Update();
        }

        private float EntryHeight(int index)
        {
            if (index < 0 || index >= _headers.Length || index >= _shapes.Count ||
                index >= _entries.arraySize)
            {
                return 0f;
            }
            SerializedProperty entry = _entries.GetArrayElementAtIndex(index);
            bool shape = entry.FindPropertyRelative("property").enumValueIndex ==
                (int)PATAnimationHelperProperty.BlendShape;
            bool radial = serializedObject.FindProperty("mode").enumValueIndex == 1;
            float line = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float height = line * 3f;
            if (shape)
            {
                height += line * (_shapes[index].Mesh == null ? 2f : 1f);
            }
            height += ValueHeight(entry, shape, radial ? "minVector" : "startVector", radial ? Min : Start);
            height += ValueHeight(entry, shape, radial ? "maxVector" : "endVector", radial ? Max : End);
            if (radial)
            {
                height += ValueHeight(entry, shape, "initialVector", Initial);
            }
            return height + EditorGUIUtility.standardVerticalSpacing * 2f;
        }

        private static float ValueHeight(SerializedProperty entry, bool shape, string field, GUIContent label)
        {
            return (shape ? EditorGUIUtility.singleLineHeight :
                EditorGUI.GetPropertyHeight(entry.FindPropertyRelative(field), label)) +
                EditorGUIUtility.standardVerticalSpacing;
        }

        private static Rect NextLine(ref Rect rect, float height)
        {
            Rect line = rect;
            line.height = height;
            rect.y += height + EditorGUIUtility.standardVerticalSpacing;
            return line;
        }

        private void DrawEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            // NOTE: 描画中の貼り付けでも、古いキャッシュを添字で参照しない。
            if (index < 0 || index >= _headers.Length || index >= _shapes.Count ||
                index >= _entries.arraySize)
            {
                return;
            }
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            float line = EditorGUIUtility.singleLineHeight;
            SerializedProperty entry = _entries.GetArrayElementAtIndex(index);
            EditorGUI.LabelField(NextLine(ref rect, line), _headers[index], EditorStyles.boldLabel);
            EditorGUI.PropertyField(NextLine(ref rect, line), entry.FindPropertyRelative("target"), Target);
            SerializedProperty property = entry.FindPropertyRelative("property");
            int previous = property.enumValueIndex;
            EditorGUI.PropertyField(NextLine(ref rect, line), property, Property);
            if (previous != property.enumValueIndex)
            {
                _reset = index;
            }
            bool shape = property.enumValueIndex == (int)PATAnimationHelperProperty.BlendShape;
            if (shape)
            {
                ShapeOptions cache = _shapes[index];
                if (cache.Mesh == null)
                {
                    EditorGUI.HelpBox(NextLine(ref rect, line), "対象にメッシュがありません。", MessageType.Info);
                }
                int selected = EditorGUI.Popup(NextLine(ref rect, line), Shape, cache.Selected, cache.Labels);
                if (selected >= 0 && selected < cache.Names.Length)
                {
                    entry.FindPropertyRelative("blendShapeName").stringValue = cache.Names[selected];
                }
            }
            if (serializedObject.FindProperty("mode").enumValueIndex == 1)
            {
                DrawValue(ref rect, entry, shape, "minVector", "minWeight", Min, false);
                DrawValue(ref rect, entry, shape, "maxVector", "maxWeight", Max, false);
                DrawValue(ref rect, entry, shape, "initialVector", "initialWeight", Initial, true);
            }
            else
            {
                DrawValue(ref rect, entry, shape, "startVector", "startWeight", Start, true);
                DrawValue(ref rect, entry, shape, "endVector", "endWeight", End, true);
            }
        }

        private void DrawValue(ref Rect rect, SerializedProperty entry, bool shape, string vectorField,
            string weightField, GUIContent label, bool currentButton)
        {
            SerializedProperty value = entry.FindPropertyRelative(shape ? weightField : vectorField);
            float height = shape ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(value, label);
            Rect field = NextLine(ref rect, height);
            Rect button = field;
            if (currentButton)
            {
                field.width -= _currentWidth + 4f;
                button.x = field.xMax + 4f;
                button.width = _currentWidth;
                button.height = EditorGUIUtility.singleLineHeight;
            }
            EditorGUI.PropertyField(field, value, label);
            if (currentButton && GUI.Button(button, Current))
            {
                Transform entryTarget = entry.FindPropertyRelative("target").objectReferenceValue as Transform;
                if (shape)
                {
                    AnimationHelperValues.TryReadWeight(entryTarget,
                        entry.FindPropertyRelative("blendShapeName").stringValue, out float weight);
                    value.floatValue = weight;
                }
                else
                {
                    value.vector3Value = AnimationHelperValues.ReadVector(entryTarget,
                        (PATAnimationHelperProperty)entry.FindPropertyRelative("property").enumValueIndex);
                }
            }
        }
    }
}
