using UnityEngine;
using UnityEngine.AI;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles all units in game (player or enemy)
    /// </summary>
    public class UnitBehaviour : MonoBehaviour
    {
        [Header("====== UNIT SETUP")]
        [SerializeField] private UnitData unitData;
        [SerializeField] private float currentHealth;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Animator myAnimator;
        
        private Vector3 _navMeshDestination;
        private static readonly int AnimMoveSpeed = Animator.StringToHash("moveSpeed");

        // =============================================================================================================
        private void Start()
        {
            PrepareUnit();
            LookForNearestEnemy();
        }
        // =============================================================================================================
        private void Update()
        {
            HandleMovement();
        }
        // =============================================================================================================
        private void PrepareUnit()
        {
            currentHealth = unitData.maxHealthPoints;
        }
        // =============================================================================================================
        private void HandleMovement()
        {
            myAnimator.SetFloat(AnimMoveSpeed, navMeshAgent.velocity.magnitude);
        }
        // =============================================================================================================
        /// <summary>
        /// It will look for anything that is near, while walking towards the enemy flag.
        /// </summary>
        private void LookForNearestEnemy()
        {
            var allEnemies = GameObject.FindGameObjectsWithTag(GameTags.Enemy);
            if (allEnemies.Length == 0)
                return;
            SetDestination(allEnemies[0].transform.position);
        }
        // =============================================================================================================
        public void SetDestination(Vector3 pos)
        {
            _navMeshDestination = pos;
            navMeshAgent.SetDestination(_navMeshDestination);
        }
        // =============================================================================================================
    }
}