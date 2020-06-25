using General.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Manages the entire game (start, save, load and finish).
    /// Check the Text file "GAME BRIEFING" to know more.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region GAME SETUP
        
        public static GameManager Instance { get; private set; }
        
        [Header("====== MATCH SETUP")]
        [Tooltip("Score of the current level. Score is handled by how many units destroyed + enemy base destroy.")]
        [SerializeField] private int levelScore;
        [Tooltip("Gold/money of the current level. Gold is used to buy buildings and units in-game.")]
        [SerializeField] private int levelGold = 50;
        [Tooltip("Morale is your health in this game. Lose Morale and the game is lost.")]
        [SerializeField] private int levelMorale = 100;
        [Tooltip("Tells if the game is playable for the player, paused or not playing at all.")]
        [SerializeField] [ReadOnly] GameState gameState;
        [Tooltip("What the player is currently doing.")]
        [SerializeField] [ReadOnly] PlayerMode playerMode;
        private int _selectedBuildingId;

        [Header("====== GAME SETUP")]
        [Tooltip("You can only build on grounds with height less than this.")]
        [SerializeField] private float maxHeightToBuild = 0.05f;
        [Tooltip("How long the warning text message should be visible on screen.")]
        [SerializeField] private float warningTxtTimer = 3;
        [Tooltip("Main camera of the scene.")]
        [SerializeField] private Camera mainCamera;
        [Tooltip("Material that represents when you can't build.")]
        [SerializeField] private Material bluePrintLocked;
        [Tooltip("Material that represents when you CAN build.")]
        [SerializeField] private Material bluePrintUnlocked;
        [Tooltip("List of buildings to create in-game.")]
        [SerializeField] private BuildingData[] buildingsAvailable;
        [Tooltip("Layers that are possible to build.")]
        [SerializeField] private LayerMask buildGroundLayer;
        [Tooltip("Layers that are considered impossible to build.")]
        [SerializeField] private LayerMask bluePrintBadLayers;
        [Tooltip("Layers that are possible to select with a click in scene.")]
        [SerializeField] private LayerMask selectionAvailableLayers;
        /// <summary>
        /// Current prop selected in the scene (it can be a building).
        /// </summary>
        [SerializeField] [ReadOnly] private PropBehaviour propSelected;
        
        [Header("====== UI SETUP")]
        [SerializeField] private GameObject uiCanvas;
        [SerializeField] private TextMeshProUGUI uiGoldText;
        [SerializeField] private TextMeshProUGUI uiMoraleText;
        [SerializeField] private TextMeshProUGUI uiWarningText;
        [SerializeField] private GameObject uiBuildingBtnPrefab;
        [SerializeField] private Transform uiButtonsParent;
        [SerializeField] private GameObject uiBuildingDetails;
        [SerializeField] private Image uiBuildSelIcon;
        [SerializeField] private TextMeshProUGUI uiBuildSelName; //building selected name
        [SerializeField] private TextMeshProUGUI uiBuildSelHealth;
        [SerializeField] private TextMeshProUGUI uiBuildSelUnitName;
        [SerializeField] private TextMeshProUGUI uiBuildSelField1;
        [SerializeField] private TextMeshProUGUI uiBuildSelField2;
        [SerializeField] private Button uiBuildSelTrainBtn;
        [SerializeField] private Image uiBuildSelUnitImg;
        [SerializeField] private TextMeshProUGUI uiBuildSelTrainStatus;
        [SerializeField] private Sprite uiGoldIcon;
        
        /// <summary>
        /// Current building blue print selected to build.
        /// </summary>
        private BuildingData _buildingToBuild;
        /// <summary>
        /// Current unit to build selected by the selected building.
        /// </summary>
        private CharacterData _unitToBuild;
        private float _unitTimerToBuild;
        private float _unitMaxTimerToBuild;
        
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
        private void Start()
        {
            PrepareMatch();
        }
        // =============================================================================================================
        private void Update()
        {
            HandleSelection();
            HandleBuilding();
            UpdateBuildingUi();
        }
        // =============================================================================================================
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            if (_buildingToBuild == null)
                return;
            if (_buildingToBuild.bluePrintCollision == null)
                return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_buildingToBuild.bluePrintCollision.position,
                _buildingToBuild.bluePrintCollision.localScale);
        }
        // =============================================================================================================
        /// <summary>
        /// Prepares a new match.
        /// </summary>
        private void PrepareMatch()
        {
            //Set game status
            gameState = GameState.Playing;
            playerMode = PlayerMode.InScene;
            CloseWarningText();
            
            //Set starting data
            uiCanvas.SetActive(true);
            UpdatePlayerHud();
            for (var i = 0; i < buildingsAvailable.Length; i++)
            {
                var uiGo = Instantiate(uiBuildingBtnPrefab, uiButtonsParent);
                var script = uiGo.GetComponent<UiBuildingButton>();
                script.SetProp(buildingsAvailable[i].propData);
            }
            uiBuildingDetails.SetActive(false);
            
            //Prepare buildings blue prints
            for (var i = 0; i < buildingsAvailable.Length; i++)
            {
                buildingsAvailable[i].PrepareBluePrint();
                buildingsAvailable[i].SetRenderersMaterial(true, bluePrintLocked);
                buildingsAvailable[i].ToggleBluePrint(false);
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Adds gold to the player.
        /// </summary>
        public void GoldAdd(int goldAmount)
        {
            levelGold += goldAmount;
            UpdatePlayerHud();
        }
        // =============================================================================================================
        #endregion
        
        #region BUILDINGS
        // =============================================================================================================
        /// <summary>
        /// Handles the building process.
        /// </summary>
        private void HandleBuilding()
        {
            if (gameState != GameState.Playing)
                return;
            
            if (playerMode != PlayerMode.BuildingMode)
                return;
            
            if (_buildingToBuild == null)
                return;
            
            //Are we point into UI instead?
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            //Check hit position to build.
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo, 100, buildGroundLayer))
            {
                _buildingToBuild.bluePrint.position = hitInfo.point;
                //Cancel Build
                if (Input.GetButtonDown("Fire2"))
                {
                    CancelBuilding();
                    return;
                }
                //Check if there is something colliding with the blue print, if yes, not possible to build.
                var hits = Physics.OverlapBox(_buildingToBuild.bluePrintCollision.position,
                    _buildingToBuild.bluePrintCollision.localScale / 2, Quaternion.identity, bluePrintBadLayers);
                if (hits.Length > 0 || _buildingToBuild.bluePrint.position.y > maxHeightToBuild)
                {
                    BuildingCheckTouch(true);
                    return;
                }
                BuildingCheckTouch(false);
                
                //Build
                if (Input.GetButtonDown("Fire1"))
                {
                    SpawnBuilding();
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Handles what the player can select in the scene/scenery.
        /// </summary>
        private void HandleSelection()
        {
            if (gameState != GameState.Playing)
                return;
            
            if (playerMode != PlayerMode.InScene)
                return;

            //Are we point into UI instead?
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            if (Input.GetButtonDown("Fire2"))
            {
                CancelSelection();
            }
            
            if (Input.GetButtonDown("Fire1"))
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hitInfo, 1000, selectionAvailableLayers))
                {
                    if (hitInfo.transform.CompareTag(GameTags.TeamBlue))
                    {
                        BuildingSelFromScene(hitInfo.transform.GetComponent<PropBehaviour>());
                        return;
                    }
                    CancelSelection();
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Creates a building in a given position and rotation.
        /// </summary>
        private void SpawnBuilding()
        {
            if (_buildingToBuild == null)
                return;
            if (levelGold < _buildingToBuild.propData.goldCost)
            {
                ShowWarningText("Not enough gold!");
                return;
            }
            levelGold -= _buildingToBuild.propData.goldCost;
            var pos = _buildingToBuild.bluePrint.position;
            var building = Instantiate(_buildingToBuild.prefabToCreate, pos, _buildingToBuild.bluePrint.rotation);
            building.tag = GameTags.TeamBlue;
            VfxManager.Instance.CallVFx(0, pos, Quaternion.identity);
            CameraBehaviour.Instance.ShakeCamera_Building();
            UpdatePlayerHud();
            //Clear selected building
            CancelBuilding();
        }
        // =============================================================================================================
        /// <summary>
        /// Cancel the building selection.
        /// </summary>
        private void CancelBuilding()
        {
            _buildingToBuild.ToggleBluePrint(false);
            _buildingToBuild = null;
            playerMode = PlayerMode.InScene;
        }
        // =============================================================================================================
        /// <summary>
        /// Cancel the selection of a building in game.
        /// </summary>
        private void CancelSelection()
        {
            propSelected = null;
            uiBuildingDetails.SetActive(false);
        }
        // =============================================================================================================
        /// <summary>
        /// Checks if a building is touching something that will lock or unlock the creation.
        /// </summary>
        /// <param name="isLocked"></param>
        private void BuildingCheckTouch(bool isLocked)
        {
            _buildingToBuild?.SetRenderersMaterial(isLocked, isLocked ? bluePrintLocked : bluePrintUnlocked);
        }
        // =============================================================================================================
        /// <summary>
        /// Select a building to build and make it visible in UI. Check array order in buildingsAvailable.
        /// </summary>
        /// <param name="buildingData"></param>
        public void BuildingSelToBuild(PropData buildingData)
        {
            for (var i = 0; i < buildingsAvailable.Length; i++)
            {
                buildingsAvailable[i].ToggleBluePrint(false);
                if (buildingsAvailable[i].propData == buildingData)
                    _selectedBuildingId = i;
            }
            _buildingToBuild = buildingsAvailable[_selectedBuildingId];
            _buildingToBuild.ToggleBluePrint(true);
            playerMode = PlayerMode.BuildingMode;
        }
        // =============================================================================================================
        /// <summary>
        /// Select a building from the scene (already built).
        /// </summary>
        private void BuildingSelFromScene(PropBehaviour building)
        {
            propSelected = building;
            _unitToBuild = propSelected.GetUnitDataType;
            uiBuildingDetails.SetActive(true);
            uiBuildSelIcon.sprite = propSelected.GetPropType.picture;
            uiBuildSelName.text = propSelected.GetPropType.propName;
            if (propSelected.GetPropType.objectType == ObjectType.BuildingFarm)
            {
                uiBuildSelUnitName.text = "Gold income";
                uiBuildSelField1.text = "<color=yellow>+" + propSelected.GetPropType.goldGenerate;
                uiBuildSelField2.text = "";
                uiBuildSelUnitImg.sprite = uiGoldIcon;
                uiBuildSelTrainBtn.interactable = false;
            }
            else if (propSelected.GetPropType.objectType == ObjectType.BuildingBarracks ||
                     propSelected.GetPropType.objectType == ObjectType.BuildingDefenseTw ||
                     propSelected.GetPropType.objectType == ObjectType.BuildingMagicTw)
            {
                uiBuildSelUnitName.text = _unitToBuild.unitName + " (" + _unitToBuild.goldCost + " G)";
                uiBuildSelField1.text = "<color=red>ATK: " + _unitToBuild.damage;
                uiBuildSelField2.text = "<color=blue>DEF: " + _unitToBuild.defense;
                uiBuildSelUnitImg.sprite = _unitToBuild.picture;
                uiBuildSelTrainBtn.interactable = true;
            }
            UpdateBuildingUi();
        }
        // =============================================================================================================
        /// <summary>
        /// Train a unit from the selected building.
        /// </summary>
        public void BuildingTrainUnit()
        {
            if (propSelected != null && _unitToBuild != null)
            {
                if (propSelected.GetActionTimer > 0)
                {
                    ShowWarningText("Already training a unit!");
                    return;
                }
                if (levelGold < _unitToBuild.goldCost)
                {
                    ShowWarningText("Not enough gold to train!");
                    return;
                }
                if (propSelected.GetActionTimer > 0)
                    return;
                propSelected.TrainUnit();
                levelGold -= _unitToBuild.goldCost;
                UpdatePlayerHud();
                UpdateBuildingUi();
            }
        }
        // =============================================================================================================
        #endregion
        
        #region UI DATA
        // =============================================================================================================
        /// <summary>
        /// Shows a warning message on screen for the player.
        /// </summary>
        /// <param name="txt"></param>
        private void ShowWarningText(string txt)
        {
            uiWarningText.text = txt;
            if (IsInvoking(nameof(CloseWarningText)))
                CancelInvoke(nameof(CloseWarningText));
            Invoke(nameof(CloseWarningText), warningTxtTimer);
        }
        private void CloseWarningText()
        {
            uiWarningText.text = "";
        }
        // =============================================================================================================
        /// <summary>
        /// Update player hud on all values (gold, morale, etc).
        /// </summary>
        private void UpdatePlayerHud()
        {
            uiGoldText.text = levelGold.ToString();
            uiMoraleText.text = levelMorale + "%";
        }
        // =============================================================================================================
        /// <summary>
        /// Update building UI if selected.
        /// </summary>
        private void UpdateBuildingUi()
        {
            if (propSelected == null)
                return;
            uiBuildSelHealth.text = "Health: " + propSelected.GetHealthPoints;
            uiBuildSelTrainStatus.text = propSelected.GetBuildActionTimerStatus();
        }
        // =============================================================================================================
        #endregion
    }

    /// <summary>
    /// Tells the game state (running, paused, etc).
    /// </summary>
    public enum GameState
    {
        NotPlaying = 0,
        Playing = 1,
        Paused = 2,
        FinishedWin = 3,
        FinishedLose = 4
    }
    /// <summary>
    /// In player mode, I can tell if I am selecting a building, a unit or just moving camera.
    /// </summary>
    public enum PlayerMode
    {
        InScene = 0,
        BuildingMode = 1,
        UnitMode = 2
    }
    /// <summary>
    /// Object ID for all objects that it is in-game
    /// </summary>
    public enum ObjectType
    {
        BuildingFarm = 0,
        BuildingBarracks = 1,
        BuildingDefenseTw = 2,
        BuildingMagicTw = 3,
        NatureTree = 4,
        NatureRock = 5
    }
    /// <summary>
    /// Tags in game, to be easier to modify in case we need.
    /// </summary>
    public static class GameTags
    {
        public const string TeamRed = "TeamRed";
        public const string TeamBlue = "TeamBlue";
        public const string FlagRed = "FlagRed";
        public const string FlagBlue = "FlagBlue";
    }
}
