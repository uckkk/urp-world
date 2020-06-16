using General.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles props in-game.
    /// </summary>
    public class PropBehaviour : MonoBehaviour
    {
        #region SETUP
        [Header("====== PROP SETUP")]
        [Tooltip("My type of object in game.")]
        [SerializeField] private PropData propType;
        [Tooltip("Health points of this prop. If it reaches zero, it will be destroyed.")]
        [SerializeField] [ReadOnly] private float healthPoints;
        
        [Header("====== UI DATA")]
        [Tooltip("Window to be animated.")]
        [SerializeField] private GameObject uiWindow;
        [Tooltip("Show text data when needed here.")]
        [SerializeField] private TextMeshProUGUI uiText;
        [SerializeField] private float uiTimerOnScreen = 2;
        
        [Header("====== SPAWN UNITS ONLY")]
        [Tooltip("What kind of Unit can this building create.")]
        [SerializeField] private UnitData unitDataType;
        [Tooltip("Where a new unit will spawn.")]
        [SerializeField] private Transform unitSpawnPosition;

        /// <summary>
        /// Our action timer will work for different buildings.
        /// For farms, it works to control how long it takes to generate gold.
        /// For barracks, it handles the timer to create a new unity.
        /// </summary>
        [FormerlySerializedAs("_actionTimer")] [SerializeField] private float actionTimer;

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
        public UnitData GetUnitDataType => unitDataType;
        
        // =============================================================================================================
        private void Start()
        {
            PrepareProp();
        }
        // =============================================================================================================
        private void Update()
        {
            Handle_Farms();
            Handle_Barracks();
        }
        // =============================================================================================================
        /// <summary>
        /// Prepare the correct behaviour of this prop.
        /// </summary>
        private void PrepareProp()
        {
            healthPoints = propType.maxHealthPoints;
            CloseUiText();
            if (propType.objectType == ObjectType.BuildingFarm)
                actionTimer = propType.timerForGoldIncome;
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
        private void Handle_Farms()
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
        
        #region BARRACKS
        // =============================================================================================================
        /// <summary>
        /// Handles Barracks behaviours in game.
        /// </summary>
        private void Handle_Barracks()
        {
            if (propType.objectType == ObjectType.BuildingBarracks)
            {
                //In barracks, timer will count when the player chose to build a unit.
                if (actionTimer <= 0)
                    return;
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    UnitsManager.Instance.SpawnNewUnit(unitDataType, unitSpawnPosition.position, true);
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
            
        }
        // =============================================================================================================
        /// <summary>
        /// Get the progress of the unit creation progress (in %).
        /// </summary>
        /// <returns></returns>
        public string GetUnitBuildStatus()
        {
            if (actionTimer > 0)
                return (actionTimer / unitDataType.timerToSpawn).ToString("f0") + "%";
            return "";
        }
        // =============================================================================================================
        #endregion
    }
}
