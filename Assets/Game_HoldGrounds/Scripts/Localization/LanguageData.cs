using General.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game_HoldGrounds.Scripts.Localization
{
	/// <summary>
	/// Use CSV:
	/// Use Google Sheets instead of EXCEL when doing Excel files and exporting to CSV (due to Encoding).
	/// WARNING: IN a sheet, before exporting to CSV, THE LAST COLUMN (OR ROW) FROM THE FILE, NEEDS TO HAVE A VALID
	/// CHARACTER, OR IT WILL NOT CONSIDER THE '\n' END OF LINE. Do not leave it blank.
	/// 
	/// Class to handle all language data for the game.
	/// Tried to use this as struct, but each time I need to read "localizedTextList", it returned an error.
	/// </summary>
	[Serializable]
	public class LanguageData
	{
		// =====================================================================
		/// <summary>
		/// File to read general language localization. Items are in another file.
		/// </summary>
		[Tooltip("File to read GENERAL language localization. Items are in another file.")]
		[SerializeField] private TextAsset fileToGeneralTexts;
		/// <summary>
		/// File to read characters language localization.
		/// </summary>
		[Tooltip("File to read characters language localization.")]
		[SerializeField] private TextAsset fileToCharacterTexts;
		/// <summary>
		/// File to read buildings language localization.
		/// </summary>
		[Tooltip("File to read buildings language localization.")]
		[SerializeField] private TextAsset fileToBuildingsTexts;

		// =====================================================================
		/// <summary>
		/// Current language selected running the game.
		/// </summary>
		[Tooltip("Current language selected running the game.")]
		public int languageSelected;
		/// <summary>
		/// Type here the names of each languages added (in the correct order as the file).
		/// </summary>
		[Tooltip("Type here the names of each languages added (in the correct order as the file).")]
		[SerializeField] private string[] languagesAdded;
		// =====================================================================
		/// <summary>
		/// Texts already separated by language (id and translation for each language).
		/// </summary>
		private List<TextByLanguage> localizedGeneralTextList;
		/// <summary>
		/// Items texts already separated by language (id and translation for each language).
		/// </summary>
		private List<TextByLanguage> localizedCharactersList;
		/// <summary>
		/// Items texts already separated by language (id and translation for each language).
		/// </summary>
		private List<TextByLanguage> localizedBuildingsList;
		// =====================================================================
		/// <summary>
		/// Is language loaded and ready to be called?
		/// </summary>
		public bool LanguageLoaded { get; private set; }
		/// <summary>
		/// Language name used to save as configuration.
		/// </summary>
		/// <returns></returns>
		public const string PlayerPrefLanguage = "languageSelected";
		// =========================================================================================================
		/// <summary>
		/// Load all the localization texts.
		/// Also, used Regex because CSV consider commas when exporting, so we need to convert commas inside phrases,
		/// then it will not misunderstood with a new Row.
		/// FOR NOW, IT CANNOT READ MULTIPLE LINES IN A SINGLE ROW from CSV. Will update in the future.
		/// </summary>
		public void Language_LoadLocalization()
		{
			LanguageLoaded = false;
			// =========================================================
			
			localizedGeneralTextList = new List<TextByLanguage>();
			localizedCharactersList = new List<TextByLanguage>();
			localizedBuildingsList = new List<TextByLanguage>();
			var localizationGeneral = fileToGeneralTexts.text.Split(new char[] {'\n'}); //string[] type
			var localizationItems = fileToCharacterTexts.text.Split(new char[] {'\n'});
			var localizationItemsDescr = fileToBuildingsTexts.text.Split(new char[] {'\n'});
			
			//FOR NOW, IT CANNOT READ MULTIPLE LINES IN A SINGLE ROW from CSV.
			
			//GENERAL
			SetupAndSetLocalizedTexts(localizationGeneral, localizedGeneralTextList);
			//CHARACTERS
			SetupAndSetLocalizedTexts(localizationItems, localizedCharactersList);
			//BUILDINGS
			SetupAndSetLocalizedTexts(localizationItemsDescr, localizedBuildingsList);
			
			// =========================================================
			//Select the last saved language
			if (PlayerPrefs.HasKey(PlayerPrefLanguage))
				languageSelected = PlayerPrefs.GetInt("languageSelected");
			else
			{
				languageSelected = 0;
				PlayerPrefs.SetInt(PlayerPrefLanguage, languageSelected);
			}
			LanguageLoaded = true;
		}
		// =========================================================================================================
		/// <summary>
		/// Populates all lists regarding a given data for localization.
		/// </summary>
		/// <param name="locList">List of string to search for wrong/empty words.</param>
		/// <param name="list">List of the localized ready text.</param>
		private void SetupAndSetLocalizedTexts(IReadOnlyList<string> locList, ICollection<TextByLanguage> list)
		{
			for (var i = 1; i < locList.Count - 1; i++) //First line is the description line from CSV
			{
				var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"); //Regex
				var dataFixed = csvParser.Split(locList[i]); //string[] type
				for (var w = 0; w < dataFixed.Count(); w++)
				{
					dataFixed[w] = dataFixed[w].TrimStart(' ', '"');
					dataFixed[w] = dataFixed[w].TrimEnd('"');
				}
				var newText = new TextByLanguage
				{
					textName = dataFixed[0],
					languageList = new string[languagesAdded.Length]
				};
				for (var p = 0; p < languagesAdded.Length; p++)
				{
					newText.languageList[p] = dataFixed[p + 1];
				}
				list.Add(newText);
			}
		}
		// =========================================================================================================
		/// <summary>
		/// Get localized text based on localization ID.
		/// </summary>
		public string Language_GetLocalizedText(string idName)
		{
			// Debug.Log("idName: " + idName);
			return localizedGeneralTextList.Find(x => x.textName == idName).languageList[languageSelected];
		}
		// =========================================================================================================
	}

	/// <summary>
	/// Store text localization.
	/// </summary>
	[Serializable]
	public struct TextByLanguage
	{
		/// <summary>
		/// Unique text ID name to identify in the localization list.
		/// </summary>
		public string textName;

		/// <summary>
		/// List of translations of the textName for each language.
		/// </summary>
		public string[] languageList;
	}
}