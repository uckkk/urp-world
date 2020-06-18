using General.Utilities;
using TMPro;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles props in-game.
    /// </summary>
    public class PropBehaviour : MonoBehaviour
    {
        #region SETUP
        [Header("====== PROP SETUP")]
        [SerializeField] [ReadOnly] private bool isAlly;
        [Tooltip("My type of object in game.")]
        [SerializeField] private PropData propType;
        [Tooltip("Health points of this prop. If it reaches zero, it will be destroyed.")]
        [SerializeField] [ReadOnly] private float healthPoints;
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
        [Tooltip("Where a new unit will spawn.")]
        [SerializeField] private Transform unitSpawnPosition;
        [Tooltip("If there is something to animate when building.")]
        [SerializeField] private Animator buildingTrainAnimation;

        /// <summary>
        /// Get prop type of this prop.
        /// </summary>
        public PropData GetPropType => propType;
        /// <summary>
        /// Get current health points.
        /// </summary>
        public float GetHealthPoints => healthPoints;
        /// <summary>
        /// Get the unit this building can spawn.
        /// </summary>
        public CharacterData GetUnitDataType => propType.unitDataType;
        /// <summary>
        /// Action timer will work for different buildings.
        /// </summary>
        public float GetActionTimer => actionTimer;
        
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
            isAlly = CompareTag(GameTags.TeamBlue);
            healthPoints = propType.maxHealthPoints;
            CloseUiText();
            if (propType.objectType == ObjectType.BuildingFarm)
                actionTimer = propType.timerForGoldIncome;
            if (buildingTrainAnimation != null)
                buildingTrainAnimation.enabled = false;
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
        #endregion
        
        #region FARMS
        // =============================================================================================================
        /// <summary>
        /// Handle farms in game.
        /// </summary>
        private void HandleFarms()
        {
            if (propType.objectType == ObjectType.BuildingFarm)
            {
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    GameManager.Instance.GoldAdd(propType.goldGenerate);
                    actionTimer = propType.timerForGoldIncome;
                    ShowUiText("+" + propType.goldGenerate);
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
                    CharacterManager.Instance.SpawnNewUnit(propType.unitDataType, unitSpawnPosition.position, true);
                    buildingTrainAnimation.enabled = false;
                    ShowUiText("+1");
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Train unit from the current building.
        /// </summary>
        public void TrainUnit()
        {
            if (actionTimer > 0)
                return;
            actionTimer = propType.unitDataType.timerToSpawn;
            buildingTrainAnimation.enabled = true;
            Debug.Log("Training new unity...");
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
