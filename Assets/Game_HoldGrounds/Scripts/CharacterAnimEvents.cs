using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// This script will receive animation events from the Animations in Animator.
    /// </summary>
    public class CharacterAnimEvents : MonoBehaviour
    {
        [SerializeField] private CharacterBehaviour characterScript;
        
        // =============================================================================================================
        public void AttackHit()
        {
            characterScript.AttackAnimationHit();
        }
        // =============================================================================================================
        public void AttackCast()
        {
            characterScript.AttackAnimationCast();
        }
        // =============================================================================================================
    }
}