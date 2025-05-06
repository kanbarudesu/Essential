using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditorInternal;

namespace Kanbarudesu.StatSystem.Editor
{
    using UnityEditor;
    [CustomEditor(typeof(StatusEffect))]
    public class StatusEffectEditor : Editor
    {
        private SerializedObject so;
        private ReorderableList modifierList;

        private void OnEnable()
        {
            modifierList = new ReorderableList(serializedObject, serializedObject.FindProperty("Modifiers"), true, true, true, true);
            modifierList.drawHeaderCallback = DrawHeader;
            modifierList.drawElementCallback = DrawElement;
            modifierList.elementHeightCallback = DrawElementHeight;
        }

        public override VisualElement CreateInspectorGUI()
        {
            so = new SerializedObject(target);
            var container = new ScrollView();

            container.Add(new PropertyField(serializedObject.FindProperty("EffectName")));
            container.Add(new PropertyField(serializedObject.FindProperty("Duration")));
            container.Add(new PropertyField(serializedObject.FindProperty("Description")));

            var iconContainer = new IMGUIContainer(CustomIconPreviewField);
            iconContainer.style.alignSelf = Align.FlexStart;
            iconContainer.style.height = 55;
            container.Add(iconContainer);

            container.Add(new PropertyField(serializedObject.FindProperty("Id")));
            container.Add(new PropertyField(serializedObject.FindProperty("StackingRule")));
            container.Add(new IMGUIContainer(CustomModifierField));

            return container;
        }

        private void CustomIconPreviewField()
        {
            so.Update();
            var iconProp = so.FindProperty("Icon");
            EditorGUIUtility.labelWidth = 124;
            iconProp.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField("Icon", iconProp.objectReferenceValue, typeof(Sprite), false);
            iconProp.serializedObject.ApplyModifiedProperties();
        }

        private void CustomModifierField()
        {
            so.Update();
            modifierList.DoLayoutList();
            modifierList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Modifiers");
        }

        private float DrawElementHeight(int index)
        {
            SerializedProperty element = modifierList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (modifierList.serializedProperty.GetArrayElementAtIndex(index) == null) return;

            SerializedProperty element = modifierList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty endProperty = element.GetEndProperty();
            SerializedProperty modifierTypeProp = element.FindPropertyRelative("Type");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalPadding = EditorGUIUtility.standardVerticalSpacing;
            Rect r = new Rect(rect.x, rect.y, rect.width, lineHeight);

            var content = modifierTypeProp.enumNames[modifierTypeProp.enumValueIndex] + " Modifier";
            element.isExpanded = EditorGUI.Foldout(new Rect(r.x, r.y, r.width - 20f, lineHeight), element.isExpanded, content, true);

            if (element.isExpanded && element.NextVisible(true))
            {
                do
                {
                    r.y += lineHeight + verticalPadding;
                    EditorGUI.PropertyField(r, element, true);
                }
                while (element.NextVisible(false) && !SerializedProperty.EqualContents(element, endProperty));
            }
        }
    }
}