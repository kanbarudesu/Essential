using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Kanbarudesu.StatSystem;

public class StatusEffectPanel : MonoBehaviour
{
    [SerializeField] private StatusEffectController statusEffectController;
    [SerializeField] private TMP_Text modifierText;
    [SerializeField] private Button applyButton;

    private StatusEffect statusEffect;

    private void Start()
    {
        applyButton.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (statusEffect == null) return;
        statusEffectController.ApplyStatusEffect(statusEffect);
    }

    public void SetPanelDisplay(StatusEffect statusEffect)
    {
        this.statusEffect = statusEffect;
        StringBuilder modText = new();
        foreach (var modifier in statusEffect.Modifiers)
        {
            string isPercentage = modifier.IsPercentage ? "%" : "";
            string valueType = modifier.Value > 0 ? "+" : "-";
            modText.Append($"{modifier.Type} : {valueType}{modifier.Value}{isPercentage} \n");
        }
        modText.Append($"Duration: {statusEffect.Duration} \n");
        modText.Append($"Stacking Rule: {statusEffect.StackingRule}");
        modifierText.text = modText.ToString();
    }
}
