using General.Utilities;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Use this script together with BuildingBehaviour to make a Building to attack (shoot) something.
    /// </summary>
    [RequireComponent(typeof(BuildingBehaviour))] //this means we need a script of this type in the same GameObject
    public class BuildingDefenseMode : MonoBehaviour
    {
        private BuildingBehaviour buildingMain;
        
        [Header("====== SETUP")]
        [Tooltip("What will be used as graphics to animate for shooting.")]
        [SerializeField] private Transform shootingTower;
        [Tooltip("Make a transform to rotate, just as visual.")]
        [SerializeField] private Transform visualToRotate;
        [Tooltip("Check id order in VfxManager.")]
        [SerializeField] private int projectileToShoot;
        [Tooltip("What are the game objects this building will look to attack.")]
        [SerializeField] private LayerMask layerToSearchFor;
        [Tooltip("Timer to search for a target, we don't want to do this in each frame.")]
        [SerializeField] private float timerToSearchForTarget = 2;
        [Tooltip("Where to aim.")]
        [SerializeField] [ReadOnly] private LiveObject myTarget;

        [SerializeField] [ReadOnly] private float currentTimerToShoot;
        [SerializeField] [ReadOnly] private float currentTimerToSearch;
        
        // =============================================================================================================
        private void Start()
        {
            buildingMain = GetComponent<BuildingBehaviour>();
            currentTimerToShoot = buildingMain.GetPropType.defenseAttackRate;
            currentTimerToSearch = timerToSearchForTarget;
        }
        // =============================================================================================================
        private void Update()
        {
            HandleTarget();
        }
        // =============================================================================================================
        /// <summary>
        /// Rotates to target and shoot when possible
        /// </summary>
        private void HandleTarget()
        {
            if (!buildingMain.IsEnabled)
                return;
            
            if (myTarget == null)
            {
                currentTimerToSearch -= Time.deltaTime;
                if (currentTimerToSearch <= 0)
                {
                    currentTimerToSearch = timerToSearchForTarget;
                    LookForNearestEnemy();
                }
                return;
            }
            //Check if target is still alive, I will do this so the target will have time to play death animation
            if (myTarget.GetMyHealth <= 0)
            {
                myTarget = null;
                return;
            }
            
            //Check if the target is still in the minimum distance
            var dist = Vector3.Distance(shootingTower.position, myTarget.transform.position);
            if (dist > buildingMain.GetPropType.defenseRadiusDistance)
            {
                myTarget = null;
                return;
            }
            
            //Make visual
            if (visualToRotate != null)
            {
                // visualToRotate.Rotate(0, Time.deltaTime * 10, 0);
                var dir = myTarget.transform.position - visualToRotate.position;
                dir.y = 0;
                var rot = Quaternion.LookRotation(dir);
                visualToRotate.rotation = Quaternion.Slerp(visualToRotate.rotation, rot, Time.deltaTime * 10);
            }
            
            //Rotate to target (THIS IS FOR AIM) - so pick an empty game object just as aim object
            var direction = myTarget.transform.position - shootingTower.position;
            // direction.y = 0;
            var rot2 = Quaternion.LookRotation(direction);
            shootingTower.rotation = Quaternion.Slerp(shootingTower.rotation, rot2, Time.deltaTime * 10);
            
            //Shoot
            currentTimerToShoot -= Time.deltaTime;
            if (currentTimerToShoot <= 0)
            {
                currentTimerToShoot = buildingMain.GetPropType.defenseAttackRate;
                VfxManager.Instance.CastProjectile(projectileToShoot, shootingTower.position, shootingTower.rotation,
                    buildingMain.IsItAlly, buildingMain.GetPropType.defenseDamage);
            }
        }
        // =============================================================================================================
        /// <summary>
        /// It will look for anything that is near.
        /// </summary>
        private void LookForNearestEnemy()
        {
            var possibleHits = new Collider[20];
            var nearestDist = 999f;
            var size = Physics.OverlapSphereNonAlloc(shootingTower.position,
                buildingMain.GetPropType.defenseRadiusDistance, possibleHits, layerToSearchFor);
            for (var i = 0; i < size; i++)
            {
                if (buildingMain.IsItAlly)
                {
                    if (possibleHits[i].CompareTag(GameTags.TeamBlue))
                        continue;
                }
                else
                {
                    if (possibleHits[i].CompareTag(GameTags.TeamRed))
                        continue;
                }
                var dist = Vector3.Distance(shootingTower.position, possibleHits[i].transform.position);
                if (nearestDist > dist)
                {
                    nearestDist = dist;
                    myTarget = possibleHits[i].GetComponent<LiveObject>();
                }
            }
        }
        // =============================================================================================================
    }
}