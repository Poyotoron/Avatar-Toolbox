using System;
using UnityEditor;
using UnityEngine;

namespace Poyo.AvatarToolbox.Editor
{
    internal sealed class AvatarToolboxWindow : EditorWindow
    {
        private static readonly string[] ToolSymbols = CreateToolSymbols();
        private static readonly GUIContent[] ToolNameContents = CreateToolNameContents();
        private static readonly GUIContent ComponentsContent = new GUIContent("コンポーネント");
        private static readonly GUIContent AddComponentContent =
            new GUIContent("Add Component から追加します");
        private static readonly GUIContent HeaderContent =
            new GUIContent(AvatarToolboxInfo.DisplayName + "  v" + AvatarToolboxInfo.Version);
        private static readonly GUIContent RecompileNoticeContent = new GUIContent(
            "ツールの切り替えを適用するとスクリプトが再コンパイルされます。");
        private static readonly GUIContent PendingNoticeContent = new GUIContent(
            "未適用の変更があります。適用するとスクリプトが再コンパイルされます。");
        private static readonly GUIContent CompilingNoticeContent = new GUIContent(
            "スクリプトをコンパイルしています。完了するまで設定を変更できません。");
        private static readonly GUIContent PendingMarkerContent = new GUIContent("*");
        private static readonly GUIContent ApplyContent = new GUIContent("適用");
        private static readonly GUIContent DiscardContent = new GUIContent("破棄");
        private static readonly GUIContent DisabledButtonContent = new GUIContent("無効です");
        private static readonly GUIContent CompilingButtonContent = new GUIContent("コンパイル中…");
        private static readonly GUIContent OpenButtonContent = new GUIContent("開く");
        private static readonly GUIContent UnavailableMarkerContent = new GUIContent("(未導入)");
        private static readonly GUIContent UnavailableButtonContent =
            new GUIContent("必要なパッケージがありません");
        private static readonly GUIContent SelectionMarkerContent = new GUIContent("▶");
        private static readonly GUIContent DependencyHeaderContent =
            new GUIContent("必要なパッケージ");
        private static readonly Color SelectedRowColor =
            new Color(0.45f, 0.65f, 1f, 1f);
        private const float DetailHeight = 96f;
        private const float ButtonHeight = 24f;
        private const string NoDependencyLabel = "なし（追加の導入は不要です）";
        private const string NotInstalledFormat =
            "{0} が見つかりません。導入すると有効化できます。";

        [SerializeField] private string _selectedToolId;
        [SerializeField] private bool[] _pendingEnabled;
        [SerializeField] private Vector2 _listScroll;
        [SerializeField] private Vector2 _detailScroll;
        private bool[] _actualEnabled;
        private bool _wasCompiling;
        private float _dependencyHeaderWidth;
        private string _selectedDependencyWarning = string.Empty;
        private string _deprecatedWarning = string.Empty;
        private string _componentInstructions = string.Empty;
        private readonly float[] _toolNameWidths = new float[AvatarToolboxToolCatalog.Tools.Length];

        [MenuItem(AvatarToolboxInfo.MenuPath, false, AvatarToolboxInfo.MenuPriority)]
        private static void Open()
        {
            var window = GetWindow<AvatarToolboxWindow>(
                false, AvatarToolboxInfo.DisplayName, true);
            window.minSize = new Vector2(520f, 600f);
            window.Show();
        }

        private void OnEnable()
        {
            _wasCompiling = EditorApplication.isCompiling;
            RefreshActual();
            if (AvatarToolboxToolCatalog.Find(_selectedToolId) == null &&
                AvatarToolboxComponentCatalog.Find(_selectedToolId) == null &&
                AvatarToolboxToolCatalog.Tools.Length > 0)
            {
                _selectedToolId = AvatarToolboxToolCatalog.Tools[0].Id;
            }

            RefreshSelectedToolDetails();
        }

