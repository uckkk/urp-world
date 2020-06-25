using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Helps creating sound effects in real time, like gun shots, environment, etc.
    /// It will setup all pooling systems to be used.
    /// </summary>
    public class VfxManager : MonoBehaviour
    {
        public static VfxManager Instance { get; private set; }

        [Header("======== Pooling Setup")]
        [SerializeField] private int poolingSize = 10;
        [Tooltip("Where all the pooling objects will go.")]
        [SerializeField] private Transform poolingParent;
        
        [Header("======== Types of VFX")]
        [Tooltip("Collecting items, spawning items, etc.")]
        [SerializeField] private GameObject[] vfxPrefabs;
        private int[] lastVfxPoolId;
        private GameObject[,] poolingVfx;
        
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
        private void Start()
        {
            SetupPoolingSystems();
        }
        // =============================================================================================================
        /// <summary>
        /// Setup pooling system to be ready in-game.
        /// </summary>
        private void SetupPoolingSystems()
        {
            poolingVfx = new GameObject[vfxPrefabs.Length, poolingSize];
            lastVfxPoolId = new int[vfxPrefabs.Length];
            
            for (var i = 0; i < vfxPrefabs.Length; i++)
            {
                for (var x = 0; x < poolingSize; x++)
                {
                    var poolObj = Instantiate(vfxPrefabs[i], poolingParent);
                    poolObj.SetActive(false);
                    poolObj.transform.localPosition = Vector3.zero;
                    poolingVfx[i, x] = poolObj;
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Call a VFX (sound and particles).
        /// </summary>
        /// <param name="vfxOrder">Array id, but also it can come from ItemData, GetVfxCreationId.</param>
        /// <param name="pos">Global position.</param>
        /// <param name="rot">Global rotation.</param>
        public GameObject CallVFx(int vfxOrder, Vector3 pos, Quaternion rot)
        {
            // SaveLog("VFX called: " + vfxOrder);
            poolingVfx[vfxOrder, lastVfxPoolId[vfxOrder]].transform.position = pos;
            poolingVfx[vfxOrder, lastVfxPoolId[vfxOrder]].transform.rotation = rot;
            poolingVfx[vfxOrder, lastVfxPoolId[vfxOrder]].SetActive(true);
            var vfx = poolingVfx[vfxOrder, lastVfxPoolId[vfxOrder]];
            lastVfxPoolId[vfxOrder]++;
            if (lastVfxPoolId[vfxOrder] >= poolingSize)
                lastVfxPoolId[vfxOrder] = 0;
            poolingVfx[vfxOrder, lastVfxPoolId[vfxOrder]].SetActive(false);
            return vfx;
        }
        // =============================================================================================================
        /// <summary>
        /// Call a VFX (sound and particles), set if it is Ally (player) and the damage.
        /// </summary>
        /// <param name="vfxOrder">Array id, but also it can come from ItemData, GetVfxCreationId.</param>
        /// <param name="pos">Global position.</param>
        /// <param name="rot">Global rotation.</param>
        /// <param name="isAlly">In case you need to set if this belongs to the player.</param>
        /// <param name="dmg">Power of damage to apply when it hits someething.</param>
        public void CastProjectile(int vfxOrder, Vector3 pos, Quaternion rot, bool isAlly, float dmg)
        {
            var go = CallVFx(vfxOrder, pos, rot);
            var projectile = go.GetComponent<RangedProjectileHandler>(); 
            projectile.SetIfAlly(isAlly);
            projectile.SetDamage(dmg);
        }
        // =============================================================================================================
    }
}