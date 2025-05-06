using TMPro;
using UnityEngine;
using Kanbarudesu.StatSystem;

public class LevelExpDisplayDemo : MonoBehaviour
{
    [SerializeField] private StatusEffectController statusEffectController;
    [SerializeField] private UnitStats unitStats;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private RectTransform expBar;

    RuntimeStat expRuntimestat;
    Stat levelStat;

    StatusEffect levelStatusEffect;
    StatModifier levelModifier;
    StatusEffectIdentifier levelStatusEffectIdentifier;

    float previousLevelExp;

    private void Start()
    {
        unitStats.TryGetStat(StatType.Level, out levelStat);
        expRuntimestat = unitStats.GetRuntimeStat(StatType.Experience);
        expRuntimestat.CurrentValue = 0;

        levelStatusEffectIdentifier = ScriptableObject.CreateInstance<StatusEffectIdentifier>();
        levelStatusEffectIdentifier.name = StatType.Level.ToString();

        levelModifier = new StatModifier();
        levelModifier.Type = StatType.Level;

        levelStatusEffect = ScriptableObject.CreateInstance<StatusEffect>();
        levelStatusEffect.EffectName = StatType.Level.ToString();
        levelStatusEffect.Id = levelStatusEffectIdentifier;
        levelStatusEffect.Duration = -1f;
        levelStatusEffect.StackingRule = StatusStackingRule.Overwrite;
        levelStatusEffect.Modifiers = new()
        {
            levelModifier
        };

        UpdateLevelDisplay(0);
        expRuntimestat.OnRuntimeStatChanged += UpdateLevelDisplay;
        expRuntimestat.OnMaxStatChanged += OnStatChanged;
        levelStat.OnStatChanged += OnStatChanged;
    }

    private void OnStatChanged(float value)
    {
        levelText.text = "Level: " + levelStat.GetBasicFinalValue();
        
        float exp = expRuntimestat.CurrentValue - previousLevelExp;
        float nextExp = expRuntimestat.MaxValue - previousLevelExp;
        expText.text = "Exp: " + exp.ToString("0") + "/" + nextExp.ToString("0");
    }

    private void UpdateLevelDisplay(float currentValue = 0)
    {
        if (currentValue >= expRuntimestat.MaxValue)
        {
            previousLevelExp = expRuntimestat.MaxValue;
            levelModifier.Value += 1;
        }
        float exp = currentValue - previousLevelExp;
        float nextExp = expRuntimestat.MaxValue - previousLevelExp;
        expText.text = "Exp: " + exp.ToString("0") + "/" + nextExp.ToString("0");
        expBar.localScale = new Vector3(exp / nextExp, 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            expRuntimestat.Add(15f, false);
            statusEffectController.ApplyStatusEffect(levelStatusEffect);
        }
    }

    public void AddRandomExp(float maxValue)
    {
        expRuntimestat.Add(Random.Range(5f, maxValue), false);
        statusEffectController.ApplyStatusEffect(levelStatusEffect);
    }
}
