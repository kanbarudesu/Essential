using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kanbarudesu.StatSystem.Editor
{
    public class StatsSystemEditorWindow : EditorWindow
    {
        // Define the tabs
        private enum ToolbarTab
        {
            UnitStats,
            StatusEffects,
            StatFormula,
            Identifiers_EnumEditor
        }
        private ToolbarTab currentTab = ToolbarTab.UnitStats;

        // Asset managers
        private UnitStatsAssetEditor unitStatsAssetEditor;
        private StatusEffectsAssetEditor statusEffectsAssetEditor;
        private IdentifiersAssetEditor identifiersAssetEditor;
        private StatFormulaAssetEditor statFormulaAssetEditor;
        private StatTypeEnumEditor statTypeEnumEditor;

        // Common UI state
        private Vector2 leftScroll;
        private string newAssetName = "";

        private ScrollView leftPanel, rightPanel;
        private ToolbarButton[] tabButtons;

        [MenuItem("Tools/Stats System Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<StatsSystemEditorWindow>("Stats System Editor");
            window.minSize = new Vector2(800, 500);
        }

        private void OnEnable()
        {
            unitStatsAssetEditor = new UnitStatsAssetEditor(EditorPrefs.GetString("UnitStatsFolder", "Assets/Data/UnitStats"), this);
            statusEffectsAssetEditor = new StatusEffectsAssetEditor(EditorPrefs.GetString("StatusEffectsFolder", "Assets/Data/StatusEffects"), this);
            identifiersAssetEditor = new IdentifiersAssetEditor(EditorPrefs.GetString("IdentifierFolder", "Assets/Data/StatusEffectIdentifiers"), this);
            statFormulaAssetEditor = new StatFormulaAssetEditor(EditorPrefs.GetString("StatFormulaFolder", "Assets/Data/StatFormulas"), this);
            statTypeEnumEditor = new StatTypeEnumEditor();

            RefreshAll();
        }

        private void OnDisable()
        {
            SaveFoldersLocation();
        }

        private void SaveFoldersLocation()
        {
            EditorPrefs.SetString("UnitStatsFolder", unitStatsAssetEditor.FolderPath);
            EditorPrefs.SetString("StatusEffectsFolder", statusEffectsAssetEditor.FolderPath);
            EditorPrefs.SetString("IdentifierFolder", identifiersAssetEditor.FolderPath);
            EditorPrefs.SetString("StatFormulaFolder", statFormulaAssetEditor.FolderPath);
        }

        private void RefreshAll()
        {
            unitStatsAssetEditor.RefreshAssets();
            statusEffectsAssetEditor.RefreshAssets();
            identifiersAssetEditor.RefreshAssets();
            statFormulaAssetEditor.RefreshAssets();
            statTypeEnumEditor.LoadStatTypeEnum();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var splitPanel = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            splitPanel.Q("unity-dragline-anchor").style.display = DisplayStyle.None;

            DrawToolbarButton(root);

            leftPanel = new ScrollView();
            leftPanel.style.minWidth = 250;
            leftPanel.style.marginTop = 5;
            leftPanel.style.marginLeft = 5;
            leftPanel.style.marginRight = 5;
            leftPanel.style.marginBottom = 5;
            leftPanel.Q("unity-content-container").style.flexGrow = 1;

            rightPanel = new ScrollView();
            rightPanel.style.minWidth = 500;
            rightPanel.style.marginTop = 5;
            rightPanel.style.marginLeft = 5;
            rightPanel.style.marginRight = 5;
            rightPanel.style.marginBottom = 5;
            rightPanel.Q("unity-content-container").style.flexGrow = 1;

            DrawPanelContent();

            splitPanel.Add(leftPanel);
            splitPanel.Add(rightPanel);

            root.Add(splitPanel);
        }

        private void DrawToolbarButton(VisualElement root)
        {
            tabButtons = new ToolbarButton[4];
            var toolbar = new Toolbar();
            toolbar.style.minWidth = 500;
            toolbar.style.flexGrow = 0;
            string[] tabNames = { "Unit Stats", "Status Effects", "Stat Formulas", "Identifiers & Enum Editor" };
            for (int i = 0; i < tabNames.Length; i++)
            {
                var index = i;
                tabButtons[i] = new ToolbarButton(() =>
                {
                    currentTab = (ToolbarTab)index;
                    DrawPanelContent();
                    OnToolbarButtonClicked(index);
                });
                tabButtons[i].style.unityFontStyleAndWeight = FontStyle.Bold;
                tabButtons[i].style.flexGrow = 1;
                tabButtons[i].text = tabNames[i];
                toolbar.Add(tabButtons[i]);
            }
            root.Add(toolbar);
        }

        private void OnToolbarButtonClicked(int index)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (i == index)
                {
                    tabButtons[i].style.backgroundColor = new StyleColor(new Color(0.482f, 0.682f, 0.980f));
                    tabButtons[i].style.color = new StyleColor(Color.black);
                }
                else
                {
                    tabButtons[i].style.backgroundColor = default;
                    tabButtons[i].style.color = new StyleColor(Color.white);
                }
            }
        }

        public void DrawPanelContent()
        {
            leftPanel.Clear();
            rightPanel.Clear();

            var leftPanelContainer = new IMGUIContainer(DrawLeftPanel);
            leftPanelContainer.style.flexGrow = 1;
            leftPanel.Add(leftPanelContainer);

            var drawRightPanel = DrawRightPanel();
            drawRightPanel.style.flexGrow = 1;
            rightPanel.Add(drawRightPanel);
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250));
            leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

            switch (currentTab)
            {
                case ToolbarTab.UnitStats:
                    EditorGUILayout.LabelField("Unit Stats", EditorStyles.boldLabel);
                    unitStatsAssetEditor.DrawAssetList();
                    break;
                case ToolbarTab.StatusEffects:
                    EditorGUILayout.LabelField("Status Effects", EditorStyles.boldLabel);
                    statusEffectsAssetEditor.DrawAssetList(() => { DrawPanelContent(); });
                    break;
                case ToolbarTab.StatFormula:
                    EditorGUILayout.LabelField("Stat Formulas", EditorStyles.boldLabel);
                    statFormulaAssetEditor.DrawAssetList(() => { DrawPanelContent(); });
                    break;
                case ToolbarTab.Identifiers_EnumEditor:
                    EditorGUILayout.LabelField("Status Effect Identifiers", EditorStyles.boldLabel);
                    identifiersAssetEditor.DrawAssetList();
                    break;
            }

            EditorGUILayout.EndScrollView();

            string assetName = "";
            IAssetEditor currentManager = null;

            switch (currentTab)
            {
                case ToolbarTab.UnitStats:
                    assetName = "Unit Stats";
                    currentManager = unitStatsAssetEditor;
                    break;
                case ToolbarTab.StatusEffects:
                    assetName = "Status Effect";
                    currentManager = statusEffectsAssetEditor;
                    break;
                case ToolbarTab.StatFormula:
                    assetName = "Stat Formula";
                    currentManager = statFormulaAssetEditor;
                    break;
                case ToolbarTab.Identifiers_EnumEditor:
                    assetName = "Status Effect Identifier";
                    currentManager = identifiersAssetEditor;
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Create New " + assetName, EditorStyles.boldLabel);

            if (currentManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 80;
                GUIContent label = new GUIContent("Save Folder", currentManager.FolderPath);
                EditorGUILayout.LabelField(label, GUILayout.Width(80));

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(currentManager.FolderPath);
                EditorGUI.EndDisabledGroup();

                GUIContent folderIcon = EditorGUIUtility.IconContent("Folder On Icon");
                EditorGUIUtility.SetIconSize(new Vector2(15, 15));
                if (GUILayout.Button(folderIcon, GUILayout.Width(30)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        string selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", currentManager.FolderPath, "");
                        if (!string.IsNullOrEmpty(selectedFolder) && selectedFolder.StartsWith(Application.dataPath))
                        {
                            string relativePath = "Assets" + selectedFolder.Substring(Application.dataPath.Length);
                            currentManager.FolderPath = relativePath;
                        }
                    };
                }
                EditorGUILayout.EndHorizontal();

                newAssetName = EditorGUILayout.TextField("Asset Name", newAssetName);

                GUIContent createNewIcon = EditorGUIUtility.IconContent("d_Toolbar Plus");
                createNewIcon.text = "Create New";
                if (GUILayout.Button(createNewIcon, GUILayout.Height(25)))
                {
                    if (string.IsNullOrWhiteSpace(newAssetName))
                    {
                        EditorUtility.DisplayDialog("Invalid Name", "Asset name cannot be empty.", "OK");
                    }
                    else
                    {
                        if (currentManager.CreateNewAsset(newAssetName))
                        {
                            newAssetName = "";
                            RefreshAll();
                            DrawPanelContent();
                            Repaint();
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = -1; // Reset
        }

        private VisualElement DrawRightPanel()
        {
            var rightPanel = new Box();

            switch (currentTab)
            {
                case ToolbarTab.UnitStats:
                    rightPanel.Add(new IMGUIContainer(unitStatsAssetEditor.DrawInspector));
                    break;
                case ToolbarTab.StatusEffects:
                    rightPanel.Add(statusEffectsAssetEditor.DrawInspectorVE());
                    break;
                case ToolbarTab.StatFormula:
                    rightPanel.Add(statFormulaAssetEditor.DrawInspectorVE());
                    break;
                case ToolbarTab.Identifiers_EnumEditor:
                    rightPanel.Add(new IMGUIContainer(statTypeEnumEditor.DrawEnumEditorUI));
                    break;
            }
            return rightPanel;
        }
    }
}