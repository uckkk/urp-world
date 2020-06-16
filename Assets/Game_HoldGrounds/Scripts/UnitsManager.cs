using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles all units spawn and creation for the game (player or enemy).
    /// </summary>
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance { get; private set; }

        [Tooltip("List of units to create and compare.")]
        [SerializeField] private UnitData[] listOfUnits;
        
        // =============================================================================================================
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        // =============================================================================================================
        /// <summary>
        /// Creates a new unit in a given place.
        /// If Ally, it will belongs to the player. If not, it will belongs to the enemy.
        /// </summary>
        public void SpawnNewUnit(UnitData unit, Vector3 pos, bool isAlly)
        {
            for (var i = 0; i < listOfUnits.Length; i++)
            {
                if (listOfUnits[i] == unit)
                    Instantiate(listOfUnits[i].unitPrefab, pos, Quaternion.identity);
            }
        }
        // =============================================================================================================
    }
}