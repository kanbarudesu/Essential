using TMPro;
using UnityEngine;

namespace Kanbarudesu.StatSystem
{
    public class StatValueBinder : MonoBehaviour
    {
        [SerializeField] private UnitStats unitStats;
        [SerializeField] private StatType statTypeToBind;
        [SerializeField] private TMP_Text statValueText;

        public string Prefix = "";
        public string Suffix = "";
        public bool isRuntimeValue;
        public bool ShowMaxValue;

        private Stat boundStat;
        private RuntimeStat boundRuntimeStat;

        private void Start()
        {
            if (unitStats == null)
            {
                Debug.LogWarning("UnitStats is null", this);
                return;
            }

            InitializeStatBinding();
        }

        public void InitializeStatBinding()
        {
            if (isRuntimeValue)
            {
                boundRuntimeStat = unitStats.GetRuntimeStat(statTypeToBind);
                boundRuntimeStat.OnRuntimeStatChanged += UpdateDisplay;
                boundRuntimeStat.OnMaxStatChanged += UpdateDisplay;
                UpdateDisplay(boundRuntimeStat.CurrentValue);
            }
            else if (unitStats.TryGetStat(statTypeToBind, out boundStat))
            {
                boundStat.OnStatChanged += UpdateDisplay;
                UpdateDisplay(unitStats.GetStatFormulaValue(statTypeToBind));
            }
        }

        private void OnDisable()
        {
            if (boundStat != null)
            {
                boundStat.OnStatChanged -= UpdateDisplay;
            }

            if (isRuntimeValue)
            {
                boundRuntimeStat.OnRuntimeStatChanged -= UpdateDisplay;
                boundRuntimeStat.OnMaxStatChanged -= UpdateDisplay;
            }
        }

        private void UpdateDisplay(float value)
        {
            statValueText.text = isRuntimeValue
                ? $"{Prefix}{boundRuntimeStat.CurrentValue}" + (ShowMaxValue ? $"/{boundRuntimeStat.MaxValue}" : "") + Suffix
                : $"{Prefix}{boundStat.GetFinalValue()}" + (ShowMaxValue ? $"/{boundStat.MaxValue}" : "") + Suffix;
        }

        public void SetupStatBinding(UnitStats unitStats, StatType statTypeToBind, TMP_Text statValueText)
        {
            this.unitStats = unitStats;
            this.statTypeToBind = statTypeToBind;
            this.statValueText = statValueText;
        }
    }
}