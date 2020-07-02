using General.Utilities;
using Game_HoldGrounds.Scripts.Localization;
using Game_HoldGrounds.Scripts.Audio;
using System;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Add this script as priority in loading in Unity.
    /// Manages the save, load, score system and version.
    /// Also handles basic localization.
    /// </summary>
    public class CoreManager : MonoBehaviour
    {
        public static CoreManager Instance { get; private set; }
        
        [Tooltip("The current selected level to play. It will be set by the MainMenu script." +
                 "Or you can manually set to test.")]
        [SerializeField] private LevelData levelDataLoaded;
        public LevelData GetLevelData => levelDataLoaded;

        public LevelData SetLevelData
        {
	        set => levelDataLoaded = value;
        }

        [Tooltip("Prefab for showing a picture for scene loading.")]
        [SerializeField] private GameObject sceneLoadPrefab;
        
        // =============================================================================================================
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            //Load localization
            languageInfo.Language_LoadLocalization();
            
            //Load volumes (Audio)
            Audio_LoadSavedData();
        }
        // =============================================================================================================
        /// <summary>
        /// Load a new scene.
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadScene(string sceneName)
        {
	        Instantiate(sceneLoadPrefab);
	        LoadingSceneHandler.Instance.SetSceneName(sceneName);
        }
        // =============================================================================================================
        
        #region LANGUAGE DATA
		
        /// <summary>
        /// Holds all language info data.
        /// </summary>
        [SerializeField] private LanguageData languageInfo;
        /// <summary>
        /// Is all languages already loaded?
        /// </summary>
        public bool LanguageLoaded => languageInfo.LanguageLoaded;
		
        /// <summary>
        /// On language changed call.
        /// </summary>
        public static Action OnLanguageChanged;
		
        // =============================================================================================================
        /// <summary>
        /// Call that the main language has been changed.
        /// </summary>
        /// <param name="langSelected">Language ID selected.</param>
        public void Language_SetChange(int langSelected)
        {
            languageInfo.languageSelected = langSelected;
            PlayerPrefs.SetInt(LanguageData.PlayerPrefLanguage, langSelected);
            OnLanguageChanged?.Invoke();
            Debug.Log("Language has been changed to: " + langSelected);
        }
        // =============================================================================================================
        /// <summary>
        /// Get a GENERAL text translated by ID.
        /// </summary>
        public string Language_GetTextById(string txtId)
        {
            var txt = languageInfo.Language_GetLocalizedText(txtId);
            return string.IsNullOrEmpty(txt) ? "NO TEXT ID" : txt;
        }
        // =============================================================================================================
        /// <summary>
        /// Get a GENERAL text translated by ID.
        /// </summary>
        public string Language_GetBuildingName(string txtId)
        {
	        var txt = languageInfo.Language_GetBuildingName(txtId);
	        return string.IsNullOrEmpty(txt) ? "NO BUILDING ID" : txt;
        }
        // =============================================================================================================
        /// <summary>
        /// Get a GENERAL text translated by ID.
        /// </summary>
        public string Language_GetCharacterName(string txtId)
        {
	        var txt = languageInfo.Language_GetCharacterName(txtId);
	        return string.IsNullOrEmpty(txt) ? "NO CHARACTER ID" : txt;
        }
        // =============================================================================================================
		
		

        #endregion
        
        #region -------------------------- AUDIO DATA

		/// <summary>
		/// Stores all audio data for the game.
		/// </summary>
		[Tooltip("Stores all audio data for the game.")]
		[SerializeField] private AudioData audioInfo;
		
		// =============================================================================================================
		/// <summary>
		/// Loads audio volume data.
		/// </summary>
		private void Audio_LoadSavedData()
		{
			if (PlayerPrefs.HasKey(AudioData.PlayerPrefAudioOverall))
			{
				audioInfo.audioOverallVolume = PlayerPrefs.GetFloat(AudioData.PlayerPrefAudioOverall);
				audioInfo.audioMusicVolume = PlayerPrefs.GetFloat(AudioData.PlayerPrefAudioMusic);
				audioInfo.audioSfxVolume = PlayerPrefs.GetFloat(AudioData.PlayerPrefAudioSfx);
				Audio_ApplyToMixer(audioInfo.audioOverallVolume, audioInfo.audioMusicVolume, audioInfo.audioSfxVolume);
			}
			else
			{
				audioInfo.audioOverallVolume = 0;
				audioInfo.audioMusicVolume = 0;
				audioInfo.audioSfxVolume = 0;
				PlayerPrefs.SetFloat(AudioData.PlayerPrefAudioOverall, audioInfo.audioOverallVolume);
				PlayerPrefs.SetFloat(AudioData.PlayerPrefAudioMusic, audioInfo.audioMusicVolume);
				PlayerPrefs.SetFloat(AudioData.PlayerPrefAudioSfx, audioInfo.audioSfxVolume);
			}
			Debug.Log("Audio data loaded!");
		}
		// =============================================================================================================
		/// <summary>
		/// Gets audio volume data.
		/// </summary>
		public float AudioGetVolumeOverall => audioInfo.audioOverallVolume;
		/// <summary>
		/// Gets audio volume data.
		/// </summary>
		public float AudioGetVolumeMusic => audioInfo.audioMusicVolume;
		/// <summary>
		/// Gets audio volume data.
		/// </summary>
		public float AudioGetVolumeSfx => audioInfo.audioSfxVolume;
		// =============================================================================================================
		/// <summary>
		/// Call that the main audio has been changed.
		/// </summary>
		/// <param name="volOverall">Volume of overall game.</param>
		/// <param name="volMusic">Volume of music.</param>
		/// <param name="volSfx">Volume of sound effects and ambient.</param>
		public void Audio_SetChange(float volOverall, float volMusic, float volSfx)
		{
			//get the name in audio mixer exposed var
			Audio_ApplyToMixer(volOverall, volMusic, volSfx);
			PlayerPrefs.SetFloat(AudioData.PlayerPrefAudioOverall, volOverall);
			PlayerPrefs.SetFloat(AudioData.PlayerPrefAudioMusic, volMusic);
			PlayerPrefs.SetFloat(AudioData.PlayerPrefAudioSfx, volSfx);
			audioInfo.audioOverallVolume = PlayerPrefs.GetFloat(AudioData.PlayerPrefAudioOverall);
			audioInfo.audioMusicVolume = PlayerPrefs.GetFloat(AudioData.PlayerPrefAudioMusic);
			audioInfo.audioSfxVolume = PlayerPrefs.GetFloat(AudioData.PlayerPrefAudioSfx);
			Debug.Log("Audio volume changed!");
		}
		// =============================================================================================================
		/// <summary>
		/// Converts the audio parameter in main menu (0 to 1), into the mixer volume.
		/// Remember to set the name of the exposed parameters in AudioMixer before applying here.
		/// Check the AudioData script for the name.
		/// </summary>
		private void Audio_ApplyToMixer(float volOverall, float volMusic, float volSfx)
		{
			audioInfo.audioMixer.SetFloat(AudioData.PlayerPrefAudioOverall, volOverall);
			audioInfo.audioMixer.SetFloat(AudioData.PlayerPrefAudioMusic, volMusic);
			audioInfo.audioMixer.SetFloat(AudioData.PlayerPrefAudioSfx, volSfx);
		}
		// =============================================================================================================

		#endregion
    }
}