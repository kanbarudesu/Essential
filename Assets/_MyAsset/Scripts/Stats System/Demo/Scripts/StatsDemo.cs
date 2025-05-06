using TMPro;
using UnityEngine;
using NCalc;
using System;
using System.Collections.Generic;
using Kanbarudesu.StatSystem;

public class StatsDemo : MonoBehaviour
{
    public UnitStats Stats;
    public StatusEffect StatusEffect;
    public StatusEffectController StatusEffectController;

    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI DamageText;
    public TextMeshProUGUI LuckText;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI ManaText;

    public StatFormula StatFormula;

    private void Awake()
    {
        Stats.InitializeUnitStats();
    }

    private void Start()
    {
        // Stats.RegisterStatChange(StatType.Speed, value => SpeedText.text = "Speed: " + value.ToString());
        // Stats.RegisterStatChange(StatType.BaseDamage, value => DamageText.text = "Base Damage: " + value.ToString());
        // Stats.RegisterStatChange(StatType.Luck, value => LuckText.text = "Luck: " + value.ToString());

        // SpeedText.text = "Speed: " + Stats.GetStatValue(StatType.Speed).ToString();
        // DamageText.text = "Base Damage: " + Stats.GetStatValue(StatType.BaseDamage).ToString();
        // LuckText.text = "Luck: " + Stats.GetStatValue(StatType.Luck).ToString();

        // var health = Stats.GetRuntimeStat(StatType.Health);
        // var mana = Stats.GetRuntimeStat(StatType.Mana);

        // health.OnStatChanged += value => HealthText.text = "Health/MaxHealth: " + value + "/" + health.MaxValue;
        // mana.OnStatChanged += value => ManaText.text = "Mana/MaxMana: " + value + "/" + mana.MaxValue;

        // HealthText.text = "Health: " + health.CurrentValue + "/" + health.MaxValue;
        // ManaText.text = "Mana: " + mana.CurrentValue + "/" + mana.MaxValue;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StatusEffectController.ApplyStatusEffect(StatusEffect);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Stats.SetRuntimeStat(StatType.Health, -10);
            Stats.SetRuntimeStat(StatType.Mana, -5);
        }
    }

    [ContextMenu("Initialize Formulas")]
    public void Test1()
    {
        Stats.InitializeUnitStats();
    }

    [ContextMenu("Test2")]
    public void Test2()
    {
        Debug.Log("Health Formula Value : " + Stats.GetStatFormulaValue(StatType.Health));
        Debug.Log("Damage Formula Value : " + Stats.GetStatFormulaValue(StatType.BaseDamage));
        Debug.Log("Speed Formula Value : " + Stats.GetStatFormulaValue(StatType.Speed));
    }

    [ContextMenu("Test3")]
    public void Test3()
    {
        var expression = new Expression("Cos(0) + Speed * Damage - Luck * Attribute");
        var expression2 = new Expression("Speed * BaseDamage - Luck");

        foreach (var parameter in expression2.GetParameterNames())
        {
            if (Enum.TryParse(parameter, out StatType statType))
            {
                Debug.Log(statType.ToString() + ": " + Stats.GetStatValue(statType));
                expression2.DynamicParameters[parameter] = _ => Stats.GetStatValue(statType);
            }
        }
        Debug.Log("evaluated: " + expression2.Evaluate());
    }

    private Dictionary<string, ExpressionParameter> cachedDynamicParameters = new();
    Expression testExpression;

    [ContextMenu("Test Cached Dynamic Parameters")]
    public void Test4()
    {
        foreach (var type in Enum.GetNames(typeof(StatType)))
        {
            if (Enum.TryParse(type, out StatType statType))
            {
                cachedDynamicParameters[type] = _ => Stats.GetStatValue(statType);
            }
        }
    }

    [ContextMenu("Test result 1")]
    public void Test5()
    {
        testExpression = new Expression("Speed + BaseDamage - Luck");
        testExpression.DynamicParameters = cachedDynamicParameters;
        Debug.Log("evaluated : " + Convert.ToSingle(testExpression.Evaluate()));
    }

    [ContextMenu("Test result 2")]
    public void Test6()
    {
        testExpression = new Expression(StatFormula.Formula);
        testExpression.DynamicParameters = cachedDynamicParameters;
        Debug.Log("evaluated : " + Convert.ToSingle(testExpression.Evaluate()));
    }
}