        private void Update()
        {
            bool isCompiling = EditorApplication.isCompiling;
            if (isCompiling == _wasCompiling)
            {
                return;
            }

            bool compilationFinished = _wasCompiling && !isCompiling;
            _wasCompiling = isCompiling;
            if (compilationFinished)
            {
                RefreshActual();
                Array.Copy(_actualEnabled, _pendingEnabled, _actualEnabled.Length);
            }

            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(HeaderContent, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(RecompileNoticeContent, EditorStyles.miniLabel);

            _listScroll = EditorGUILayout.BeginScrollView(
                _listScroll, GUILayout.ExpandHeight(true));
            DrawToolList();
            DrawComponentList();
            EditorGUILayout.EndScrollView();

            int selectedIndex = FindToolIndex(_selectedToolId);
            if (selectedIndex >= 0)
            {
                EditorGUILayout.LabelField(
                    AvatarToolboxToolCatalog.Tools[selectedIndex].DisplayName,
                    EditorStyles.boldLabel);
            }
            else
            {
                AvatarToolboxComponentEntry component = AvatarToolboxComponentCatalog.Find(_selectedToolId);
                EditorGUILayout.LabelField(
                    component != null ? component.DisplayName : string.Empty, EditorStyles.boldLabel);
            }

            _detailScroll = EditorGUILayout.BeginScrollView(
                _detailScroll, GUILayout.Height(DetailHeight));
            DrawSelectedToolDetail(selectedIndex);
            EditorGUILayout.EndScrollView();

            DrawOperationControls(selectedIndex);
        }

        private void DrawToolList()
        {
            Event currentEvent = Event.current;

            for (int i = 0; i < AvatarToolboxToolCatalog.Tools.Length; i++)
            {
                AvatarToolboxToolEntry tool = AvatarToolboxToolCatalog.Tools[i];
                bool isSelected = string.Equals(
                    _selectedToolId, tool.Id, StringComparison.Ordinal);
                Color previousBackground = GUI.backgroundColor;
                if (isSelected)
                {
                    GUI.backgroundColor = SelectedRowColor;
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.backgroundColor = previousBackground;

                if (isSelected)
                {
                    EditorGUILayout.LabelField(
                        SelectionMarkerContent, GUILayout.Width(14f));
                }
                else
                {
                    EditorGUILayout.LabelField(
                        GUIContent.none, GUILayout.Width(14f));
                }

                // NOTE: コンパイル完了時に実際の設定で入力を更新するため、
                //       処理中の編集を許すと利用者の変更が黙って失われてしまう。
                EditorGUI.BeginDisabledGroup(!tool.IsAvailable || IsBusy);
                _pendingEnabled[i] = EditorGUILayout.Toggle(
                    _pendingEnabled[i], GUILayout.Width(18f));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.LabelField(tool.DisplayName, EditorStyles.boldLabel);
                Rect nameRect = GUILayoutUtility.GetLastRect();
                if (_toolNameWidths[i] <= 0f)
                {
                    _toolNameWidths[i] = EditorStyles.boldLabel.CalcSize(ToolNameContents[i]).x;
                }
                // NOTE: IMGUI のリッチテキストには取り消し線がないため、文字幅で線を描く。
                if (tool.IsDeprecated && currentEvent.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(
                        new Rect(nameRect.x, nameRect.y + nameRect.height * 0.5f,
                            Mathf.Min(_toolNameWidths[i], nameRect.width), 1f),
                        EditorStyles.boldLabel.normal.textColor);
                }
                if (!tool.IsAvailable)
                {
                    EditorGUILayout.LabelField(
                        UnavailableMarkerContent, GUILayout.Width(54f));
                }
                if (_pendingEnabled[i] != _actualEnabled[i])
                {
                    EditorGUILayout.LabelField(
                        PendingMarkerContent, GUILayout.Width(12f));
                }
                EditorGUILayout.LabelField(tool.Summary, EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
                Rect rowRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(rowRect, MouseCursor.Link);
                if (currentEvent.type == EventType.MouseDown &&
                    rowRect.Contains(currentEvent.mousePosition))
                {
                    if (!string.Equals(
                            _selectedToolId, tool.Id, StringComparison.Ordinal))
                    {
                        _selectedToolId = tool.Id;
                        RefreshSelectedToolDetails();
                    }

                    currentEvent.Use();
                    Repaint();
                }
            }
        }

        private void DrawOperationControls(int selectedIndex)
        {
            bool hasPendingChanges = HasPendingChanges();
            Action opener = null;
            GUIContent openButtonContent = DisabledButtonContent;
            bool canOpen = false;
            if (selectedIndex >= 0)
            {
                AvatarToolboxToolEntry selectedTool = AvatarToolboxToolCatalog.Tools[selectedIndex];
                bool isEnabled = _actualEnabled[selectedIndex];
                bool isRegistered = AvatarToolboxToolRegistry.TryGetOpener(
                    selectedTool.Id, out opener);

                if (!selectedTool.IsAvailable)
                {
                    openButtonContent = UnavailableButtonContent;
                }
                else if (!isEnabled)
                {
                    openButtonContent = DisabledButtonContent;
                }
                else if (IsBusy || !isRegistered)
                {
                    openButtonContent = CompilingButtonContent;
                }
                else
                {
                    openButtonContent = OpenButtonContent;
                    canOpen = true;
                }
            }

            if (AvatarToolboxComponentCatalog.Find(_selectedToolId) != null)
            {
                openButtonContent = AddComponentContent;
            }
            EditorGUI.BeginDisabledGroup(!canOpen);
            bool openPressed = GUILayout.Button(
                openButtonContent, GUILayout.Height(ButtonHeight));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(4f);
            if (IsBusy)
            {
                EditorGUILayout.HelpBox(
                    CompilingNoticeContent.text, MessageType.Info);
            }
            else if (hasPendingChanges)
            {
                EditorGUILayout.HelpBox(PendingNoticeContent.text, MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!hasPendingChanges || IsBusy);
            bool applyPressed = GUILayout.Button(
                IsBusy ? CompilingButtonContent : ApplyContent,
                GUILayout.Height(ButtonHeight));
            bool discardPressed = GUILayout.Button(
                DiscardContent, GUILayout.Height(ButtonHeight));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (openPressed)
            {
                opener();
            }
            else if (applyPressed)
            {
                ApplyPendingChanges();
            }
            else if (discardPressed)
            {
                Array.Copy(_actualEnabled, _pendingEnabled, _actualEnabled.Length);
            }
        }

        private void DrawSelectedToolDetail(int selectedIndex)
        {
            AvatarToolboxComponentEntry component = AvatarToolboxComponentCatalog.Find(_selectedToolId);
            if (!string.IsNullOrEmpty(_deprecatedWarning))
            {
                EditorGUILayout.HelpBox(_deprecatedWarning, MessageType.Warning);
            }
            if (selectedIndex < 0)
            {
                if (component != null)
                {
                    EditorGUILayout.LabelField(component.Description, EditorStyles.wordWrappedLabel);
                    DrawRequirement(component.RequirementLabel);
                    EditorGUILayout.LabelField(_componentInstructions, EditorStyles.wordWrappedLabel);
                    DrawDependencyWarning();
                }
                return;
            }

            AvatarToolboxToolEntry selectedTool = AvatarToolboxToolCatalog.Tools[selectedIndex];
            EditorGUILayout.LabelField(
                selectedTool.Description, EditorStyles.wordWrappedLabel);
            DrawRequirement(selectedTool.RequirementLabel);
            DrawDependencyWarning();
        }

        private void DrawRequirement(string requirement)
        {
            if (_dependencyHeaderWidth <= 0f)
            {
                // NOTE: GUI コンテキスト外では計測できないため、最初の描画時だけ求める。
                _dependencyHeaderWidth =
                    EditorStyles.label.CalcSize(DependencyHeaderContent).x + 4f;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                DependencyHeaderContent, GUILayout.Width(_dependencyHeaderWidth));
            EditorGUILayout.LabelField(
                string.IsNullOrEmpty(requirement)
                    ? NoDependencyLabel
                    : requirement);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDependencyWarning()
        {
            if (!string.IsNullOrEmpty(_selectedDependencyWarning))
            {
                EditorGUILayout.HelpBox(
                    _selectedDependencyWarning, MessageType.Warning);
            }
        }

        private void RefreshActual()
        {
            int toolCount = AvatarToolboxToolCatalog.Tools.Length;
            if (_actualEnabled == null || _actualEnabled.Length != toolCount)
            {
                _actualEnabled = new bool[toolCount];
            }

            bool initializePending =
                _pendingEnabled == null || _pendingEnabled.Length != toolCount;
            if (initializePending)
            {
                _pendingEnabled = new bool[toolCount];
            }

            AvatarToolboxDefines.GetDefined(ToolSymbols, _actualEnabled);
            if (initializePending)
            {
                Array.Copy(_actualEnabled, _pendingEnabled, toolCount);
            }

            // NOTE: 要件を満たさない行は操作できないため、解消不能な保留差分を残さない。
            for (int toolIndex = 0; toolIndex < toolCount; toolIndex++)
            {
                if (!AvatarToolboxToolCatalog.Tools[toolIndex].IsAvailable)
                {
                    _pendingEnabled[toolIndex] = _actualEnabled[toolIndex];
                }
            }
        }

        private void RefreshSelectedToolDetails()
        {
            AvatarToolboxToolEntry selectedTool =
                AvatarToolboxToolCatalog.Find(_selectedToolId);
            AvatarToolboxComponentEntry component = AvatarToolboxComponentCatalog.Find(_selectedToolId);
            _deprecatedWarning = selectedTool != null && selectedTool.IsDeprecated
                ? string.Format("このツールは非推奨です。代わりに {0} を使ってください。", selectedTool.ReplacementLabel)
                : string.Empty;
            _componentInstructions = component != null
                ? string.Format("コンポーネントは切り替えずに使えます。アバターの GameObject を選び、Add Component の「Poyo's Avatar Toolbox」から {0} を追加します。", component.ComponentName)
                : string.Empty;
            if (component != null)
            {
                _selectedDependencyWarning = !component.IsAvailable
                    ? string.Format(NotInstalledFormat, component.RequirementLabel) : string.Empty;
                return;
            }
            _selectedDependencyWarning =
                selectedTool != null &&
                !selectedTool.IsAvailable &&
                !string.IsNullOrEmpty(selectedTool.RequirementLabel)
                    ? string.Format(
                        NotInstalledFormat, selectedTool.RequirementLabel)
                    : string.Empty;
        }

        private void DrawComponentList()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(ComponentsContent, EditorStyles.boldLabel);
            foreach (AvatarToolboxComponentEntry entry in AvatarToolboxComponentCatalog.Components)
            {
                bool selected = _selectedToolId == entry.Id;
                Color previous = GUI.backgroundColor;
                if (selected)
                {
                    GUI.backgroundColor = SelectedRowColor;
                }
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.backgroundColor = previous;
                EditorGUILayout.LabelField(selected ? SelectionMarkerContent : GUIContent.none,
                    GUILayout.Width(14f));
                // NOTE: 型を無効化すると保存済みのシーンや Prefab が Missing Script になる。
                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(18f));
                EditorGUILayout.LabelField(entry.DisplayName, EditorStyles.boldLabel);
                if (!entry.IsAvailable)
                {
                    EditorGUILayout.LabelField(UnavailableMarkerContent, GUILayout.Width(54f));
                }
                EditorGUILayout.LabelField(entry.Summary, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                Rect row = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(row, MouseCursor.Link);
                if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition))
                {
                    _selectedToolId = entry.Id;
                    RefreshSelectedToolDetails();
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        private static GUIContent[] CreateToolNameContents()
        {
            var contents = new GUIContent[AvatarToolboxToolCatalog.Tools.Length];
            for (int i = 0; i < contents.Length; i++)
            {
                contents[i] = new GUIContent(AvatarToolboxToolCatalog.Tools[i].DisplayName);
            }
            return contents;
        }

        private bool HasPendingChanges()
        {
            for (int i = 0; i < _actualEnabled.Length; i++)
            {
                if (_actualEnabled[i] != _pendingEnabled[i])
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyPendingChanges()
        {
            // NOTE: 描画後にコンパイルが始まる可能性もあるため、ボタンの状態だけに
            //       頼らず、設定を書き込む直前にも処理中でないことを確認する。
            if (IsBusy)
            {
                return;
            }

            int changeCount = 0;
            for (int i = 0; i < _actualEnabled.Length; i++)
            {
                if (_actualEnabled[i] != _pendingEnabled[i])
                {
                    changeCount++;
                }
            }

            var symbols = new string[changeCount];
            var enabled = new bool[changeCount];
            int destinationIndex = 0;
            for (int i = 0; i < _actualEnabled.Length; i++)
            {
                if (_actualEnabled[i] == _pendingEnabled[i])
                {
                    continue;
                }

                symbols[destinationIndex] = ToolSymbols[i];
                enabled[destinationIndex] = _pendingEnabled[i];
                destinationIndex++;
            }

            // NOTE: シンボルの書き換えによる再コンパイルで GUI のレイアウト処理を
            //       中断しないよう、描画が完了してから変更する。
            EditorApplication.delayCall += () =>
                AvatarToolboxDefines.SetDefined(symbols, enabled);
        }

        private static int FindToolIndex(string toolId)
        {
            for (int i = 0; i < AvatarToolboxToolCatalog.Tools.Length; i++)
            {
                if (AvatarToolboxToolCatalog.Tools[i].Id == toolId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static string[] CreateToolSymbols()
        {
            var symbols = new string[AvatarToolboxToolCatalog.Tools.Length];
            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i] = AvatarToolboxToolCatalog.Tools[i].DefineSymbol;
            }

            return symbols;
        }

        // NOTE: 判定を分散させると、表示側だけが操作可能なのに書き込み側では
        //       拒否される状態が生まれるため、処理中の判定を 1 か所にまとめる。
        private static bool IsBusy
        {
            get
            {
                return EditorApplication.isCompiling || EditorApplication.isUpdating;
            }
        }
    }
}
