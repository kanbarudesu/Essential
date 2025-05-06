using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Kanbarudesu.StatSystem;

public class StatusEffectDisplayDemo : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    [SerializeField] private StatusEffect statusEffect;
    [SerializeField] private StatusEffectPanel statusEffectPanel;

    private void Start()
    {
        buttonText.text = statusEffect.EffectName;
        button.onClick.AddListener(() =>
        {
            statusEffectPanel.SetPanelDisplay(statusEffect);
        });
    }
}
