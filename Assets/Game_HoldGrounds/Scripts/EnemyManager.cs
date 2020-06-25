using General.Utilities;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles enemies spawn and management.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        [Header("====== WAVE SETUP")]
        [Tooltip("How long player needs to wait before first wave.")]
        [SerializeField] private float initialWaitTimer = 30;
        [Tooltip("Timer to spawn a wave of enemies.")]
        [SerializeField] private float waveTimer = 60;
        [Tooltip("Do not edit this, it will auto fill when game starts.")]
        [SerializeField] [ReadOnly] private PropBehaviour[] listOfEnemyBuildings;
        [SerializeField] [ReadOnly] private float currentWaveTimer;
        
        // =============================================================================================================
        private void Start()
        {
            PrepareEnemies();
        }
        // =============================================================================================================
        private void Update()
        {
            EnemyWaveHandler();
        }
        // =============================================================================================================
        /// <summary>
        /// Prepare the scene for enemies.
        /// </summary>
        private void PrepareEnemies()
        {
            //Prepare wave
            currentWaveTimer = initialWaitTimer;
            
            //Get all enemy buildings in the scene.
            var gos = GameObject.FindGameObjectsWithTag(GameTags.TeamRed);
            listOfEnemyBuildings = new PropBehaviour[gos.Length];
            for (var i = 0; i < gos.Length; i++)
            {
                listOfEnemyBuildings[i] = gos[i].GetComponent<PropBehaviour>();
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Check when to spawn enemies.
        /// The enemies will be trained, as any other unit in game (like the ones from players).
        /// But the thing is, Enemy does not need to handle Gold management.
        /// </summary>
        private void EnemyWaveHandler()
        {
            currentWaveTimer -= Time.deltaTime;
            if (currentWaveTimer <= 0)
            {
                currentWaveTimer = waveTimer;
                //Spawn wave
                for (var i = 0; i < listOfEnemyBuildings.Length; i++)
                {
                    //Some enemy buildings might be already destroyed.
                    if (listOfEnemyBuildings[i] != null)
                        listOfEnemyBuildings[i].TrainUnit();
                }
            }
        }
        // =============================================================================================================
    }
}