using System;
using General.Utilities;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles the projectile the player and enemies can cast.
    /// It can be used for Arrows or Magic.
    /// </summary>
    public class RangedProjectileHandler : MonoBehaviour
    {
        [Tooltip("Do I belong to the player?")]
        [SerializeField] [ReadOnly] private bool isAlly;
        [Tooltip("Damage should come from the Character unit that casted this.")]
        [SerializeField] [ReadOnly] private float damage;
        [Tooltip("Timer to make it disappear in case it does not hit anything")]
        [SerializeField] private float timeAlive = 3;
        [Tooltip("Check order in VfxManager.")]
        [SerializeField] private int impactHitVfx;
        
        // =============================================================================================================
        private void OnEnable()
        {
            CancelInvoke(nameof(DisableMySelf));
            Invoke(nameof(DisableMySelf), timeAlive);
        }
        // =============================================================================================================
        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag(GameTags.TeamBlue) && !isAlly || col.CompareTag(GameTags.TeamRed) && isAlly)
            {
                //Hit
                col.gameObject.GetComponent<LiveObject>().TakeDamage(damage);
                VfxManager.Instance.CallVFx(impactHitVfx, transform.position, Quaternion.identity);
                DisableMySelf();
            }
            else
            {
                if (!col.CompareTag(GameTags.TeamBlue) && !col.CompareTag(GameTags.TeamRed))
                {
                    VfxManager.Instance.CallVFx(impactHitVfx, transform.position, Quaternion.identity);
                    DisableMySelf();
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Set if this projectile is ally (player) or not (what team does it belong).
        /// </summary>
        public void SetIfAlly(bool toggle)
        {
            isAlly = toggle;
        }
        // =============================================================================================================
        /// <summary>
        /// Set the damage to apply.
        /// </summary>
        public void SetDamage(float dmg)
        {
            damage = dmg;
        }
        // =============================================================================================================
        private void DisableMySelf()
        {
            gameObject.SetActive(false);
        }
        // =============================================================================================================
    }
}