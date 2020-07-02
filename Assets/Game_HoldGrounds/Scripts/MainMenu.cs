using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles the first main menu to load levels, options and exit game.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("====== SETUP")]
        [Tooltip("Text file for the game briefing.")]
        [SerializeField] private TextAsset gameBriefing;
        [Tooltip("Text file for the game briefing.")]
        [SerializeField] private TextMeshProUGUI uiTextBriefing;
        [SerializeField] private string gamePlayMapName = "MainMap";

        [Header("====== UI")]
        [SerializeField] private GameObject uiFirstMenuWindow;
        [SerializeField] private GameObject uiOptionsWindow;
        [SerializeField] private GameObject uiLevelWindow;
        [SerializeField] private GameObject uiOptionsLanguage;
        [SerializeField] private GameObject uiOptionsGraphics;
        [SerializeField] private GameObject uiOptionsAudio;
        [SerializeField] private Slider audioOverall;
        [SerializeField] private Slider audioMusic;
        [SerializeField] private Slider audioSoundFx;

        /// <summary>
        /// Just to make sure some values are not changed or called before we want to.
        /// </summary>
        private bool menuFinishedLoading;
        // =============================================================================================================
        private void Start()
        {
            uiTextBriefing.text = gameBriefing.text;
            CloseAllMenuWindows();
            OpenMenuWindow(0);
            audioOverall.value = CoreManager.Instance.AudioGetVolumeOverall;
            audioMusic.value = CoreManager.Instance.AudioGetVolumeMusic;
            audioSoundFx.value = CoreManager.Instance.AudioGetVolumeSfx;
            menuFinishedLoading = true;
        }
        // =============================================================================================================
        /// <summary>
        /// Start a level
        /// </summary>
        /// <param name="levelData"></param>
        public void StartLevel(LevelData levelData)
        {
            CloseAllMenuWindows();
            CoreManager.Instance.SetLevelData = levelData;
            CoreManager.Instance.LoadScene(gamePlayMapName);
        }
        // =============================================================================================================
        /// <summary>
        /// Close all main menu windows.
        /// </summary>
        private void CloseAllMenuWindows()
        {
            uiFirstMenuWindow.SetActive(false);
            uiOptionsWindow.SetActive(false);
            uiLevelWindow.SetActive(false);
        }
        // =============================================================================================================
        /// <summary>
        /// Open a menu window
        /// </summary>
        /// <param name="id"></param>
        public void OpenMenuWindow(int id)
        {
            uiFirstMenuWindow.SetActive(id == 0);
            uiOptionsWindow.SetActive(id == 1);
            uiLevelWindow.SetActive(id == 2);
            OpenOptionsSubmenu(0);
        }
        // =============================================================================================================
        /// <summary>
        /// Open an options sub menu window
        /// </summary>
        /// <param name="id"></param>
        public void OpenOptionsSubmenu(int id)
        {
            uiOptionsLanguage.SetActive(id == 0);
            uiOptionsGraphics.SetActive(id == 1);
            uiOptionsAudio.SetActive(id == 2);
        }
        // =============================================================================================================
        /// <summary>
        /// Select a new language.
        /// Check id order from language info array order.
        /// </summary>
        /// <param name="id"></param>
        public void LanguageSet(int id)
        {
            CoreManager.Instance.Language_SetChange(id);
        }
        // =============================================================================================================
        /// <summary>
        /// Select graphics.
        /// </summary>
        /// <param name="id"></param>
        public void GraphicsSet(int id)
        {
            QualitySettings.SetQualityLevel(id);
        }
        // =============================================================================================================
        /// <summary>
        /// Change music
        /// </summary>
        /// <param name="amount"></param>
        public void AudioSetMusic(float amount)
        {
            
        }
        // =============================================================================================================
        /// <summary>
        /// Called when the slider of one of the audios changed.
        /// </summary>
        public void AudioChanged()
        {
            if (!menuFinishedLoading)
                return;
            CoreManager.Instance.Audio_SetChange(audioOverall.value, audioMusic.value, audioSoundFx.value);
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
    }
}