using System.Collections;
using General.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Manage all game levels.
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
        [SerializeField] private int levelGold = 100;
        [Tooltip("Morale is your health in this game. Lose Morale and the game is lost.")]
        [SerializeField] private int levelMorale = 100;
        [Tooltip("Keep track of units lost.")]
        [SerializeField] private int unitsLost;
        [Tooltip("Keep track of units destroyed (not buildings).")]
        [SerializeField] private int unitsDestroyed;
        [Tooltip("How much score you make when you destroy an unit.")]
        [SerializeField] private int scorePerUnit = 1;
        [Tooltip("How much score you make when you destroy a building.")]
        [SerializeField] private int scorePerBuilding = 1;
        [Tooltip("Tells if the game is playable for the player, paused or not playing at all.")]
        [SerializeField] [ReadOnly] private GameState gameState;
        [Tooltip("What the player is currently doing.")]
        [SerializeField] [ReadOnly] private PlayerMode playerMode;
        [Tooltip("Player flag, if the enemy gets here, it is game over. It will auto search during start.")]
        [SerializeField] [ReadOnly] private Transform flagBlue;
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Header("====== GAME SETUP")]
        [Tooltip("You can only build on grounds with height less than this.")]
        [SerializeField] private float maxHeightToBuild = 0.05f;
        [Tooltip("Max distance from the Player flag to build anything.")]
        [SerializeField] private float maxRadiusToBuild = 40;
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
        [Tooltip("We will add here all buildings the player created, so we can search for it later.")]
        [SerializeField] private Transform parentForBuildings;
        /// <summary>
        /// Current prop selected in the scene (it can be a building).
        /// </summary>
        [SerializeField] [ReadOnly] private BuildingBehaviour buildingSelected;
        [Tooltip("Is the player attacking or defending?")]
        [SerializeField] [ReadOnly] private bool isAttacking;
        
        [Header("====== UI SETUP")]
        [SerializeField] private GameObject uiCanvas;
        [SerializeField] private GameObject uiPlayerHud;
        [SerializeField] private GameObject uiFinishHud;
        [SerializeField] private GameObject uiWinHud;
        [SerializeField] private GameObject uiLoseHud;
        [SerializeField] private TextMeshProUGUI uiGoldText;
        [SerializeField] private TextMeshProUGUI uiMoraleText;
        [SerializeField] private TextMeshProUGUI uiScoreText;
        [SerializeField] private TextMeshProUGUI uiWarningText;
        [SerializeField] private TextMeshProUGUI uiGamePaceText;
        [SerializeField] private GameObject uiBuildingBtnPrefab;
        [SerializeField] private Transform uiButtonsParent;
        [SerializeField] private GameObject uiBuildingDetails;
        [SerializeField] private Image uiBuildSelIcon;
        [SerializeField] private TextMeshProUGUI uiBuildSelName; //building selected name
        [SerializeField] private TextMeshProUGUI uiBuildSelHealth;
        [SerializeField] private TextMeshProUGUI uiBuildSelDmg;
        [SerializeField] private TextMeshProUGUI uiBuildSelAtkRate;
        [SerializeField] private TextMeshProUGUI uiBuildSelUnitName;
        [SerializeField] private TextMeshProUGUI uiBuildSelField1;
        [SerializeField] private TextMeshProUGUI uiBuildSelField2;
        [SerializeField] private Button uiBuildSelTrainBtn;
        [SerializeField] private Image uiBuildSelUnitImg;
        [SerializeField] private TextMeshProUGUI uiBuildSelTrainStatus;
        [SerializeField] private Sprite uiGoldIcon;
        [SerializeField] private GameObject uiMoveModeDetails;
        [SerializeField] private TextMeshProUGUI uiMoveModeText;
        [SerializeField] private TextMeshProUGUI uiWaveTimer;
        [SerializeField] private TextMeshProUGUI uiStatsScore;
        [SerializeField] private TextMeshProUGUI uiStatsUnitsLost;
        [SerializeField] private TextMeshProUGUI uiStatsUnitsDestroyed;
        
        /// <summary>
        /// Current building blue print selected to build.
        /// </summary>
        private BuildingData buildingToBuild;
        /// <summary>
        /// Current unit to build selected by the selected building.
        /// </summary>
        private CharacterData unitToBuild;
        private float unitTimerToBuild;
        private float unitMaxTimerToBuild;
        
        //Event for game state
        public delegate void OnCallGameState(GameState gState);
        public static event OnCallGameState OnGameStateChange;
        
        //Event for attacking and defending
        public delegate void OnCallAttackMode(bool atk);
        public static event OnCallAttackMode OnAttackModeComplete;
        
        /// <summary>
        /// Get game state.
        /// </summary>
        public GameState GetGameState => gameState;
        /// <summary>
        /// Get if the player is attacking or defending
        /// </summary>
        public bool GetIfIsAttacking => isAttacking;

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
            StartCoroutine(PrepareMatch());
        }
        // =============================================================================================================
        private void Update()
        {
            HandleDebugs();
            HandleSelection();
            HandleBuilding();
            UpdateBuildingUi();
        }
        // =============================================================================================================
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            if (buildingToBuild == null)
                return;
            if (buildingToBuild.bluePrintCollision == null)
                return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(buildingToBuild.bluePrintCollision.position,
                buildingToBuild.bluePrintCollision.localScale);
        }
        // =============================================================================================================
        /// <summary>
        /// Prepares a new match.
        /// </summary>
        private IEnumerator PrepareMatch()
        {
            uiCanvas.SetActive(false);

            if (CoreManager.Instance.GetLevelData == null)
            {
                Debug.LogError("There is no level data! If you are testing, just add one.");
                yield break;
            }
            
            //Get level details
            levelGold = CoreManager.Instance.GetLevelData.startingGold;
            levelMorale = CoreManager.Instance.GetLevelData.startingMorale;
            
            //Load our level and wait before continuing
            yield return StartCoroutine(LoadAdditiveScene(CoreManager.Instance.GetLevelData.sceneName));
            
            //Search the main flags
            var goFlag1 = GameObject.FindGameObjectWithTag(GameTags.FlagBlue);
            var goFlag2 = GameObject.FindGameObjectWithTag(GameTags.FlagRed);
            if (goFlag1 == null || goFlag2 == null)
            {
                Debug.LogError("ERROR: no flag found, required to play the game!");
                yield break;
            }
            flagBlue = goFlag1.transform;
            // flagRed = goFlag2.transform;
            
            yield return new WaitForSeconds(1);
            
            //Set starting data
            CloseWarningText();
            AttackMode(false);
            for (var i = 0; i < buildingsAvailable.Length; i++)
            {
                var uiGo = Instantiate(uiBuildingBtnPrefab, uiButtonsParent);
                var script = uiGo.GetComponent<UiBuildingButton>();
                script.SetProp(buildingsAvailable[i].propData);
            }
            uiBuildingDetails.SetActive(false);
            uiMoveModeDetails.SetActive(true);
            
            //Prepare buildings blue prints
            for (var i = 0; i < buildingsAvailable.Length; i++)
            {
                buildingsAvailable[i].PrepareBluePrint();
                buildingsAvailable[i].SetRenderersMaterial(true, bluePrintLocked);
                buildingsAvailable[i].ToggleBluePrint(false);
            }

            //Now we can start playing
            SetGameState(GameState.Playing);
            playerMode = PlayerMode.InScene;
            uiCanvas.SetActive(true);
            uiPlayerHud.SetActive(true);
            uiFinishHud.SetActive(false);
            uiWinHud.SetActive(false);
            uiLoseHud.SetActive(false);
            UpdatePlayerHud();
            ChangeGameSpeed(1);
        }
        // =============================================================================================================
        /// <summary>
        /// Load the current sub-scene for the player to spawn.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadAdditiveScene(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        // =============================================================================================================
        #endregion
        
        #region MISC
        
        // =============================================================================================================
        /// <summary>
        /// Set game state, it will affect all units and buildings.
        /// </summary>
        /// <param name="gState"></param>
        private void SetGameState(GameState gState)
        {
            Debug.Log("Game state changed: " + gState);
            gameState = gState;
            OnGameStateChange?.Invoke(gameState);
        }
        // =============================================================================================================
        /// <summary>
        /// Adds score when destroy an unit.
        /// </summary>
        public void ScorePerUnit()
        {
            levelScore += scorePerUnit;
            UpdatePlayerHud();
        }
        // =============================================================================================================
        /// <summary>
        /// Adds score when destroy a building.
        /// </summary>
        public void ScorePerBuilding()
        {
            levelScore += scorePerBuilding;
            UpdatePlayerHud();
        }
        // =============================================================================================================
        /// <summary>
        /// Adds or remove gold to the player.
        /// </summary>
        public void GoldAdd(int goldAmount)
        {
            levelGold += goldAmount;
            UpdatePlayerHud();
        }
        // =============================================================================================================
        /// <summary>
        /// Adds or removes Morale to the player.
        /// </summary>
        public void MoraleAdd(int amount)
        {
            levelMorale += amount;
            UpdatePlayerHud();
            
            //Check if player lost
            if (levelMorale <= 0)
            {
                levelMorale = 0;
                TriggerFinishGame(false);
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Handles debug commands like toggle HUD and cheat codes.
        /// </summary>
        private void HandleDebugs()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                uiCanvas.SetActive(!uiCanvas.activeSelf);
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Adds a player unit was lost.
        /// </summary>
        public void AddUnitLost()
        {
            unitsLost++;
        }
        // =============================================================================================================
        /// <summary>
        /// Adds an enemy unit was destroyed.
        /// </summary>
        public void AddUnitDestroyed()
        {
            unitsDestroyed++;
        }
        // =============================================================================================================
        /// <summary>
        /// Change time scale speed.
        /// </summary>
        public void ChangeGameSpeed(float speedOption)
        {
            if (speedOption == 0)
                uiGamePaceText.text = CoreManager.Instance.Language_GetTextById("paused");
            else if (speedOption == 0.5f)
                uiGamePaceText.text = CoreManager.Instance.Language_GetTextById("halfSpeed");
            else if (speedOption == 1)
                uiGamePaceText.text = CoreManager.Instance.Language_GetTextById("normalSpeed");
            else if (speedOption > 1)
                uiGamePaceText.text = CoreManager.Instance.Language_GetTextById("fastSpeed");
            Time.timeScale = speedOption;
        }
        // =============================================================================================================
        /// <summary>
        /// Player won or lost.
        /// </summary>
        public void TriggerFinishGame(bool playerWon)
        {
            SetGameState(playerWon ? GameState.FinishedWin : GameState.FinishedLose);
            ChangeGameSpeed(1);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("Player won? " + playerWon);
            uiPlayerHud.SetActive(false);
            uiFinishHud.SetActive(true);
            uiWinHud.SetActive(playerWon);
            uiLoseHud.SetActive(!playerWon);
            uiStatsScore.text = levelScore.ToString();
            uiStatsUnitsLost.text = unitsLost.ToString();
            uiStatsUnitsDestroyed.text = unitsDestroyed.ToString();
        }
        // =============================================================================================================
        /// <summary>
        /// Exit game. Duh.
        /// </summary>
        public void ExitGame()
        {
            Application.Quit();
        }
        // =============================================================================================================
        /// <summary>
        /// Loads back the main menu.
        /// </summary>
        public void BackToMainMenu()
        {
            uiCanvas.SetActive(false);
            CoreManager.Instance.LoadScene(mainMenuScene);
        }
        // =============================================================================================================
        #endregion
        
        #region ATK or DEF/ MODE
        
        // =============================================================================================================
        /// <summary>
        /// Attack or defend mode. True is for attack.
        /// </summary>
        /// <param name="toggle"></param>
        public void AttackMode(bool toggle)
        {
            isAttacking = toggle;
            uiMoveModeText.text = toggle ? CoreManager.Instance.Language_GetTextById("attacking") :
                CoreManager.Instance.Language_GetTextById("defending");
            OnAttackModeComplete?.Invoke(toggle);
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
            
            if (buildingToBuild == null)
                return;
            
            //Are we over the UI instead?
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            //Check hit position to build.
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo, 100, buildGroundLayer))
            {
                buildingToBuild.bluePrint.position = hitInfo.point;
                //Cancel Build
                if (Input.GetButtonDown("Fire2"))
                {
                    CancelBuilding();
                    return;
                }
                //Check if there is something colliding with the blue print, if yes, not possible to build.
                var hits = Physics.OverlapBox(buildingToBuild.bluePrintCollision.position,
                    buildingToBuild.bluePrintCollision.localScale / 2, Quaternion.identity, bluePrintBadLayers);
                if (hits.Length > 0 || buildingToBuild.bluePrint.position.y > maxHeightToBuild)
                {
                    BuildingCheckTouch(true);
                    return;
                }
                
                //Check if we are close enough from player flag
                var dist = Vector3.Distance(buildingToBuild.bluePrintCollision.position, flagBlue.position);
                if (dist > maxRadiusToBuild)
                {
                    BuildingCheckTouch(true);
                    if (Input.GetButtonDown("Fire1"))
                    {
                        ShowWarningText(CoreManager.Instance.Language_GetTextById("msg_cantBuild"));
                    }
                    return;
                }
                
                //OK TO BUILD
                BuildingCheckTouch(false);
                if (Input.GetButtonDown("Fire1"))
                {
                    CreateBuilding();
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
                        BuildingSelFromScene(hitInfo.transform.GetComponent<BuildingBehaviour>());
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
        private void CreateBuilding()
        {
            if (buildingToBuild == null)
                return;
            if (levelGold < buildingToBuild.propData.goldCost)
            {
                ShowWarningText(CoreManager.Instance.Language_GetTextById("msg_noGold"));
                return;
            }
            var pos = buildingToBuild.bluePrint.position;
            var building = Instantiate(buildingToBuild.prefabToCreate, pos,
                buildingToBuild.bluePrint.rotation, parentForBuildings);
            building.tag = GameTags.TeamBlue;
            VfxManager.Instance.CallVFx(0, pos, Quaternion.identity);
            CameraBehaviour.Instance.ShakeCamera_Building();
            GoldAdd(-buildingToBuild.propData.goldCost);
            //Clear selected building
            CancelBuilding();
        }
        // =============================================================================================================
        /// <summary>
        /// Cancel the building selection.
        /// </summary>
        private void CancelBuilding()
        {
            buildingToBuild.ToggleBluePrint(false);
            buildingToBuild = null;
            playerMode = PlayerMode.InScene;
        }
        // =============================================================================================================
        /// <summary>
        /// Cancel the selection of a building in game.
        /// </summary>
        private void CancelSelection()
        {
            if (buildingSelected != null)
                buildingSelected.BuildingSelectedToggle(false);
            buildingSelected = null;
            uiBuildingDetails.SetActive(false);
            uiMoveModeDetails.SetActive(true);
        }
        // =============================================================================================================
        /// <summary>
        /// Checks if a building is touching something that will lock or unlock the creation.
        /// </summary>
        /// <param name="isLocked"></param>
        private void BuildingCheckTouch(bool isLocked)
        {
            buildingToBuild?.SetRenderersMaterial(isLocked, isLocked ? bluePrintLocked : bluePrintUnlocked);
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
                {
                    buildingToBuild = buildingsAvailable[i];
                    buildingToBuild.ToggleBluePrint(true);
                }
            }
            playerMode = PlayerMode.BuildingMode;
        }
        // =============================================================================================================
        /// <summary>
        /// Select a building from the scene (already built).
        /// </summary>
        private void BuildingSelFromScene(BuildingBehaviour building)
        {
            if (buildingSelected != null)
                buildingSelected.BuildingSelectedToggle(false);
            
            buildingSelected = building;
            buildingSelected.BuildingSelectedToggle(true);
            unitToBuild = buildingSelected.GetUnitDataType;
            uiBuildingDetails.SetActive(true);
            uiMoveModeDetails.SetActive(false);
            uiBuildSelIcon.sprite = buildingSelected.GetPropType.picture;
            uiBuildSelName.text = buildingSelected.GetPropType.propName;
            if (buildingSelected.GetPropType.objectType == ObjectType.BuildingFarm)
            {
                uiBuildSelUnitName.text = CoreManager.Instance.Language_GetTextById("goldIncome");
                uiBuildSelField1.text = "<color=yellow>+" + buildingSelected.GetPropType.goldGenerate + " / " + 
                                        buildingSelected.GetPropType.timerForGoldIncome + "s";
                uiBuildSelField2.text = "<color=yellow>Bonus: +" + buildingSelected.GetGoldBonusPerTree;
                uiBuildSelUnitImg.sprite = uiGoldIcon;
                uiBuildSelTrainBtn.interactable = false;
            }
            else if (buildingSelected.GetPropType.objectType == ObjectType.BuildingBarracks ||
                     buildingSelected.GetPropType.objectType == ObjectType.BuildingDefenseTw ||
                     buildingSelected.GetPropType.objectType == ObjectType.BuildingMagicTw)
            {
                uiBuildSelUnitName.text = CoreManager.Instance.Language_GetCharacterName(unitToBuild.unitName) + 
                                           " (" + unitToBuild.goldCost + " G)";
                uiBuildSelField1.text = "<color=red>ATK: " + unitToBuild.damage;
                uiBuildSelField2.text = "<color=blue>DEF: " + unitToBuild.defense;
                uiBuildSelUnitImg.sprite = unitToBuild.picture;
                uiBuildSelTrainBtn.interactable = true;
            }
            UpdateBuildingUi();
        }
        // =============================================================================================================
        /// <summary>
        /// Train a unit from the selected building (called from UI).
        /// </summary>
        public void BuildingTrainUnit()
        {
            if (buildingSelected != null && unitToBuild != null)
            {
                BuildingTryToTrainUnit(buildingSelected, unitToBuild, true);
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Try to train all possible units in all buildings available.
        /// </summary>
        public void TrainAllPossibleUnits()
        {
            ShowWarningText(CoreManager.Instance.Language_GetTextById("msg_trainAllPossible"));
            var allBuildings = parentForBuildings.GetComponentsInChildren<BuildingBehaviour>();
            for (var i = 0; i < allBuildings.Length; i++)
            {
                BuildingTryToTrainUnit(allBuildings[i], allBuildings[i].GetUnitDataType, false);
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Check if the selected building can really train a unit
        /// </summary>
        /// <param name="building"></param>
        /// <param name="unit"></param>
        /// <param name="showErrorMsgs"></param>
        /// <returns></returns>
        private void BuildingTryToTrainUnit(BuildingBehaviour building, CharacterData unit, bool showErrorMsgs)
        {
            if (building.GetActionTimer > 0)
            {
                if (showErrorMsgs)
                    ShowWarningText(CoreManager.Instance.Language_GetTextById("msg_alreadyTraining"));
                return;
            }
            if (levelGold < unit.goldCost)
            {
                if (showErrorMsgs)
                    ShowWarningText(CoreManager.Instance.Language_GetTextById("msg_noGold"));
                return;
            }
            if (building.GetActionTimer > 0)
                return;
            building.TrainUnit();
            GoldAdd(-unit.goldCost);
            UpdateBuildingUi();
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
            uiScoreText.text = levelScore.ToString();
        }
        // =============================================================================================================
        /// <summary>
        /// Update building UI if selected.
        /// </summary>
        private void UpdateBuildingUi()
        {
            if (gameState != GameState.Playing)
                return;
            if (buildingSelected == null)
                return;
            uiBuildSelHealth.text = CoreManager.Instance.Language_GetTextById("health") +
                ": " + buildingSelected.GetHealthPoints;
            if (buildingSelected.GetDamage > 0)
            {
                uiBuildSelDmg.text = CoreManager.Instance.Language_GetTextById("damage") + 
                                     ": +" + buildingSelected.GetDamage;
                uiBuildSelAtkRate.text = CoreManager.Instance.Language_GetTextById("atkRate") + 
                                         ": 1 / " + buildingSelected.GetAtkRate + "s";
            }
            else
            {
                uiBuildSelDmg.text = "";
                uiBuildSelAtkRate.text = "";
            }
            uiBuildSelTrainStatus.text = buildingSelected.GetBuildActionTimerStatus();
        }
        // =============================================================================================================
        /// <summary>
        /// Set wave timer from EnemyManager.
        /// </summary>
        /// <param name="amount"></param>
        public void SetUiWaveTimer(float amount)
        {
            uiWaveTimer.text = amount.ToString("f0") + "s";
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
        BuildingMagicTw = 3
    }
    /// <summary>
    /// Tags in game, to be easier to modify in case we need.
    /// </summary>
    public static class GameTags
    {
        public const string Untagged = "Untagged";
        public const string TeamRed = "TeamRed";
        public const string TeamBlue = "TeamBlue";
        public const string FlagRed = "FlagRed";
        public const string FlagBlue = "FlagBlue";
        public const string Nature = "Nature";
        public const string PortalSpawn = "PortalSpawn";
    }
}
