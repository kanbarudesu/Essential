using System;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace Kanbarudesu.StatSystem.Editor
{
    using UnityEditor;
    [CustomEditor(typeof(StatFormula))]
    public class StatFormulaEditor : Editor
    {
        public StyleSheet styleSheet;
        private readonly string[] numbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private readonly string[] operators = { "+", "-", "*", "/", "%", "^", ">", "<", "(", ")", "=", ".", "," };
        private readonly string[] mathFunctions = { "Sin", "Cos", "Tan", "Asin", "Acos", "Atan", "Abs", "Sqrt", "Ceil", "Floor",
                                                "Round", "Log", "Min", "Max", "Pow", "Exp", "Sign", "Truncate" };
        private TextField formulaField;
        private Label cursorPosLabel;

        public override VisualElement CreateInspectorGUI()
        {
            // Create root container
            var root = new ScrollView();

            // Create formula text area with default styling
            formulaField = new TextField($"Formula : ({target.name})")
            {
                multiline = true
            };
            formulaField.bindingPath = "Formula";
            formulaField.style.flexDirection = FlexDirection.Column;
            formulaField.style.marginBottom = 10;

            // Make it look and behave more like a text area
            var textInput = formulaField.Q(TextField.textInputUssName);
            textInput.style.whiteSpace = WhiteSpace.Normal;
            textInput.style.unityTextAlign = TextAnchor.UpperLeft;
            textInput.style.minHeight = 50;
            textInput.style.flexGrow = 1;
            textInput.style.width = Length.Percent(100);

            root.Add(formulaField);

            // Add toolboxes in foldouts
            AddToolboxFoldout(root, "Numbers", CreateNumberButtons());
            AddToolboxFoldout(root, "Operators", CreateOperatorsButtons());
            AddToolboxFoldout(root, "Math .Net Functions", CreateMathFunctionButtons());
            AddToolboxFoldout(root, "Stat Types", CreateStatTypeButtons());
            AddToolboxFoldout(root, "Utility", CreateUtilityButtons());

            // Add cursor position indicator
            var cursorPosContainer = new VisualElement();
            cursorPosContainer.style.marginTop = 10;
            cursorPosContainer.style.borderTopWidth = 1;
            cursorPosContainer.style.borderTopColor = new Color(0.24f, 0.24f, 0.24f);
            cursorPosContainer.style.paddingTop = 5;

            cursorPosLabel = new Label("Cursor Position: 0");
            cursorPosLabel.style.fontSize = 12;
            cursorPosLabel.style.color = new Color(0.65f, 0.65f, 0.65f);
            cursorPosContainer.Add(cursorPosLabel);

            root.Add(cursorPosContainer);

            // Register callbacks to update cursor position label
            RegisterCursorPositionCallbacks();

            return root;
        }

        private void RegisterCursorPositionCallbacks()
        {
            // Common handler for updating cursor position
            void UpdateCursorPosition() => cursorPosLabel.text = $"Cursor Position: {formulaField.cursorIndex}";

            formulaField.RegisterCallback<KeyUpEvent>(_ => UpdateCursorPosition());
            formulaField.RegisterCallback<MouseUpEvent>(_ => UpdateCursorPosition());
            formulaField.RegisterCallback<FocusEvent>(_ => UpdateCursorPosition());
        }

        private void AddToolboxFoldout(VisualElement root, string title, List<Button> buttons)
        {
            // Create a foldout for the toolbox
            var foldout = new Foldout
            {
                text = title,
                value = SessionState.GetBool(title, true)
            };
            foldout.styleSheets.Add(styleSheet);
            foldout.RegisterValueChangedCallback(evt => SessionState.SetBool(title, evt.newValue));

            // Create container for buttons
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.flexWrap = Wrap.Wrap;
            buttonContainer.style.marginTop = 5;
            buttonContainer.style.marginBottom = 5;

            // Add all buttons to the container
            foreach (var button in buttons)
            {
                buttonContainer.Add(button);
            }

            foldout.Add(buttonContainer);
            root.Add(foldout);
        }

        private List<Button> CreateNumberButtons()
        {
            var buttons = new List<Button>();

            foreach (var number in numbers)
            {
                buttons.Add(CreateButton(number, number));
            }

            return buttons;
        }

        private List<Button> CreateOperatorsButtons()
        {
            var buttons = new List<Button>();

            foreach (var op in operators)
            {
                buttons.Add(CreateButton(op, op));
            }

            return buttons;
        }

        private List<Button> CreateMathFunctionButtons()
        {
            var buttons = new List<Button>();

            foreach (var func in mathFunctions)
            {
                buttons.Add(CreateButton(func, func + "()"));
            }

            return buttons;
        }

        private List<Button> CreateStatTypeButtons()
        {
            var buttons = new List<Button>();

            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                buttons.Add(CreateButton(statType.ToString(), statType.ToString()));
            }

            return buttons;
        }

        private List<Button> CreateUtilityButtons()
        {
            var buttons = new List<Button>
        {
            CreateButton("Space", " "),
            new Button(() =>
            {
                serializedObject.FindProperty("Formula").stringValue = "";
                serializedObject.ApplyModifiedProperties();

                // Update the text field
                formulaField.SetValueWithoutNotify("");
                formulaField.cursorIndex = 0;
                formulaField.Focus();
            })
            { text = "Clear Formula" },
            new Button(() => RemoveTextAtCursor())
            { text = "Backspace" }
        };

            return buttons;
        }

        private Button CreateButton(string label, string insertText = null)
        {
            if (insertText == null)
                insertText = label;

            var button = new Button(() => InsertTextAtCursor(insertText))
            {
                text = label
            };

            // Apply standard button styling
            button.style.marginRight = 3;
            button.style.marginBottom = 3;

            return button;
        }

        private void InsertTextAtCursor(string text)
        {
            // Get the current formula and cursor position
            string currentFormula = formulaField.value ?? "";
            int cursorPos = formulaField.cursorIndex;

            // Insert the text at the cursor position
            if (cursorPos >= 0 && cursorPos <= currentFormula.Length)
            {
                // Record undo operation
                Undo.RecordObject(target, "Insert Formula Element");

                // Insert text and update formula
                string newFormula = currentFormula.Insert(cursorPos, text);

                // Update the serialized object
                serializedObject.FindProperty("Formula").stringValue = newFormula;
                serializedObject.ApplyModifiedProperties();

                // Update the text field
                formulaField.SetValueWithoutNotify(newFormula);

                // Set cursor position after the inserted text
                formulaField.cursorIndex = cursorPos + text.Length;

                // Keep focus on the text field
                formulaField.Focus();
                formulaField.selectAllOnFocus = false;
                formulaField.selectAllOnMouseUp = false;

                // Update cursor position label
                cursorPosLabel.text = $"Cursor Position: {formulaField.cursorIndex}";
            }
        }

        private void RemoveTextAtCursor()
        {
            // Get the current formula and cursor position
            string currentFormula = formulaField.value ?? "";
            int cursorPos = formulaField.cursorIndex - 1;

            // Remove text at the cursor position
            if (cursorPos >= 0 && cursorPos < currentFormula.Length)
            {
                // Record undo operation
                Undo.RecordObject(target, "Remove Formula Element");

                // Remove text and update formula
                string newFormula = currentFormula.Remove(cursorPos, 1);

                // Update the serialized object
                serializedObject.FindProperty("Formula").stringValue = newFormula;
                serializedObject.ApplyModifiedProperties();

                // Update the text field
                formulaField.SetValueWithoutNotify(newFormula);

                // Set cursor position after the removed text
                formulaField.cursorIndex = cursorPos;

                // Keep focus on the text field
                formulaField.Focus();
                formulaField.selectAllOnFocus = false;
                formulaField.selectAllOnMouseUp = false;

                // Update cursor position label
                cursorPosLabel.text = $"Cursor Position: {formulaField.cursorIndex}";
            }
        }
    }
}