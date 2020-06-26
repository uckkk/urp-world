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
        [Tooltip("How much gold we can generate from each tree as a Bonus.")]
        public int extraGoldPerTree;
        [Tooltip("The radius to search for the trees.")]
        public float extraGoldSearchRadius;
        
        [Header("====== IF SPAWN UNITS")]
        [Tooltip("What kind of Unit can this building create, if any.")]
        public CharacterData unitDataType;
        
        [Header("====== DEFENSE - BUILDINGS")]
        [Tooltip("Damage to apply to a Unit if this building shoots something.")]
        public float defenseDamage;
        [Tooltip("Distance to start attacking.")]
        public float defenseRadiusDistance;
        [Tooltip("How fast it should attack.")]
        public float defenseAttackRate;
        
        // =============================================================================================================
        /// <summary>
        /// Picture to be used in UI.
        /// </summary>
        public Sprite SetPicture {set => picture = value;}
        // =============================================================================================================
        /// <summary>
        /// Picture to be used in UI.
        /// </summary>
        public Sprite GetPicture => picture;
        // =============================================================================================================
    }
}