using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Kanbarudesu.Editor.Utility
{
    [CreateAssetMenu(fileName = "EditorSceneDataSO", menuName = "Essential/Editor/EditorSceneData")]
    public class EditorSceneDataSO : ScriptableObject
    {
        [HideInInspector] public List<SceneData> ScenesData = new List<SceneData>();
        [HideInInspector] public List<SceneData> VisibleScenesData = new List<SceneData>();

        public void PopulateScenesData()
        {
            var guids = AssetDatabase.FindAssets("t:Scene").ToList();
            if (ScenesData.Count > guids.Count)
            {
                ScenesData.RemoveAll(data => !guids.Contains(data.SceneGuid));
            }

            foreach (var guid in guids)
            {
                if (!ScenesData.Any(data => data.SceneGuid == guid))
                {
                    ScenesData.Add(new SceneData(guid, true));
                }
            }
        }

        public void PopulateVisibleScenesData()
        {
            VisibleScenesData = ScenesData.FindAll(data => data.IsVisible == true);
        }
    }

    [System.Serializable]
    public struct SceneData
    {
        public string SceneGuid;
        public bool IsVisible;

        public SceneData(string sceneGuid, bool isVisible)
        {
            SceneGuid = sceneGuid;
            IsVisible = isVisible;
        }
    }
}