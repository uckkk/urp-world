using System.Collections;
using TMPro;
using UnityEngine;

namespace Game_HoldGrounds.Scripts.Localization
{
    /// <inheritdoc />
    /// <summary>
    /// Modifies the language of the UI text where the script is in.
    /// The core script for language will search for this script and make the correct changes.
    /// </summary>
    public class LanguageModifier : MonoBehaviour
    {
        /// <summary>
        /// ID code for the text. Example: mainMenu.
        /// </summary>
        [Tooltip("ID code for the text. Example: mainMenu.")]
        [SerializeField] private string idCodeForText;
        
        /// <summary>
        /// The UI field to get.
        /// </summary>
        private TextMeshProUGUI uiTextField;
        
        // =============================================================================================================
        private void OnEnable()
        {
            CoreManager.OnLanguageChanged += Language_SetChanged;
            if (uiTextField == null) uiTextField = GetComponent<TextMeshProUGUI>();
            StartCoroutine(SetText());
        }
        // =============================================================================================================
        private void OnDisable()
        {
            CoreManager.OnLanguageChanged -= Language_SetChanged;
        }
        // =============================================================================================================
        private IEnumerator SetText()
        {
            while (!CoreManager.Instance.LanguageLoaded)
            {
                //wait till language load
                yield return new WaitForFixedUpdate();
            }
            uiTextField.text = CoreManager.Instance.Language_GetTextById(idCodeForText);
        }
        // =============================================================================================================
        /// <summary>
        /// Called when the language has been changed globally.
        /// </summary>
        private void Language_SetChanged()
        {
            StartCoroutine(SetText());
        }
        // =============================================================================================================
    }
}