using System.Collections.Generic;
using UnityEngine;

namespace Kanbarudesu.StatSystem
{
    public class StatusEffectController : MonoBehaviour
    {
        public UnitStats UnitStats;

        private List<RuntimeStatusEffect> activeStatusEffect = new();

        void Update()
        {
            for (int i = activeStatusEffect.Count - 1; i >= 0; i--)
            {
                var modifier = activeStatusEffect[i];
                if (modifier.ExpireTime == null) continue;

                if (Time.time >= modifier.ExpireTime)
                {
                    RemoveStatusEffect(modifier);
                    activeStatusEffect.RemoveAt(i);
                }
            }
        }

        public void ApplyStatusEffect(StatusEffect statusEffect)
        {
            var existStatusEffect = activeStatusEffect.FindAll(runtimeStatusEffect => runtimeStatusEffect.Source.Id == statusEffect.Id);

            switch (statusEffect.StackingRule)
            {
                case StatusStackingRule.Ignore:
                    if (existStatusEffect != null) return;
                    break;

                case StatusStackingRule.Refresh:
                    if (existStatusEffect != null  && existStatusEffect.Count > 0)
                    {
                        existStatusEffect[0].RefreshTime();
                        Debug.Log($"Refreshed buff: <color=green>{statusEffect.EffectName}</color>");
                        return;
                    }
                    break;

                case StatusStackingRule.Overwrite:
                    if (existStatusEffect != null && existStatusEffect.Count > 0)
                    {
                        Debug.Log($"Overwritten all buff: <color=red>{existStatusEffect[0].Source.EffectName}</color> → <color=green>{statusEffect.EffectName}</color>");
                        RemoveStatusEffects(existStatusEffect);
                    }
                    break;

                case StatusStackingRule.Stack:
                    // Allow stacking (default)
                    break;
            }

            foreach (var modifier in statusEffect.Modifiers)
            {
                UnitStats.AddModifier(modifier.Type, modifier);
            }
            activeStatusEffect.Add(new RuntimeStatusEffect(statusEffect, statusEffect.Duration));
            Debug.Log($"Applied buff: <color=green>{statusEffect.EffectName}</color>");
        }

        private void RemoveStatusEffect(RuntimeStatusEffect runtimeStatusEffect)
        {
            foreach (var modifier in runtimeStatusEffect.Source.Modifiers)
            {
                UnitStats.RemoveModifier(modifier.Type, modifier);
                Debug.Log($"Removed modifier for <color=red>{modifier.Type}</color> StatType from buff: <color=lightblue>{runtimeStatusEffect.Source.EffectName}</color>");
            }
        }

        private void RemoveStatusEffects(List<RuntimeStatusEffect> activeStatusEffects)
        {
            foreach (var activeStatus in activeStatusEffects)
            {
                RemoveStatusEffect(activeStatus);
                activeStatusEffect.Remove(activeStatus);
            }
        }
    }
}