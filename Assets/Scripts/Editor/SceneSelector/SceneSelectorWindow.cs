using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

namespace Kanbarudesu.Editor.Utility
{
    public class SceneSelectorWindow : EditorWindow
    {
        private ReorderableList editSceneList;
        private ReorderableList visibleSceneList;

        private SerializedObject serializedObject;
        private SerializedProperty sceneDataProperty;
        private SerializedProperty visibleSceneDataProperty;

        private EditorSceneDataSO sceneData;
        private bool isEditing;

        private Vector2 scrollPos;

        [MenuItem("Essential/SceneSelector")]
        private static void ShowWindow()
        {
            var window = GetWindow<SceneSelectorWindow>();
            window.titleContent = new GUIContent("SceneSelector");
            window.maxSize = new Vector2(350, window.maxSize.y);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSceneDataAsset();
            InitializeSceneData();
        }

        private void OnDisable()
        {
            editSceneList.drawElementCallback -= OnDrawEditSceneElement;
            visibleSceneList.drawElementCallback -= OnDrawVisibleSceneElement;
        }

        private void InitializeSceneData()
        {
            sceneData.PopulateScenesData();
            sceneData.PopulateVisibleScenesData();

            serializedObject = new SerializedObject(sceneData);
            sceneDataProperty = serializedObject.FindProperty("ScenesData");
            visibleSceneDataProperty = serializedObject.FindProperty("VisibleScenesData");

            editSceneList = new ReorderableList(serializedObject, sceneDataProperty, true, false, false, false);
            editSceneList.drawElementCallback += OnDrawEditSceneElement;

            visibleSceneList = new ReorderableList(serializedObject, visibleSceneDataProperty, false, false, false, false);
            visibleSceneList.drawElementCallback += OnDrawVisibleSceneElement;
        }

        private void LoadSceneDataAsset()
        {
            sceneData = AssetDatabase.LoadAssetAtPath("Assets/Scripts/Editor/SceneSelector/EditorSceneDataSO.asset", typeof(ScriptableObject)) as EditorSceneDataSO;
            if (sceneData == null)
            {
                sceneData = ScriptableObject.CreateInstance<EditorSceneDataSO>();
                AssetDatabase.CreateAsset(sceneData, "Assets/Scripts/Editor/SceneSelector/EditorSceneDataSO.asset");
            }
        }

        private void OnGUI()
        {
            if (sceneData == null)
            {
                LoadSceneDataAsset();
                InitializeSceneData();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                HeaderButton();
                serializedObject.Update();
                if (isEditing)
                {
                    editSceneList.DoLayoutList();
                }
                else
                {
                    visibleSceneList.DoLayoutList();
                }
                editSceneList.draggable = EditorApplication.isPlaying ? false : true;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndScrollView();
        }

        private void HeaderButton()
        {
            GUIContent refreshContent = EditorGUIUtility.IconContent("Refresh@2x");
            GUIContent settingContent = EditorGUIUtility.IconContent("_Popup@2x");
            refreshContent.tooltip = "Refresh";
            settingContent.tooltip = "Edit Mode";

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(refreshContent))
                {
                    InitializeSceneData();
                }
                if (GUILayout.Button(settingContent))
                {
                    isEditing = !isEditing;
                    InitializeSceneData();
                    editSceneList.draggable = isEditing ? true : false;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnDrawEditSceneElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= editSceneList.serializedProperty.arraySize) return;
            var element = editSceneList.serializedProperty.GetArrayElementAtIndex(index);

            var guidProperty = element.FindPropertyRelative("SceneGuid");
            var isVisibleProperty = element.FindPropertyRelative("IsVisible");

            var scenePath = AssetDatabase.GUIDToAssetPath(guidProperty.stringValue);
            var sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
            if (sceneAsset == null)
            {
                InitializeSceneData();
                Repaint();
                return;
            }

            bool isLoadedScene = EditorSceneManager.GetActiveScene().name == sceneAsset.name;
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying || isLoadedScene);
            {
                GUI.Label(new Rect(rect.x, rect.y, rect.width - 30f, EditorGUIUtility.singleLineHeight + 5f), sceneAsset.name);
                GUIContent visibleGui = new GUIContent(isVisibleProperty.boolValue ?
                        EditorGUIUtility.IconContent("animationvisibilitytoggleon@2x") :
                        EditorGUIUtility.IconContent("animationvisibilitytoggleoff@2x"));
                visibleGui.tooltip = isVisibleProperty.boolValue ? "Shown" : "Hidden";
                if (GUI.Button(new Rect(rect.xMax - 30f, rect.y, 30f, EditorGUIUtility.singleLineHeight + 5f), visibleGui))
                {
                    isVisibleProperty.boolValue = !isVisibleProperty.boolValue;
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void OnDrawVisibleSceneElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= visibleSceneList.serializedProperty.arraySize) return;
            var element = visibleSceneList.serializedProperty.GetArrayElementAtIndex(index);

            var guidProperty = element.FindPropertyRelative("SceneGuid");
            var isVisibleProperty = element.FindPropertyRelative("IsVisible");

            var scenePath = AssetDatabase.GUIDToAssetPath(guidProperty.stringValue);
            var sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
            if (sceneAsset == null)
            {
                InitializeSceneData();
                Repaint();
                return;
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                bool isLoadedScene = EditorSceneManager.GetActiveScene().name == sceneAsset.name;
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = isLoadedScene ? Color.green : oldColor;
                if (GUI.Button(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight + 5f), sceneAsset.name, buttonStyle))
                {
                    if (EditorSceneManager.GetActiveScene().name != sceneAsset.name)
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
                GUI.backgroundColor = oldColor;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
