using System;
using UnityEngine;

namespace Kanbarudesu.StatSystem
{
    public class RuntimeStat
    {
        public StatType Type;
        public float CurrentValue;
        private UnitStats owner;

        public event Action<float> OnRuntimeStatChanged; //Runtime value changed
        public event Action<float> OnMaxStatChanged; //Max Stat value changed

        public RuntimeStat(StatType type, UnitStats owner)
        {
            Type = type;
            this.owner = owner;
            CurrentValue = MaxValue;
        }

        public float MaxValue => owner.GetStatFormulaValue(Type);

        public void Add(float amount, bool clampValue = true)
        {
            float oldValue = CurrentValue;
            if (clampValue)
                CurrentValue = Mathf.Clamp(CurrentValue + amount, 0, MaxValue);
            else
                CurrentValue += amount;

            if (Math.Abs(oldValue - CurrentValue) > 0.001f)
                OnRuntimeStatChanged?.Invoke(CurrentValue);
        }

        public void Set(float value, bool clampValue = true)
        {
            float oldValue = CurrentValue;
            if (clampValue)
                CurrentValue = Mathf.Clamp(value, 0, MaxValue);
            else
                CurrentValue = value;

            if (Math.Abs(oldValue - CurrentValue) > 0.001f)
                OnRuntimeStatChanged?.Invoke(CurrentValue);
        }

        public void UpdateMaxStatValue()
        {
            OnMaxStatChanged?.Invoke(MaxValue);
        }

        public void RestoreToMax()
        {
            CurrentValue = MaxValue;
            OnRuntimeStatChanged?.Invoke(CurrentValue);
        }
    }
}