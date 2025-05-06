using UnityEngine;

namespace Kanbarudesu.StatSystem
{
    [CreateAssetMenu(fileName = "NewStatFormula", menuName = "Stats/StatFormula", order = 0)]
    public class StatFormula : ScriptableObject
    {
        public string Formula;
    }
}