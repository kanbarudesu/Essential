using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kanbarudesu.SceneManagement
{
    public class LoadingScreenUI : MonoBehaviour
    {
        [Header("Hints")]
        [SerializeField] private LoadingSceneHints loadingSceneHints;
        [SerializeField] private string hintPrefix = "Hint: ";

        [Header("Loading Progress")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image splashscreen;
        [SerializeField] private TMP_Text hintLabelText;
        [SerializeField] private TMP_Text loadingIndicatorText;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private TMP_Text loadingProgressText;
        [SerializeField] private TMP_Text loadingDescriptionText;

        [SerializeField] private CanvasGroup canvasGroup;

        private bool waitingForClose;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            SceneLoadingManager.Instance.OnLoadSceneComplete += OnLoadSceneComplete;
        }

        private void Update()
        {
            if (SceneLoadingManager.Instance.IsLoading)
            {
                loadingBar.value = SceneLoadingManager.Instance.TotalProgress;
                loadingProgressText.text = SceneLoadingManager.Instance.TotalProgress.ToString("##%");
                loadingDescriptionText.text = SceneLoadingManager.Instance.CurrentProcessDescription;
            }
        }

        public void UpdateLoadingIndicatorText(string value)
        {
            loadingIndicatorText.text = value;
        }

        public void Show(SceneLocation sceneLocation)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;

            titleText.text = sceneLocation.Name;
            hintLabelText.text = hintPrefix + loadingSceneHints.GetRandomHint();
            splashscreen.overrideSprite = sceneLocation.LoadingSplashScreen;
            loadingIndicatorText.text = "LOADING";
            loadingDescriptionText.text = "";
            loadingProgressText.text = "0%";
            loadingBar.value = 0;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnLoadSceneComplete()
        {
            Hide();
        }
    }
}
