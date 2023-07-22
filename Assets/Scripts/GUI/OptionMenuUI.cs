using Kanbarudesu.Audio;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenuUI : MonoBehaviour
{
    [SerializeField] private Button muteMusicButton;
    [SerializeField] private Button muteSfxButton;
    [SerializeField] private Button backButton;

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] private Sprite mutedMusicSprite;
    [SerializeField] private Sprite mutedSfxSprite;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        InitializeButtonAndSlider();
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    private void InitializeButtonAndSlider()
    {
        muteMusicButton.onClick.AddListener(OnMuteMusicButtonPressed);
        muteSfxButton.onClick.AddListener(OnMuteSfxButtonPressed);
        backButton.onClick.AddListener(OnBackButtonPressed);

        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void OnBackButtonPressed()
    {
        AudioManager.Instance.SaveAudiSourcesState();
    }

    private void OnSfxVolumeChanged(float value)
    {
        AudioManager.Instance.SetSfxVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnMuteSfxButtonPressed()
    {
        AudioManager.Instance.ToggleMuteSfxAudio();
        muteSfxButton.GetComponent<Image>().overrideSprite = AudioManager.Instance.IsSfxMuted ? mutedSfxSprite : null;
    }

    private void OnMuteMusicButtonPressed()
    {
        AudioManager.Instance.ToggleMuteMusicAudio();
        muteMusicButton.GetComponent<Image>().overrideSprite = AudioManager.Instance.IsMusicMuted ? mutedMusicSprite : null;
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}
