using General.Utilities;
using UnityEngine;
using TMPro;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles enemies spawn and management.
    /// Enemies will be spawned from each Enemy building available, each given seconds.
    /// But also, some will come from the portal and these waves will be harder and harder (another timer).
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        [Header("====== WAVE SETUP")]
        [Tooltip("How long player needs to wait before first wave.")]
        [SerializeField] private float initialWaitTimer = 30;
        [Tooltip("Timer to enemy train units, this will never change.")]
        [SerializeField] private float trainUnitsTimer = 30;
        [Tooltip("Timer to spawn a wave of enemies. This will increase per each wave.")]
        [SerializeField] private float waveTimer = 60;
        [Tooltip("wait times for waves will be no longer than this.")]
        [SerializeField] private float maxWaveTimer = 300;
        [Tooltip("Where the big wave spawn. Remember that enemy buildings will also train units. It will auto set." +
                 "You need a gameObject with the correct tag to search and auto set.")]
        [SerializeField] [ReadOnly] private Transform portalSpawnPosition;
        [Tooltip("How many units, per counter and per wave, it will spawn from the portal.")]
        [SerializeField] private CharacterData[] unitsPortalSpawnPerWave;
        [Tooltip("Do not edit this, it will auto fill when game starts.")]
        [SerializeField] [ReadOnly] private BuildingBehaviour[] listOfEnemyBuildings;
        [SerializeField] [ReadOnly] private float currentTrainUnitsTimer;
        [SerializeField] [ReadOnly] private float currentWaveTimer;
        [Tooltip("How many waves already gone...")]
        [SerializeField] [ReadOnly] private int waveCounter;
        
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
            //Check spawn position
            var portal = GameObject.FindGameObjectWithTag(GameTags.PortalSpawn);
            if (portal == null)
            {
                Debug.LogError("No portal spawn position for waves!");
                return;
            }
            portalSpawnPosition = portal.transform;
            
            //Prepare wave
            currentWaveTimer = initialWaitTimer;
            currentTrainUnitsTimer = initialWaitTimer;
            waveCounter = 0;
            
            //Get all enemy buildings in the scene.
            var gos = GameObject.FindGameObjectsWithTag(GameTags.TeamRed);
            listOfEnemyBuildings = new BuildingBehaviour[gos.Length];
            for (var i = 0; i < gos.Length; i++)
            {
                listOfEnemyBuildings[i] = gos[i].GetComponent<BuildingBehaviour>();
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Check when to spawn enemies.
        /// The enemies will be trained, as any other unit in game (like the ones from players).
        /// But the thing is, Enemy does not need to handle Gold management.
        /// Also, there will be enemies spawned from the portal (the Portal that the player needs to destroy).
        /// </summary>
        private void EnemyWaveHandler()
        {
            //TRAIN UNITS
            currentTrainUnitsTimer -= Time.deltaTime;
            if (currentTrainUnitsTimer <= 0)
            {
                currentTrainUnitsTimer = trainUnitsTimer;
                //Train units at the enemy buildings
                for (var i = 0; i < listOfEnemyBuildings.Length; i++)
                {
                    //Some enemy buildings might be already destroyed.
                    if (listOfEnemyBuildings[i] != null)
                        listOfEnemyBuildings[i].TrainUnit();
                }
            }
            
            //WAVES
            currentWaveTimer -= Time.deltaTime;
            GameManager.Instance.SetUiWaveTimer(currentWaveTimer);
            if (currentWaveTimer <= 0)
            {
                waveCounter++;
                currentWaveTimer = waveTimer * waveCounter;
                if (currentWaveTimer > maxWaveTimer)
                    currentWaveTimer = maxWaveTimer;
                //Spawn wave, each time will be harder
                for (var i = 0; i < unitsPortalSpawnPerWave.Length; i++)
                {
                    for (var x = 0; x < waveCounter; x++)
                    {
                        CharacterManager.Instance.SpawnNewUnit(unitsPortalSpawnPerWave[i],
                            portalSpawnPosition.position, false);
                    }
                }
            }
        }
        // =============================================================================================================
    }
}