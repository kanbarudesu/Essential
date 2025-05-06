using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kanbarudesu.StatSystem.Editor
{
    using UnityEditor;

    public class UnitStatsAssetEditor : BaseAssetEditor<UnitStats>
    {
        public UnitStatsAssetEditor(string folderPath) : base(folderPath) { }
    }

    public class StatusEffectsAssetEditor : BaseAssetEditor<StatusEffect>
    {
        public StatusEffectsAssetEditor(string folderPath) : base(folderPath) { }
    }

    public class IdentifiersAssetEditor : BaseAssetEditor<StatusEffectIdentifier>
    {
        public IdentifiersAssetEditor(string folderPath) : base(folderPath) { }
    }

    public class StatFormulaAssetEditor : BaseAssetEditor<StatFormula>
    {
        public StatFormulaAssetEditor(string folderPath) : base(folderPath) { }
    }

    public interface IAssetEditor
    {
        string FolderPath { get; set; }
        void RefreshAssets();
        bool CreateNewAsset(string assetName);
        void DrawAssetList(Action onSelect = null);
        void DrawInspector();
        bool HasSelection();
    }

    public abstract class BaseAssetEditor<T> : IAssetEditor where T : ScriptableObject
    {
        protected List<T> assetList = new List<T>();
        protected T selectedAsset;
        protected Editor cachedEditor;

        public string FolderPath { get; set; }

        public BaseAssetEditor(string folderPath)
        {
            FolderPath = folderPath;
        }

        public virtual void RefreshAssets()
        {
            assetList.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    assetList.Add(asset);
            }
        }

        public virtual bool CreateNewAsset(string assetName)
        {
            // Check for duplicate names
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path).Equals(assetName, System.StringComparison.OrdinalIgnoreCase))
                {
                    EditorUtility.DisplayDialog("Duplicate Name", $"An asset named '{assetName}' already exists:\n{path}", "OK");
                    return false;
                }
            }

            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                AssetDatabase.Refresh();
            }

            // Create the asset
            T newAsset = ScriptableObject.CreateInstance<T>();
            string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(FolderPath, assetName + ".asset"));
            AssetDatabase.CreateAsset(newAsset, newAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the new asset
            selectedAsset = newAsset;
            return true;
        }

        public virtual void DrawAssetList(Action onSelect = null)
        {
            for (int i = 0; i < assetList.Count; i++)
            {
                var asset = assetList[i];
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(asset.name, GUILayout.ExpandWidth(true)))
                {
                    selectedAsset = asset;
                    GUI.FocusControl(null);
                    onSelect?.Invoke();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("_Popup"), GUILayout.Width(30)))
                {
                    ShowAssetMenu(asset);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        public virtual void DrawInspector()
        {
            if (selectedAsset != null)
            {
                Editor.CreateCachedEditor(selectedAsset, null, ref cachedEditor);
                cachedEditor?.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.LabelField("Select an asset from the left panel.", EditorStyles.wordWrappedLabel);
            }
        }

        public virtual VisualElement DrawInspectorVE()
        {
            if (selectedAsset != null)
            {
                Editor editor = Editor.CreateEditor(selectedAsset);
                VisualElement ve = editor.CreateInspectorGUI();
                ve.Bind(new SerializedObject(selectedAsset));
                return ve;
            }
            else
            {
                return new IMGUIContainer(() => EditorGUILayout.LabelField("Select an asset from the left panel.", EditorStyles.wordWrappedLabel));
            }
        }

        public bool HasSelection()
        {
            return selectedAsset != null;
        }

        protected virtual void ShowAssetMenu(T asset)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Ping in Project"), false, () =>
            {
                EditorGUIUtility.PingObject(asset);
            });

            menu.AddItem(new GUIContent("Rename"), false, () =>
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string currentName = Path.GetFileNameWithoutExtension(path);
                RenamePopupWindow.Show(currentName, newName =>
                {
                    if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(path), newName + ".asset");
                        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newPath) != null)
                        {
                            EditorUtility.DisplayDialog("Rename Failed", $"An asset named '{newName}' already exists.", "OK");
                        }
                        else
                        {
                            AssetDatabase.RenameAsset(path, newName);
                            AssetDatabase.SaveAssets();
                            RefreshAssets();
                        }
                    }
                });
            });

            menu.AddItem(new GUIContent("Duplicate"), false, () =>
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string newPath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CopyAsset(path, newPath);
                AssetDatabase.SaveAssets();
                RefreshAssets();
            });

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                if (EditorUtility.DisplayDialog("Delete Asset", $"Are you sure you want to delete '{asset.name}'?", "Yes", "No"))
                {
                    string path = AssetDatabase.GetAssetPath(asset);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();

                    if (selectedAsset == asset)
                        selectedAsset = null;

                    RefreshAssets();
                }
            });

            menu.ShowAsContext();
        }
    }
}