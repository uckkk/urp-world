using General.Utilities;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Base and helper class for all live objects in game, like buildings and units (characters).
    /// </summary>
    public class LiveObject : MonoBehaviour
    {
        [Header("====== OBJECT SETUP")]
        [SerializeField] [ReadOnly] private bool isActivated;
        [SerializeField] [ReadOnly] private bool isAlly;
        [SerializeField] [ReadOnly] private float currentHealth;
        [SerializeField] [ReadOnly] private float myDefensePower;
        [Tooltip("Just to show some nice visuals when it is badly damaged.")]
        [SerializeField] private GameObject badlyDamagedFx;
        
        protected bool IsActivated => isActivated;
        protected bool IsAlly => isAlly;
        protected float GetHealth => currentHealth;
        public float GetMyHealth => currentHealth;
        public Vector3 GetPosition => transform.position;
        private float maxHealth;
        
        // =============================================================================================================
        protected void OnEnable()
        {
            GameManager.OnGameStateChange += OnGameState;
        }
        // =============================================================================================================
        protected void OnDisable()
        {
            GameManager.OnGameStateChange -= OnGameState;
        }
        // =============================================================================================================
        /// <summary>
        /// Change this unit behaviour based on game state.
        /// </summary>
        private void OnGameState(GameState gState)
        {
             SetActivated(gState == GameState.Playing);
        }
        // =============================================================================================================
        /// <summary>
        /// Set if this object is activated for interaction.
        /// </summary>
        protected void SetActivated(bool toggle)
        {
            isActivated = toggle;
        }
        // =============================================================================================================
        /// <summary>
        /// Set if this object belongs to the player.
        /// </summary>
        protected void SetAlly()
        {
            isAlly = true;
        }
        // =============================================================================================================
        /// <summary>
        /// Set amount of defense to reduce from damage.
        /// </summary>
        /// <param name="def"></param>
        protected void SetDefense(float def)
        {
            myDefensePower = def;
        }
        // =============================================================================================================
        /// <summary>
        /// Set the starting health of this live object
        /// </summary>
        /// <param name="health"></param>
        protected void SetHealth(float health)
        {
            currentHealth = health;
            maxHealth = health;
            if (badlyDamagedFx != null)
                badlyDamagedFx.SetActive(false);
        }
        // =============================================================================================================
        /// <summary>
        /// Take damage from an enemy or something.
        /// </summary>
        /// <param name="dmgAmount"></param>
        public void TakeDamage(float dmgAmount)
        {
            if (currentHealth <= 0)
            {
                //Unit/building is dead already
                return;
            }
            currentHealth -= Mathf.Abs(dmgAmount - myDefensePower);
            if (badlyDamagedFx != null)
                badlyDamagedFx.SetActive(currentHealth < maxHealth / 2);
            if (currentHealth <= 0)
            {
                Debug.Log("Object destroyed: " + name);
                OnObjectDestroyed();
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Called when this object is destroyed.
        /// </summary>
        protected virtual void OnObjectDestroyed(){}
        // =============================================================================================================
    }
}