using UnityEngine;
using UnityEngine.UI;
using Kanbarudesu.SceneManagement;

namespace Kanbarudesu.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        public enum AudioType
        {
            BgMusic,
            SFX
        }

        [SerializeField] private AudioClip audioClip;
        [SerializeField] private AudioType audioType;

        [SerializeField] private bool playOnAwake;
        [Tooltip("This will try to find Button component on awake if set to true")]
        [SerializeField] private bool playOnButtonPressed;
        [SerializeField] private bool stopMusicOnSceneChanged;

        private void Awake()
        {
            if (playOnAwake)
            {
                SceneLoadingManager.Instance.OnLoadSceneComplete += OnLoadSceneComplete;
            }

            if (playOnButtonPressed && TryGetComponent<Button>(out Button button))
            {
                button.onClick.AddListener(PlayClip);
            }
            else if (playOnButtonPressed)
            {
                Debug.Log($"Button Compoenent doesn't exist on {this.gameObject.name} Gameobject.", this.gameObject);
            }

            SceneLoadingManager.Instance.OnLoadSceneStarted += OnSceneChanged;
        }

        private void OnLoadSceneComplete()
        {
            PlayClip();
        }

        private void OnSceneChanged()
        {
            if (stopMusicOnSceneChanged)
            {
                AudioManager.Instance.StopMusicClip();
            }
        }

        public void PlayClip()
        {
            switch (audioType)
            {
                case AudioType.BgMusic: AudioManager.Instance.PlayMusicClip(audioClip); break;
                case AudioType.SFX: AudioManager.Instance.PlaySfxClip(audioClip); break;
                default: Debug.Log("Audio Type Not Found"); break;
            }
        }

        private void OnDestroy()
        {
            SceneLoadingManager.Instance.OnLoadSceneComplete -= OnLoadSceneComplete;
            SceneLoadingManager.Instance.OnLoadSceneStarted -= OnSceneChanged;
        }
    }
}
