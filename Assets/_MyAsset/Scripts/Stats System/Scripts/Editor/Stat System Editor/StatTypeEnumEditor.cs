using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanbarudesu.StatSystem.Editor
{
    [Serializable]
    public class EnumEntry
    {
        public string Name;
        public int Value;

        public EnumEntry(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() => $"{Name} = {Value}";
    }

    public class StatTypeEnumEditor
    {
        private List<EnumEntry> statTypeList = new List<EnumEntry>();
        private string newEnumName = "";
        private int newEnumValueInt = 0;

        private Vector2 scrollPosition = Vector2.zero;

        public void LoadStatTypeEnum()
        {
            statTypeList.Clear();
            string path = FindStatTypeFilePath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            string text = File.ReadAllText(path);
            int start = text.IndexOf("{");
            int end = text.LastIndexOf("}");
            if (start >= 0 && end > start)
            {
                string content = text.Substring(start + 1, end - start - 1);
                string[] parts = content.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    string trimmed = part.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        string[] pair = trimmed.Split('=');
                        string name = pair[0].Trim();
                        int value = statTypeList.Count > 0 ? statTypeList.Last().Value + 1 : 0;
                        if (pair.Length > 1 && int.TryParse(pair[1].Trim(), out int parsedValue))
                            value = parsedValue;

                        statTypeList.Add(new EnumEntry(name, value));
                    }
                }
            }
            statTypeList.Sort((a, b) => a.Value.CompareTo(b.Value));
        }

        public void DrawEnumEditorUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("StatType Enum Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enum Values:", EditorStyles.boldLabel);
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh", "Refresh Enum Values"), GUILayout.Width(60)))
            {
                LoadStatTypeEnum();
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < statTypeList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"#{i + 1}", GUILayout.Width(25));
                statTypeList[i].Name = EditorGUILayout.TextField(statTypeList[i].Name);
                statTypeList[i].Value = EditorGUILayout.IntField(statTypeList[i].Value, GUILayout.Width(50));
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    statTypeList.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            newEnumName = EditorGUILayout.TextField("Name", newEnumName);
            newEnumValueInt = EditorGUILayout.IntField("Value", newEnumValueInt);
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                if (!string.IsNullOrWhiteSpace(newEnumName) && !statTypeList.Exists(e => e.Name == newEnumName))
                {
                    statTypeList.Add(new EnumEntry(newEnumName, newEnumValueInt));
                    newEnumName = "";
                    newEnumValueInt = 0;
                    GUI.FocusControl(null);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Entry", "Name is empty or already exists.", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Regenerate Enum File", GUILayout.Height(30)))
            {
                bool doRegenerate = EditorUtility.DisplayDialog("Regenerate Enum File",
                    "Are you sure you want to regenerate the enum file? \nThis will overwrite the existing file and might break some code.", "Yes", "No");
                if (doRegenerate)
                {
                    RegenerateEnumFile();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Be Careful! Renaming Enum might break some code that uses the enum. \nUse with caution and wisely!", MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void RegenerateEnumFile()
        {
            string path = FindStatTypeFilePath();
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Error", "StatType.cs file not found!", "OK");
                return;
            }
            try
            {
                string header = "public enum StatType {\n    ";
                string entries = string.Join(",\n    ", statTypeList.Select(e => $"{e.Name} = {e.Value}"));
                string footer = "\n}";
                string fullText = header + entries + footer;

                File.WriteAllText(path, fullText);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Enum Regenerated", "The StatType enum file has been regenerated.", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", "Error regenerating enum:\n" + ex.Message, "OK");
            }
        }

        private string FindStatTypeFilePath()
        {
            string[] guids = AssetDatabase.FindAssets("t:TextAsset StatType");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == "StatType")
                {
                    return path;
                }
            }
            return null;
        }
    }
}