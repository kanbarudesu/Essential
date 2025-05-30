using System;
using System.Collections.Generic;
using NCalc;
using UnityEngine;

namespace Kanbarudesu.StatSystem
{
    [Serializable]
    public class Stat
    {
        public StatType Type;
        public float BaseValue;
        public float MinValue = 0f;
        [Tooltip("Set to -1 for no maximum value")]
        public float MaxValue = -1f;
        public bool HasRuntimeValue = false;
        public StatFormula Formula;
        public List<StatModifier> Modifiers = new List<StatModifier>();

        public event Action<float> OnStatChanged;

        public bool HasMaxValue => MaxValue >= 0f;

        private ExpressionContext formulaContext;
        private Expression expression;

        public void SetFormulaContext(ExpressionContext context)
        {
            formulaContext = context;
        }

        // Return stat value without formula calculation
        public float GetBasicFinalValue()
        {
            float flatValue = 0f;
            float percentageValue = 0f;

            // Apply flat modifiers first.
            foreach (var mod in Modifiers)
            {
                if (mod.IsPercentage)
                {
                    percentageValue += BaseValue * (mod.Value / 100f);
                }
                else
                {
                    flatValue += mod.Value;
                }
            }
            // Validate the value.
            return ValidateValue(BaseValue + flatValue + percentageValue);
        }

        // Return stat value with formula calculation
        public float GetFinalValue()
        {
            if (Formula == null || string.IsNullOrEmpty(Formula.Formula))
            {
                return GetBasicFinalValue();
            }

            expression = new Expression(Formula.Formula, formulaContext);
            if (expression.HasErrors())
            {
                Debug.LogWarning(expression.Error);
                return 0f;
            }
            return Convert.ToSingle(expression.Evaluate());
        }

        public void UpdateStatChanged()
        {
            OnStatChanged?.Invoke(GetFinalValue());
        }

        public void AddModifier(StatModifier mod, bool shouldNotifyChange = true)
        {
            Modifiers.Add(mod);
            if (shouldNotifyChange)
            {
                OnStatChanged?.Invoke(GetFinalValue());
            }
        }

        public void RemoveModifier(StatModifier mod, bool shouldNotifyChange = true)
        {
            if (Modifiers.Remove(mod) && shouldNotifyChange)
            {
                OnStatChanged?.Invoke(GetFinalValue());
            }
        }

        public void ClearModifiers()
        {
            Modifiers.Clear();
            OnStatChanged?.Invoke(GetFinalValue());
        }

        public List<StatModifier> GetPercentageModifiers()
        {
            return Modifiers.FindAll(mod => mod.IsPercentage);
        }

        public List<StatModifier> GetFlatModifiers()
        {
            return Modifiers.FindAll(mod => !mod.IsPercentage);
        }

        private float ValidateValue(float value)
        {
            if (value < MinValue)
            {
                value = MinValue;
            }
            else if (value > MaxValue && HasMaxValue)
            {
                value = MaxValue;
            }
            return value;
        }
    }
}