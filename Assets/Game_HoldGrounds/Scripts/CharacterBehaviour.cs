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
    /// - While moving to there, units will look for enemies in a radius collider.
    /// - If an enemy is found (Team red or blue), it will move to there.
    /// - If the enemy is still there, it will start attacking until it is destroyed.
    /// </summary>
    public class CharacterBehaviour : LiveObject
    {
        [Header("====== UNIT SETUP")]
        [SerializeField] private CharacterData unitData;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Animator myAnimator;
        [Tooltip("What are the game objects this unit will look for.")]
        [SerializeField] private LayerMask layerToSearchFor;

        [Header("====== COMBAT")]
        [Tooltip("Set if melee or not. Melee changes how it stops during the NavMesh move position.")]
        [SerializeField] private bool isMelee;
        [SerializeField] [ReadOnly] private bool isInCombat;
        [Tooltip("Size of the radius to search for enemy nearby")]
        [SerializeField] private float checkForEnemyRadius = 10;
        [Tooltip("This will be used to check if a unit - usually ranged - is stuck in the same place for combat.")]
        [SerializeField] private float idleMinimumSpeed = 0.3f;
        [Tooltip("Position of a melee weapon so we can cast some nice VFX.")]
        [SerializeField] private Transform weaponPosition;
        [SerializeField] [ReadOnly] private float targetDistance;
        [SerializeField] [ReadOnly] private float myCurrentVelocity;

        [Header("====== RANGED ONLY")]
        [Tooltip("If this is a ranged unit, what VFX it will spawn to hit the enemy. Check VfxManager.")]
        [SerializeField] private int rangedVfxSpawnId;
        [Tooltip("This will be used to always shoot, even if the distance changed because this unit was pushed.")]
        [SerializeField] [ReadOnly] private bool startedToShoot;
        
        [Header("====== TARGET")]
        [Tooltip("How long it takes to look for a potential new nearest target.")]
        [SerializeField] private float searchTimer = 1;
        [Tooltip("Nearest target to attack, Units will prioritize this over the main final target.")]
        [SerializeField] [ReadOnly] private LiveObject myNearestTarget;
        [Tooltip("The final target, if player destroy this, the game will be over.")]
        [SerializeField] [ReadOnly] private Transform myMainTarget;
        [SerializeField] [ReadOnly] private Vector3 navMeshDestination;
        
        private Transform myTransform;
        private float currentSearchTimer;
        private float atkWaitTimer;
        private float standingTimer;
        
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
            //Change material and color depending on the team (player or enemy)
            //Also set if it is Ally (player) unit or not
            var skinMat = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (CompareTag(GameTags.TeamBlue))
            {
                SetAlly();
                for (var i = 0; i < skinMat.Length; i++)
                    skinMat[i].material = unitData.teamMaterial[0];
                myMainTarget = GameObject.FindWithTag(GameTags.FlagRed).transform;
            }
            else
            {
                for (var i = 0; i < skinMat.Length; i++)
                    skinMat[i].material = unitData.teamMaterial[1];
                myMainTarget = GameObject.FindWithTag(GameTags.FlagBlue).transform;
            }
            SetHealth(unitData.maxHealthPoints);
            SetDefense(unitData.defense);
            myTransform = GetComponent<Transform>();
            navMeshAgent.speed = unitData.moveSpeed;
            SetDestination(myMainTarget.position);
        }
        // =============================================================================================================
        /// <summary>
        /// Handles movement direction, target and animation.
        /// </summary>
        private void HandleMovement()
        {
            //Check velocity and animate movement
            myCurrentVelocity = navMeshAgent.velocity.magnitude;
            myAnimator.SetFloat(AnimMoveSpeed, myCurrentVelocity);
            //Set timer to search
            if (myNearestTarget == null)
            {
                //If we are in combat, set it false and set the final destination again.
                if (isInCombat)
                {
                    isInCombat = false;
                    startedToShoot = false;
                    SetDestination(myMainTarget.position);
                }
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
        /// If it is in the same place for too long, make it move again.
        /// We don't call SetDestination from NavMesh in all frames, to not waste performance.
        /// </summary>
        private void HandleCombat()
        {
            if (myNearestTarget != null)
            {
                targetDistance = Vector3.Distance(myTransform.position, myNearestTarget.GetPosition);
                if (targetDistance <= unitData.atkDistance || startedToShoot)
                {
                    //Rotate to target
                    var direction = myNearestTarget.GetPosition - myTransform.position;
                    direction.y = 0;
                    var rot = Quaternion.LookRotation(direction);
                    myTransform.rotation = Quaternion.Slerp(myTransform.rotation, rot, Time.deltaTime * 10);
                    //Make unit to force stop
                    navMeshAgent.stoppingDistance = 999;
                    //Start attacking
                    atkWaitTimer -= Time.deltaTime;
                    if (atkWaitTimer <= 0)
                    {
                        atkWaitTimer = unitData.atkSpeed;
                        myAnimator.SetTrigger(AnimAttack);
                        if (!isMelee)
                            startedToShoot = true;
                    }
                }
                else
                {
                    if (myCurrentVelocity < idleMinimumSpeed)
                    {
                        standingTimer += Time.deltaTime;
                        if (standingTimer > 1)
                        {
                            standingTimer = 0;
                            SetDestination(myNearestTarget.GetPosition);
                        }
                    }
                }
            }
        }
        // =============================================================================================================
        public void SetDestination(Vector3 pos)
        {
            navMeshDestination = pos;
            navMeshAgent.SetDestination(navMeshDestination);
            navMeshAgent.stoppingDistance = 1;
        }
        // =============================================================================================================
        /// <summary>
        /// It will look for anything that is near, while walking towards the enemy flag.
        /// </summary>
        private void LookForNearestEnemy()
        {
            var possibleHits = new Collider[20];
            var nearestDist = 999f;
            var size = Physics.OverlapSphereNonAlloc(myTransform.position, checkForEnemyRadius,
                possibleHits, layerToSearchFor);
            for (var i = 0; i < size; i++)
            {
                if (IsAlly)
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
                    myNearestTarget = possibleHits[i].GetComponent<LiveObject>();
                    SetDestination(myNearestTarget.GetPosition);
                    isInCombat = true;
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
                VfxManager.Instance.CallVFx(1, weaponPosition.position, Quaternion.identity);
                myNearestTarget.TakeDamage(unitData.damage);
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
                VfxManager.Instance.CastProjectile(rangedVfxSpawnId, myTransform.position, myTransform.rotation,
                    IsAlly, unitData.damage);
            }
        }
        // =============================================================================================================
        /// <summary>
        /// To be called when this object is destroyed.
        /// </summary>
        protected override void OnObjectDestroyed()
        {
            VfxManager.Instance.CallVFx(3, transform.position, Quaternion.identity);
        }
        // =============================================================================================================
    }
}