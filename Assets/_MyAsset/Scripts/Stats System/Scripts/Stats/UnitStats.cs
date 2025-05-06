using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using NCalc;
using System;

namespace Kanbarudesu.StatSystem
{
    [CreateAssetMenu(fileName = "NewUnitStats", menuName = "Stats/UnitStats")]
    public class UnitStats : ScriptableObject
    {
        public List<Stat> Stats = new List<Stat>();

        private Dictionary<StatType, Stat> statDictionary = new();
        private Dictionary<StatType, RuntimeStat> runtimeStats = new();
        private Dictionary<StatType, List<StatType>> dependentStats = new();

        private ExpressionContext context = new();

        public void InitializeUnitStats()
        {
            statDictionary.Clear();
            statDictionary = Stats.ToDictionary(s => s.Type);

            //Initialize dynamic parameters
            context.DynamicParameters.Clear();
            foreach (var stat in Stats)
                context.DynamicParameters[stat.Type.ToString()] = _ => GetStatValue(stat.Type);

            //Initialize stat dependencies
            BuildDependencyGraph();

            //Initialize stats that has runtime value
            runtimeStats.Clear();
            runtimeStats = Stats.Where(s => s.HasRuntimeValue)
                                .ToDictionary(s => s.Type, s => new RuntimeStat(s.Type, this));

            HookStatDependencies();
        }

        private void BuildDependencyGraph()
        {
            dependentStats.Clear();

            foreach (var stat in Stats)
            {
                if (stat.Formula == null || string.IsNullOrEmpty(stat.Formula.Formula)) continue;

                stat.SetFormulaContext(context);
                var expression = new Expression(stat.Formula.Formula, context);
                foreach (var param in expression.GetParameterNames())
                {
                    if (!Enum.TryParse(param, out StatType statType) || param == stat.Type.ToString()) continue;

                    if (!dependentStats.ContainsKey(statType))
                        dependentStats[statType] = new List<StatType>();

                    if (!dependentStats[statType].Contains(stat.Type))
                        dependentStats[statType].Add(stat.Type);
                }
            }
        }

        private void HookStatDependencies()
        {
            foreach (var dependencyStat in dependentStats.Keys)
            {
                RegisterStatChange(dependencyStat, _ => OnStatChanged(dependencyStat));
            }

            void OnStatChanged(StatType changedStat)
            {
                if (!dependentStats.TryGetValue(changedStat, out var affectedStats)) return;
                foreach (var statType in affectedStats)
                {
                    if (TryGetStat(statType, out var stat))
                    {
                        stat.UpdateStatChanged();
                    }
                    if (runtimeStats.TryGetValue(statType, out var runtimeStat))
                    {
                        runtimeStat.UpdateMaxStatValue();
                    }
                }
            }
        }

        public void SetRuntimeStat(StatType type, float amount)
        {
            if (runtimeStats.TryGetValue(type, out RuntimeStat stat))
            {
                stat.Add(amount);
            }
            else
            {
                Debug.LogWarning($"Can't find RuntimeStat of {type} type in {name} Data", this);
            }
        }

        //Return stat value without formula calculation
        public float GetStatValue(StatType type)
        {
            if (TryGetStat(type, out Stat stat))
            {
                return stat.GetBasicFinalValue();
            }
            Debug.LogWarning($"{type} type not found in {name} Data", this);
            return 0f;
        }

        //Return stat value with formula calculation
        public float GetStatFormulaValue(StatType type)
        {
            if (TryGetStat(type, out Stat stat))
            {
                if (stat.Formula == null)
                {
                    return stat.GetBasicFinalValue();
                }
                return stat.GetFinalValue();
            }
            Debug.LogWarning($"{type} type not found in {this.name} Data", this);
            return 0f;
        }

        public void AddModifier(StatType type, StatModifier modifier, bool shouldNotifyChange = true)
        {
            if (TryGetStat(type, out Stat stat))
            {
                stat.AddModifier(modifier, shouldNotifyChange);
            }

            if (TryGetRuntimeStat(type, out RuntimeStat runtimeStat))
            {
                runtimeStat.UpdateMaxStatValue();
            }
        }

        public void RemoveModifier(StatType type, StatModifier modifier, bool shouldNotifyChange = true)
        {
            if (TryGetStat(type, out Stat stat))
            {
                stat.RemoveModifier(modifier, shouldNotifyChange);
            }
            if (TryGetRuntimeStat(type, out RuntimeStat runtimeStat))
            {
                runtimeStat.UpdateMaxStatValue();
            }
        }

        public void RegisterStatChange(StatType type, Action<float> callback)
        {
            if (TryGetStat(type, out Stat stat))
            {
                stat.OnStatChanged += callback;
            }
        }

        public void UnregisterStatChange(StatType type, Action<float> callback)
        {
            if (TryGetStat(type, out Stat stat))
            {
                stat.OnStatChanged -= callback;
            }
        }

        public bool TryGetStat(StatType type, out Stat stat)
        {
            return statDictionary.TryGetValue(type, out stat);
        }

        public bool TryGetRuntimeStat(StatType type, out RuntimeStat stat)
        {
            return runtimeStats.TryGetValue(type, out stat);
        }

        public RuntimeStat GetRuntimeStat(StatType type)
        {
            if (runtimeStats.TryGetValue(type, out RuntimeStat stat))
            {
                return stat;
            }
            Debug.LogWarning($"{type} type not found in {name} Data", this);
            return null;
        }

#if UNITY_EDITOR
        [ContextMenu("Log Stat Dependency Graph")]
        public void LogDependencyGraph()
        {
            foreach (var kvp in dependentStats)
            {
                Debug.Log($"{kvp.Key} affects: {string.Join(", ", kvp.Value)}");
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += ClearModifiers;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= ClearModifiers;
        }

        private void ClearModifiers(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                foreach (var stat in Stats)
                {
                    stat.ClearModifiers();
                }
            }
        }
#endif

    }
}