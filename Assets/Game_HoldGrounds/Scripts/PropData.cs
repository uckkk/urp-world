using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    [CreateAssetMenu(fileName = "PropData", menuName = "HoldGrounds/New PROP", order = 1)]
    [System.Serializable]
    public class PropData : ScriptableObject
    {
        [Header("====== PROP SETUP")]
        public string propName;
        public Sprite picture;
        public ObjectType objectType;
        public int goldCost;
        public float maxHealthPoints;

        [Header("====== BUILDINGS - FARM")]
        [Tooltip("How much gold this building generates, in case it is a farm.")]
        public int goldGenerate = 5;
        [Tooltip("Timer in seconds to wait before the next gold income;")]
        public float timerForGoldIncome = 5;
    }
}