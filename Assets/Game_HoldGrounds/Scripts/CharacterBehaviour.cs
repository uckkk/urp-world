using General.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Handles all units in game (player or enemy).
    /// How it works:
    /// - Unit characters are spawned by Buildings (PropBehaviour), but the actual call is from CharacterManager.
    /// - Units will search for the final target first (that will be their goal).
    /// - While moving forward to there, units will look for enemies in a radius collider.
    /// - If an enemy is found (Team red or blue), it will move to there.
    /// - If the enemy is still there, it will start attacking until it is destroyed.
    /// </summary>
    public class CharacterBehaviour : MonoBehaviour
    {
        [Header("====== UNIT SETUP")]
        [SerializeField] [ReadOnly] private bool isAlly;
        [SerializeField] private CharacterData unitData;
        [SerializeField] private float currentHealth;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Animator myAnimator;
        [Tooltip("What are the game objects this unit will look for.")]
        [SerializeField] private LayerMask layerToSearchFor;

        [Header("====== COMBAT")]
        [Tooltip("Size of the radius to search for enemy nearby")]
        [SerializeField] private float checkForEnemyRadius = 10;
        
        [Header("====== TARGET")]
        [Tooltip("How long it takes to look for a potential new nearest target.")]
        [SerializeField] private float searchTimer = 2;
        [Tooltip("Nearest target to attack, Units will prioritize this over the main final target.")]
        [SerializeField] [ReadOnly] private Transform myNearestTarget;
        [Tooltip("The final target, if player destroy this, the game will be over.")]
        [SerializeField] [ReadOnly] private Transform myMainTarget;
        [SerializeField] [ReadOnly] private Vector3 navMeshDestination;
        
        private Transform myTransform;
        private float currentSearchTimer;
        private float atkWaitTimer;
        private PropBehaviour temporaryEnemyProp;
        private CharacterBehaviour temporaryEnemyChar;
        
        private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int AnimAttack = Animator.StringToHash("Attack");

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
            HandleCombat();
        }
        // =============================================================================================================
        private void PrepareUnit()
        {
            isAlly = CompareTag(GameTags.TeamBlue);
            myTransform = GetComponent<Transform>();
            myMainTarget = GameObject.FindWithTag(GameTags.FinalTarget).transform;
            currentHealth = unitData.maxHealthPoints;
            navMeshAgent.speed = unitData.moveSpeed;
            SetDestination(myMainTarget.position);
        }
        // =============================================================================================================
        /// <summary>
        /// Handles movement direction, target and animation.
        /// </summary>
        private void HandleMovement()
        {
            myAnimator.SetFloat(AnimMoveSpeed, navMeshAgent.velocity.magnitude);
            if (myNearestTarget == null)
            {
                currentSearchTimer -= Time.deltaTime;
                if (currentSearchTimer <= 0)
                {
                    currentSearchTimer = searchTimer;
                    LookForNearestEnemy();
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Attack enemy in case still exists and it is close.
        /// </summary>
        private void HandleCombat()
        {
            if (myNearestTarget != null)
            {
                var minDistance = Vector3.Distance(myTransform.position, myNearestTarget.position);
                if (minDistance <= unitData.atkDistance)
                {
                    //Start attacking
                    atkWaitTimer -= Time.deltaTime;
                    if (atkWaitTimer <= 0)
                    {
                        atkWaitTimer = unitData.atkSpeed;
                        myAnimator.SetTrigger(AnimAttack);
                    }
                }
            }
        }
        // =============================================================================================================
        public void SetDestination(Vector3 pos)
        {
            navMeshDestination = pos;
            navMeshAgent.SetDestination(navMeshDestination);
        }
        // =============================================================================================================
        /// <summary>
        /// It will look for anything that is near, while walking towards the enemy flag.
        /// </summary>
        private void LookForNearestEnemy()
        {
            // var allEnemies = GameObject.FindGameObjectsWithTag(GameTags.Enemy);
            // if (allEnemies.Length == 0)
            //     return;
            // SetDestination(allEnemies[0].transform.position);

            var possibleHits = new Collider[20];
            var nearestDist = 999f;
            var size = Physics.OverlapSphereNonAlloc(myTransform.position, checkForEnemyRadius,
                possibleHits, layerToSearchFor);
            for (var i = 0; i < size; i++)
            {
                if (isAlly)
                {
                    if (possibleHits[i].CompareTag(GameTags.TeamBlue))
                        continue;
                }
                else
                {
                    if (possibleHits[i].CompareTag(GameTags.TeamRed))
                        continue;
                }
                var dist = Vector3.Distance(myTransform.position, possibleHits[i].transform.position);
                if (nearestDist > dist)
                {
                    nearestDist = dist;
                    myNearestTarget = possibleHits[i].transform;
                    SetDestination(myNearestTarget.position);
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Called when the attack animation is suppose to hit something.
        /// </summary>
        public void AttackAnimationHit()
        {
            if (myNearestTarget != null)
            {
                
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Called when the attack animation is suppose to cast something.
        /// </summary>
        public void AttackAnimationCast()
        {
            if (myNearestTarget != null)
            {
                
            }
        }
        // =============================================================================================================
    }
}