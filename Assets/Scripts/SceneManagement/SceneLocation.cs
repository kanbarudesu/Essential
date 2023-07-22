using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanbarudesu.SceneManagement
{
    [CreateAssetMenu(fileName = "SceneLocation", menuName = "Essential/SceneLocation", order = 0)]
    public class SceneLocation : ScriptableObject
    {
        public string Name;
        public Sprite LoadingSplashScreen;
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset MainScene;
        [SerializeField] private List<UnityEditor.SceneAsset> AdditiveScenes = new();
#endif
        public LoadSceneMode MainSceneLoadSceneMode;
        public bool ShowLoadingScreen = true;

        [HideInInspector] public string SceneName;
        [HideInInspector] public List<string> additiveScenesName = new();

        public void Load()
        {
            SceneLoadingManager.Instance.LoadScene(this);
        }

#if UNITY_EDITOR
        private bool firstOnValidateHasOccurred;

        private void OnValidate()
        {
            if (!firstOnValidateHasOccurred)
            {
                firstOnValidateHasOccurred = true;
                return;
            }

            OnValuesChangedInTheInspector();
        }

        private void OnValuesChangedInTheInspector()
        {
            SceneName = "";
            additiveScenesName = new();
            if (MainScene != null)
            {
                SceneName = MainScene.name;
            }
            foreach (var scene in AdditiveScenes)
            {
                if (scene != null)
                    additiveScenesName.Add(scene.name);
            }
        }
#endif
    }
}
