using General.Utilities;
using TMPro;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles props in-game, for players or enemies.
    /// </summary>
    public class BuildingBehaviour : LiveObject
    {
        #region SETUP
        [Header("====== SETUP")]
        [Tooltip("My type of object in game.")]
        [SerializeField] private PropData propType;
        /// <summary>
        /// Our action timer will work for different buildings.
        /// For farms, it works to control how long it takes to generate gold.
        /// For barracks, it handles the timer to create a new unity.
        /// </summary>
        [SerializeField] private float actionTimer;

        [Header("====== UI DATA")]
        [Tooltip("Window to be animated.")]
        [SerializeField] private GameObject uiWindow;
        [Tooltip("Show text data when needed here.")]
        [SerializeField] private TextMeshProUGUI uiText;
        [SerializeField] private float uiTimerOnScreen = 2;
        
        [Header("====== SPAWN UNITS ONLY")]
        [Tooltip("Even this building type can spawn units, set this to true to never spawn.")]
        [SerializeField] private bool spawnLocked;
        [Tooltip("Where a new unit will spawn.")]
        [SerializeField] private Transform unitSpawnPosition;
        [Tooltip("If there is something to animate when building.")]
        [SerializeField] private Animator buildingTrainAnimation;
        
        [Header("====== FARMS ONLY")]
        [Tooltip("The layers to search for a tree.")]
        [SerializeField] private LayerMask layerForTrees;
        [Tooltip("If there is something to animate when building.")]
        [SerializeField] [ReadOnly] private int goldBonusPerTree;

        /// <summary>
        /// Get prop type of this prop.
        /// </summary>
        public PropData GetPropType => propType;
        /// <summary>
        /// Get current health points.
        /// </summary>
        public float GetHealthPoints => GetHealth;
        /// <summary>
        /// Get the unit this building can spawn.
        /// </summary>
        public CharacterData GetUnitDataType => propType.unitDataType;
        /// <summary>
        /// Action timer will work for different buildings.
        /// </summary>
        public float GetActionTimer => actionTimer;
        /// <summary>
        /// Get if it building is still working.
        /// </summary>
        public bool IsEnabled => IsActivated;
        /// <summary>
        /// Get if it is Ally.
        /// </summary>
        public bool IsItAlly => IsAlly;
        /// <summary>
        /// Get damage that his building can do.
        /// </summary>
        public float GetDamage => propType.defenseDamage;
        /// <summary>
        /// Get how fast it can take, in case it can.
        /// </summary>
        public float GetAtkRate => propType.defenseAttackRate;
        /// <summary>
        /// Get how much bonus this Farm has because there are trees nearby.
        /// </summary>
        public int GetGoldBonusPerTree => goldBonusPerTree;

        /// <summary>
        /// We will use this to change the shader properties.
        /// </summary>
        [SerializeField] [ReadOnly] private Renderer[] myRenderers;
        
        /// <summary>
        /// Name of the shader property. This one in Shader Graph is boolean, but in code it is interpreted as
        /// float, being either 0 or 1.
        /// </summary>
        private static readonly int ShaderHighlight = Shader.PropertyToID("_Highlight");

        // =============================================================================================================
        private void Start()
        {
            PrepareProp();
        }
        // =============================================================================================================
        private void Update()
        {
            HandleFarms();
            HandleTrainingUnits();
        }
        // =============================================================================================================
        /// <summary>
        /// Prepare the correct behaviour of this prop.
        /// </summary>
        private void PrepareProp()
        {
            //Check if it is active
            if (GameManager.Instance.GetGameState == GameState.Playing)
                SetActivated(true);
            
            //Check what team it belongs to
            if (CompareTag(GameTags.TeamBlue))
                SetAlly();
            SetHealth(propType.maxHealthPoints);
            CloseUiText();
            if (propType.objectType == ObjectType.BuildingFarm)
            {
                actionTimer = propType.timerForGoldIncome;
                //Look for trees near the farm to generate extra gold.
                var possibleHits = new Collider[10];
                var size = Physics.OverlapSphereNonAlloc(transform.position, propType.extraGoldSearchRadius,
                    possibleHits, layerForTrees);
                goldBonusPerTree += size * propType.extraGoldPerTree;
            }
            if (buildingTrainAnimation != null)
                buildingTrainAnimation.enabled = false;
            
            //Get renderers
            myRenderers = gameObject.GetComponentsInChildren<Renderer>();
        }
        // =============================================================================================================
        /// <summary>
        /// Shows a 3D ui text.
        /// </summary>
        /// <param name="txt"></param>
        private void ShowUiText(string txt)
        {
            uiText.text = txt;
            uiWindow.SetActive(true);
            if (IsInvoking(nameof(CloseUiText)))
                return;
            Invoke(nameof(CloseUiText), uiTimerOnScreen);
        }
        private void CloseUiText()
        {
            if (uiWindow == null || uiText == null)
                return;
            
            uiWindow.SetActive(false);
            uiText.text = "";
        }
        // =============================================================================================================
        /// <summary>
        /// To be called when this object is destroyed.
        /// </summary>
        protected override void OnObjectDestroyed()
        {
            VfxManager.Instance.CallVFx(2, transform.position, Quaternion.identity);
            VfxManager.Instance.CallVFx(12, transform.position, Quaternion.identity);
            if (IsAlly)
                GameManager.Instance.MoraleAdd(-1);
            else
                GameManager.Instance.ScorePerBuilding();
            Destroy(gameObject);
        }
        // =============================================================================================================
        /// <summary>
        /// Check if this building was selected by the player, just change it's visual.
        /// </summary>
        /// <param name="sel"></param>
        public void BuildingSelectedToggle(bool sel)
        {
            for (var x = 0; x < myRenderers.Length; x++)
            {
                for (var i = 0; i < myRenderers[x].materials.Length; i++)
                {
                    myRenderers[x].materials[i].SetFloat(ShaderHighlight, sel ? 1 : 0);
                }
            }
        }
        // =============================================================================================================
        #endregion
        
        #region FARMS
        // =============================================================================================================
        /// <summary>
        /// Handle farms in game.
        /// </summary>
        private void HandleFarms()
        {
            if (!IsActivated)
                return;
            
            if (propType.objectType == ObjectType.BuildingFarm)
            {
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    var totalGold = propType.goldGenerate + goldBonusPerTree;
                    if (IsAlly)
                        GameManager.Instance.GoldAdd(totalGold);
                    actionTimer = propType.timerForGoldIncome;
                    ShowUiText("+" + totalGold);
                }
            }
        }
        // =============================================================================================================
        #endregion
        
        #region BARRACKS, DEF TW, MAGIC TW
        // =============================================================================================================
        /// <summary>
        /// Handles training units behaviours in game.
        /// </summary>
        private void HandleTrainingUnits()
        {
            if (!IsActivated)
                return;
            
            if (propType.objectType == ObjectType.BuildingBarracks ||
                propType.objectType == ObjectType.BuildingDefenseTw ||
                propType.objectType == ObjectType.BuildingMagicTw)
            {
                //Timer will count when the player chose to build a unit.
                if (actionTimer <= 0)
                    return;
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    CharacterManager.Instance.SpawnNewUnit(propType.unitDataType, unitSpawnPosition.position, IsAlly);
                    buildingTrainAnimation.enabled = false;
                    ShowUiText("+1");
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Train unit from the current building.
        /// Gold calculations is done in GameManager.
        /// </summary>
        public void TrainUnit()
        {
            if (actionTimer > 0 || spawnLocked)
                return;
            actionTimer = propType.unitDataType.timerToSpawn;
            buildingTrainAnimation.enabled = true;
        }
        // =============================================================================================================
        /// <summary>
        /// Get the progress of the unit creation progress (in %)
        /// or in case of Farms, the gold income.
        /// </summary>
        /// <returns></returns>
        public string GetBuildActionTimerStatus()
        {
            if (actionTimer > 0)
            {
                if (propType.objectType == ObjectType.BuildingFarm)
                    return (100 - (actionTimer / propType.timerForGoldIncome * 100)).ToString("f0") + "%";
                //Barracks, Archer Tower and Wizard Tower
                return (100 - (actionTimer / propType.unitDataType.timerToSpawn * 100)).ToString("f0") + "%";
            }
            return "";
        }
        // =============================================================================================================
        #endregion
    }
}
