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
        [Tooltip("What are the game objects this unit will look for attack.")]
        [SerializeField] private LayerMask layerToSearchForAtk;
        [Tooltip("What are the game objects this unit will look for defend.")]
        [SerializeField] private LayerMask layerToSearchForDef;

        [Header("====== COMBAT")]
        [Tooltip("Set if melee or not. Melee changes how it stops during the NavMesh move position.")]
        [SerializeField] private bool isMelee;
        [Tooltip("If attacking, this unit will go for the final flag. If not, it will go back to base flag. " +
                 "Enemy is always attacking.")]
        [SerializeField] [ReadOnly] private bool isAttacking;
        [Tooltip("This means this unit found a target to go fight, instead of going to final Flag.")]
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
        [Tooltip("The final target, if the character reaches this, the player wins (or enemy wins).")]
        [SerializeField] [ReadOnly] private Transform myAttackTarget;
        [Tooltip("The target that this character needs to defend. Losing this is game over for the player.")]
        [SerializeField] [ReadOnly] private Transform myDefendTarget;
        [SerializeField] [ReadOnly] private Vector3 navMeshDestination;
        [Tooltip("How far this unit is to trigger victory or defeat?")]
        [SerializeField] [ReadOnly] private float distanceFromFinalDestiny;
        
        private Transform myTransform;
        private float currentSearchTimer;
        private float atkWaitTimer;
        private float standingTimer;
        
        private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int AnimAttack = Animator.StringToHash("Attack");
        private static readonly int AnimIsDead = Animator.StringToHash("IsDead");

        // =============================================================================================================
        protected void OnDestroy()
        {
            GameManager.OnAttackModeComplete -= OnAttackMode;
        }
        // =============================================================================================================
        private void Start()
        {
            GameManager.OnAttackModeComplete += OnAttackMode;
            PrepareUnit();
            LookForNearestEnemy();
        }
        // =============================================================================================================
        private void Update()
        {
            HandleMovement();
            HandleCombat();
            HandleDefending();
            HandleWinOrLoseCondition();
        }
        // =============================================================================================================
        /// <summary>
        /// Change if this unit will attack or defend. Only works for player units.
        /// </summary>
        /// <param name="toggle"></param>
        private void OnAttackMode(bool toggle)
        {
            //This should work only for player units.
            if (!IsAlly)
                return;

            isAttacking = toggle;
            
            //Reset if we were attacking or pursuing something.
            myNearestTarget = null;
            SetDestination(isAttacking ? myAttackTarget.position : myDefendTarget.position);
        }
        // =============================================================================================================
        private void PrepareUnit()
        {
            //Check if it is active
            if (GameManager.Instance.GetGameState == GameState.Playing)
                SetActivated(true);
            
            //Change material and color depending on the team (player or enemy)
            //Also set if it is Ally (player) unit or not
            var skinMat = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (CompareTag(GameTags.TeamBlue))
            {
                SetAlly();
                for (var i = 0; i < skinMat.Length; i++)
                    skinMat[i].material = unitData.teamMaterial[0];
                myAttackTarget = GameObject.FindWithTag(GameTags.FlagRed).transform;
                myDefendTarget = GameObject.FindWithTag(GameTags.FlagBlue).transform;
            }
            else
            {
                for (var i = 0; i < skinMat.Length; i++)
                    skinMat[i].material = unitData.teamMaterial[1];
                myAttackTarget = GameObject.FindWithTag(GameTags.FlagBlue).transform;
                myDefendTarget = GameObject.FindWithTag(GameTags.FlagRed).transform;
            }
            SetHealth(unitData.maxHealthPoints);
            SetDefense(unitData.defense);
            myTransform = GetComponent<Transform>();
            navMeshAgent.speed = unitData.moveSpeed;
            distanceFromFinalDestiny = 999; //just want to make sure if does not trigger as soon as it spawns
            
            //Check first destination
            isAttacking = !IsAlly || GameManager.Instance.GetIfIsAttacking;
            SetDestination(isAttacking ? myAttackTarget.position : myDefendTarget.position);
        }
        // =============================================================================================================
        /// <summary>
        /// Handles movement direction, target and animation.
        /// </summary>
        private void HandleMovement()
        {
            if (!IsActivated)
                return;
            //Check velocity and animate movement
            myCurrentVelocity = navMeshAgent.velocity.magnitude;
            myAnimator.SetFloat(AnimMoveSpeed, myCurrentVelocity);
            
            //Set timer to search for a target
            if (myNearestTarget == null)
            {
                //If we are in combat, set it false and set the final destination again.
                if (isInCombat)
                {
                    isInCombat = false;
                    startedToShoot = false;
                    SetDestination(isAttacking ? myAttackTarget.position : myDefendTarget.position);
                }
                currentSearchTimer -= Time.deltaTime;
                if (currentSearchTimer <= 0)
                {
                    currentSearchTimer = searchTimer;
                    if (isAttacking)
                        LookForNearestEnemy();
                    else
                        LookForNearestFriendlyBuilding();
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
            if (!IsActivated)
                return;
            //Attack wait timer is always on even if you don't have a target
            atkWaitTimer -= Time.deltaTime;
            if (myNearestTarget != null && isInCombat)
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
        /// <summary>
        /// Handles when this unit is defending
        /// </summary>
        private void HandleDefending()
        {
            if (!IsActivated)
                return;
            //Check if there are enemies nearby
            if (myNearestTarget != null && !isAttacking && !isInCombat)
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
        /// Handles win or lose condition for all units, player or enemy.
        /// </summary>
        private void HandleWinOrLoseCondition()
        {
            if (!IsActivated)
                return;
            //Only works, if we don't have a target to look for
            if (myNearestTarget != null)
                return;

            distanceFromFinalDestiny = Vector3.Distance(myAttackTarget.position, myTransform.position);
            if (distanceFromFinalDestiny <= 3)
            {
                //Trigger win or lose, depending if this unit is from player or not
                GameManager.Instance.TriggerFinishGame(IsAlly);
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
                possibleHits, layerToSearchForAtk);
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
        /// It will look for anything allied building that is near, while walking towards the friendly flag to defend.
        /// </summary>
        private void LookForNearestFriendlyBuilding()
        {
            var possibleHits = new Collider[20];
            var nearestDist = 999f;
            var size = Physics.OverlapSphereNonAlloc(myTransform.position, checkForEnemyRadius,
                possibleHits, layerToSearchForDef);
            for (var i = 0; i < size; i++)
            {
                if (IsAlly)
                {
                    if (possibleHits[i].CompareTag(GameTags.TeamRed))
                        continue;
                }
                else
                {
                    if (possibleHits[i].CompareTag(GameTags.TeamBlue))
                        continue;
                }
                var dist = Vector3.Distance(myTransform.position, possibleHits[i].transform.position);
                if (nearestDist > dist)
                {
                    nearestDist = dist;
                    myNearestTarget = possibleHits[i].GetComponent<LiveObject>();
                    SetDestination(myNearestTarget.GetPosition);
                    isInCombat = false;
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
                //Check if target is still alive, I will do this so the target will have time to play death animation
                if (myNearestTarget.GetMyHealth <= 0)
                    myNearestTarget = null;
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
                //Check if target is still alive, I will do this so the target will have time to play death animation
                if (myNearestTarget.GetMyHealth <= 0)
                    myNearestTarget = null;
            }
        }
        // =============================================================================================================
        /// <summary>
        /// To be called when this object is destroyed.
        /// </summary>
        protected override void OnObjectDestroyed()
        {
            //Change tag, so this unit will not be a target anymore
            tag = GameTags.Untagged;
            //Play animation
            myAnimator.SetTrigger(AnimIsDead);
            //Destroy
            Invoke(nameof(DestroyAfterAnimation), 2);
        }
        // =============================================================================================================
        /// <summary>
        /// Just destroy the gameObject
        /// </summary>
        private void DestroyAfterAnimation()
        {
            VfxManager.Instance.CallVFx(3, transform.position, Quaternion.identity);
            if (IsAlly)
            {
                GameManager.Instance.MoraleAdd(-1);
                GameManager.Instance.AddUnitLost();
            }
            else
            {
                GameManager.Instance.ScorePerUnit();
                GameManager.Instance.AddUnitDestroyed();
            }
            Destroy(gameObject);
        }
        // =============================================================================================================
    }
}