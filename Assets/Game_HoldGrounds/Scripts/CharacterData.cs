using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "HoldGrounds/New UNIT", order = 1)]
    [System.Serializable]
    public class CharacterData : ScriptableObject
    {
        [Header("====== UNIT SETUP")]
        public string unitName;
        public Sprite picture;
        public int goldCost;
        public float maxHealthPoints = 100;
        public float damage;
        public float defense;
        [Tooltip("Minimum distance to start attacking. This will apply to the stop distance for the NavMeshAgent.")]
        public float atkDistance = 2;
        public float atkSpeed = 2;
        [Range(1, 6)]
        public float moveSpeed = 3;
        public float timerToSpawn = 5;
        public GameObject unitPrefab;
        [Tooltip("Check GameTags for the order.")]
        public Material[] teamMaterial;
        
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