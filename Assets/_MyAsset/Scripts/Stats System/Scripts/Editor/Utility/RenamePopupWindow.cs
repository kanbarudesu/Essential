using UnityEditor;
using UnityEngine;

public class RenamePopupWindow : EditorWindow
{
    private string newName;
    private System.Action<string> onClose;

    public static void Show(string currentName, System.Action<string> onClose)
    {
        var window = ScriptableObject.CreateInstance<RenamePopupWindow>();
        window.newName = currentName;
        window.onClose = onClose;
        window.titleContent = new GUIContent("Rename Asset");
        window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 250, 80);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("New Name:", EditorStyles.boldLabel);
        newName = EditorGUILayout.TextField(newName);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rename"))
        {
            onClose?.Invoke(newName);
            Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}