using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "HoldGrounds/New UNIT", order = 1)]
    [System.Serializable]
    public class UnitData : ScriptableObject
    {
        [Header("====== UNIT SETUP")]
        public string unitName;
        public Sprite picture;
        public int goldCost;
        public float maxHealthPoints;
        public float damage;
        public float defense;
        public float atkDistance;
        public float timerToSpawn;

        public GameObject unitPrefab;
    }
}