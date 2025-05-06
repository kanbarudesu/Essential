using UnityEngine;
using Kanbarudesu.StatSystem;

public class AttributeStatsDemo : MonoBehaviour
{
    [SerializeField] private UnitStats _unitStats;
    [SerializeField] private StatType _statType;
    [SerializeField] private StatusEffectController _statusEffectController;

    private Stat _stat;
    private StatusEffect _statusEffect;
    private StatusEffectIdentifier _statusEffectIdentifier;
    private StatModifier _statModifier;

    private void Start()
    {
        _unitStats.TryGetStat(_statType, out _stat);

        _statusEffectIdentifier = ScriptableObject.CreateInstance<StatusEffectIdentifier>();
        _statusEffectIdentifier.name = _statType.ToString();

        _statModifier = new StatModifier();
        _statModifier.Type = _statType;

        _statusEffect = ScriptableObject.CreateInstance<StatusEffect>();
        _statusEffect.EffectName = _statType.ToString();
        _statusEffect.Id = _statusEffectIdentifier;
        _statusEffect.Duration = -1f;
        _statusEffect.StackingRule = StatusStackingRule.Overwrite;
        _statusEffect.Modifiers = new()
        {
            _statModifier
        };
    }

    public void AddModifierValue(int value)
    {
        _statModifier.Value += value;
        _statusEffectController.ApplyStatusEffect(_statusEffect);
    }
}
