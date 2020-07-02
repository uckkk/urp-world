using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game_HoldGrounds.Scripts
{
    public class MenuLevelUi : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [Tooltip("Starting data of this level.")]
        [SerializeField] private LevelData levelData;
        [SerializeField] private TextMeshProUGUI levelName;
        [SerializeField] private TextMeshProUGUI startingGold;
        [SerializeField] private TextMeshProUGUI startingMorale;
        [SerializeField] private Image levelPicture;
        
        // =============================================================================================================
        private void Start()
        {
            //Setup data
            levelName.text = levelData.levelName;
            startingGold.text = levelData.startingGold.ToString();
            startingMorale.text = levelData.startingMorale.ToString();
            levelPicture.sprite = levelData.picture;
        }
        // =============================================================================================================
        /// <summary>
        /// Start the game level
        /// </summary>
        public void StartLevel()
        {
            mainMenu.StartLevel(levelData);
        }
        // =============================================================================================================
    }
}