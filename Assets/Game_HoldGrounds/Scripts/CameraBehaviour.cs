using General.Utilities;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles our camera in game.
    /// </summary>
    public class CameraBehaviour : MonoBehaviour
    {
        public static CameraBehaviour Instance { get; private set; }

        [Tooltip("We will use this to make our camera to shake.")]
        [SerializeField] private ShakeTransformObj shakeScript;
        
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
        /// <summary>
        /// Shake our camera when you build a new building.
        /// </summary>
        public void ShakeCamera_Building()
        {
            shakeScript.SetShakeTime(0.3f, false);
        }
        // =============================================================================================================
    }
}