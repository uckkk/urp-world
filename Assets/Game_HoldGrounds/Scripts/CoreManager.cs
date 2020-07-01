using Game_HoldGrounds.Scripts.Localization;
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
		
		

        #endregion
    }
}