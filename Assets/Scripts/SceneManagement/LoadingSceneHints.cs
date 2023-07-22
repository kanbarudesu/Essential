using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanbarudesu.SceneManagement
{
    [CreateAssetMenu(fileName = "LoadingSceneHints", menuName = "Essential/LoadingSceneHints", order = 0)]
    public class LoadingSceneHints : ScriptableObject
    {
        public List<string> Hints = new List<string>();

        public string GetRandomHint()
        {
            int index = Random.Range(0, Hints.Count - 1);
            return Hints[index];
        }
    }
}

