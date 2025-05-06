using UnityEngine;

namespace Kanbarudesu.StatSystem
{
    public class RuntimeStatusEffect
    {
        public StatusEffect Source;
        public float? ExpireTime;

        public RuntimeStatusEffect(StatusEffect source, float duration)
        {
            Source = source;
            ExpireTime = duration == -1 ? null : Time.time + duration;
        }

        public void RefreshTime()
        {
            ExpireTime = Time.time + Source.Duration;
        }
    }
}