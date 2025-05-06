using UnityEngine;
using System;
using System.Linq;
using UnityEditorInternal;

namespace Kanbarudesu.StatSystem.Editor
{
    using UnityEditor;
    [CustomEditor(typeof(UnitStats), true)]
    public class UnitStatsEditor : Editor
    {
        private int selectedIndex = 0;
        private int indexToRemove = -1;
        private ReorderableList reorderableList;

        private Vector2 scrollPosition;

        private void OnEnable()
        {
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("Stats"), true, true, false, false);
            reorderableList.drawHeaderCallback = DrawStatsHeader;
            reorderableList.drawElementCallback = DrawStatsElement;
            reorderableList.elementHeightCallback = GetStatsElementHeight;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawAddNewStat();
            reorderableList.DoLayoutList();
            DelayRemoveStat();
            EditorGUILayout.EndScrollView();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAddNewStat()
        {
            UnitStats unitStats = (UnitStats)serializedObject.targetObject;
            var allStatTypes = Enum.GetValues(typeof(StatType)).Cast<StatType>().ToList();
            var usedStatTypes = unitStats.Stats.Select(s => s.Type);
            var availableStatTypes = allStatTypes.Except(usedStatTypes).ToList();

            if (availableStatTypes.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Add New Stat", EditorStyles.boldLabel);

                string[] options = availableStatTypes.Select(t => t.ToString()).ToArray();
                selectedIndex = EditorGUILayout.Popup("Stat Type", selectedIndex, options);

                if (GUILayout.Button("Add Stat"))
                {
                    Undo.RecordObject(unitStats, "Add Stat");
                    Stat newStat = new Stat
                    {
                        Type = availableStatTypes[selectedIndex],
                        BaseValue = 0f
                    };

                    unitStats.Stats.Add(newStat);
                    EditorUtility.SetDirty(unitStats);
                    serializedObject.Update();
                    selectedIndex = 0;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("All available stat types have been added.", MessageType.Info);
            }

            EditorGUILayout.Space();
        }

        private void DrawStatsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Stats");
        }

        private float GetStatsElementHeight(int index)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        private void DrawStatsElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (reorderableList.serializedProperty.GetArrayElementAtIndex(index) == null) return;

            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty endProperty = element.GetEndProperty();
            SerializedProperty statTypeProperty = element.FindPropertyRelative("Type");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalPadding = EditorGUIUtility.standardVerticalSpacing;
            Rect r = new Rect(rect.x, rect.y, rect.width, lineHeight);

            string statName = statTypeProperty.enumDisplayNames[statTypeProperty.enumValueIndex];
            element.isExpanded = EditorGUI.Foldout(new Rect(r.x, r.y, r.width - 20f, lineHeight), element.isExpanded, statName, true);
            if (GUI.Button(new Rect(r.x + r.width - 20f, r.y, 20f, lineHeight), EditorGUIUtility.IconContent("Toolbar Minus", $"Remove {statName} Stat"), EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Remove Stat", $"Are you sure you want to remove {statName}?", "Yes", "No"))
                {
                    indexToRemove = index;
                }
                return;
            }
            if (element.isExpanded && element.NextVisible(true))
            {
                do
                {
                    r.y += lineHeight + verticalPadding;
                    if (SerializedProperty.EqualContents(element, statTypeProperty))
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.PropertyField(r, element, true);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUI.PropertyField(r, element, true);
                    }
                }
                while (element.NextVisible(false) && !SerializedProperty.EqualContents(element, endProperty));
            }
        }

        private void DelayRemoveStat()
        {
            if (indexToRemove >= 0)
            {
                reorderableList.serializedProperty.DeleteArrayElementAtIndex(indexToRemove);
                indexToRemove = -1;
            }
        }
    }
}