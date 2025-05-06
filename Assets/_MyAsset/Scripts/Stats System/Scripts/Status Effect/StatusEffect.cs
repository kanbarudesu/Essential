using UnityEngine;
using System.Collections.Generic;

namespace Kanbarudesu.StatSystem
{
    [CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Stats/StatusEffect")]
    public class StatusEffect : ScriptableObject
    {
        public string EffectName;
        public float Duration = 1f;
        [TextArea(3, 10)]
        public string Description;
        public Sprite Icon;
        public StatusEffectIdentifier Id;
        public StatusStackingRule StackingRule;
        public List<StatModifier> Modifiers;
    }

    public enum StatusStackingRule
    {
        Stack,      // Add multiple copies
        Refresh,    // Refresh duration
        Overwrite,  // Replace existing one
        Ignore      // Skip if already active and wait till expire
    }
}