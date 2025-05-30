using System.Collections.Generic;

namespace Kanbarudesu.StatSystem
{
    public class StatPreviewContext
    {
        public readonly Dictionary<StatType, StatPreviewChange> StatChanges = new();
        public readonly HashSet<StatType> AffectedStatTypes = new();
        public readonly Dictionary<Stat, List<StatModifier>> AppliedModifiers = new();

        public void Clear()
        {
            StatChanges.Clear();
            AffectedStatTypes.Clear();
            AppliedModifiers.Clear();
        }
    }

    public class StatPreviewChange
    {
        public float Before;
        public float After;

        public StatPreviewChange(float before, float after)
        {
            Before = before;
            After = after;
        }
    }
}