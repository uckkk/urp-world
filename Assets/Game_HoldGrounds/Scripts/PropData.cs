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
        public float maxHealthPoints = 100;

        [Header("====== BUILDINGS - FARM")]
        [Tooltip("How much gold this building generates, in case it is a farm.")]
        public int goldGenerate;
        [Tooltip("Timer in seconds to wait before the next gold income;")]
        public float timerForGoldIncome;
        
        [Header("====== IF SPAWN UNITS")]
        [Tooltip("What kind of Unit can this building create, if any.")]
        public CharacterData unitDataType;
    }
}