using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "HoldGrounds/New LEVEL", order = 1)]
    [System.Serializable]
    public class LevelData : ScriptableObject
    {
        [Header("====== SETUP")]
        public string levelName;
        public string sceneName;
        public Sprite picture;
        public int startingGold;
        public int startingMorale;
        
        // =============================================================================================================
        /// <summary>
        /// Picture to be used in UI.
        /// </summary>
        public Sprite SetPicture {set => picture = value;}
        // =============================================================================================================
        /// <summary>
        /// Picture to be used in UI.
        /// </summary>
        public Sprite GetPicture => picture;
        // =============================================================================================================
    }
}