using UnityEngine;

namespace General.Utilities
{
    /// <summary>
    /// To save each terrain data info in-game
    /// </summary>
    [System.Serializable]
    public struct TerrainInfoData
    {
        public TerrainData mTerrainData;
        public int alphamapWidth;
        public int alphamapHeight;
        public float[,,] mSplatmapData;
        public int mNumTextures;
        public Vector3 size;
        public Vector3 position;
    }
    public class TerrainGetTexturePos : MonoBehaviour
    {
        [Tooltip("Disable in case you get errors from Terrain Toolbox, so you can test.")]
        [SerializeField] private bool disable;
        // [SerializeField] private string terrainLayer = "Terrain";
        [Tooltip("Add here all terrain data from the game. They need to be the same name as terrains.")]
        [SerializeField] private Terrain[] allTerrains;
        [SerializeField] [ReadOnly] private TerrainInfoData[] dataInfoList;
        [SerializeField] [ReadOnly] private bool tracking;
        [SerializeField] [ReadOnly] private string lastTerrainName;
        [SerializeField] [ReadOnly] private int lastTerrainId;
        
        // [SerializeField] [ReadOnly] private TerrainData mTerrainData;
        // [SerializeField] [ReadOnly] private Terrain mTerrain;
        
        private Transform myTransform;
        
        // =============================================================================================================
        private void Start()
        {
            myTransform = GetComponent<Transform>();
            LoadTerrainData();
        }
        // =============================================================================================================
        private void Update()
        {
            HandleGetTerrainBelow();
        }
        // =============================================================================================================
        private void HandleGetTerrainBelow()
        {
            var ground = 1 << 18;
            if (Physics.Raycast(myTransform.position, Vector3.up * -1, out var hit, 100, ground))
            {
                tracking = true;
                if (lastTerrainName != hit.transform.name)
                {
                    for (var i = 0; i < allTerrains.Length; i++)
                    {
                        if (hit.transform.name == allTerrains[i].name)
                        {
                            // mTerrainData = allTerrainData[i];
                            lastTerrainId = i;
                            lastTerrainName = hit.transform.name;
                            break;
                        }
                    }
                    // mTerrain = hit.transform.GetComponent<Terrain>();
                    // mTerrainData = mTerrain.terrainData;
                    // lastTerrainName = hit.transform.name;
                    // GetTerrainProps();
                }
            }
            else
            {
                tracking = false;
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Load all terrain data info for ALL terrains in game, this is used to save time when the player 
        /// changes the terrain data to read. This can take some time if there are a LOT of terrains.
        /// </summary>
        private void LoadTerrainData()
        {
            if (allTerrains.Length == 0)
            {
                allTerrains = Terrain.activeTerrains;
            }
            dataInfoList = new TerrainInfoData[allTerrains.Length];
            for (var i = 0; i < allTerrains.Length; i++)
            {
                dataInfoList[i].mTerrainData = allTerrains[i].terrainData;
                dataInfoList[i].alphamapWidth = dataInfoList[i].mTerrainData.alphamapWidth;
                dataInfoList[i].alphamapHeight = dataInfoList[i].mTerrainData.alphamapHeight;
                dataInfoList[i].mSplatmapData = dataInfoList[i].mTerrainData.GetAlphamaps(0, 0,
                    dataInfoList[i].alphamapWidth, dataInfoList[i].alphamapHeight);
                dataInfoList[i].mNumTextures = dataInfoList[i].mSplatmapData.Length /
                                              (dataInfoList[i].alphamapWidth * dataInfoList[i].alphamapHeight);
                dataInfoList[i].size = dataInfoList[i].mTerrainData.size;
                dataInfoList[i].position = allTerrains[i].GetPosition();
            }
            // if (mTerrainData == null)
            // {
            //     mTerrain = GetClosestCurrentTerrain(transform.position);
            //     mTerrainData = mTerrain.terrainData;
            // }
            
            // alphamapWidth = mTerrainData.alphamapWidth;
            // alphamapHeight = mTerrainData.alphamapHeight;
            //
            // mSplatmapData = mTerrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
            // mNumTextures = mSplatmapData.Length / (alphamapWidth * alphamapHeight);
        }
        // =============================================================================================================
        private Vector3 ConvertToSplatMapCoordinate(Vector3 playerPos)
        {
            var vecRet = new Vector3();
            // var ter = mTerrain;
            // var terPosition = ter.transform.position;
            // var terrainData = ter.terrainData;
            var terPosition = dataInfoList[lastTerrainId].position;
            vecRet.x = ((playerPos.x - terPosition.x) /
                        dataInfoList[lastTerrainId].size.x) * dataInfoList[lastTerrainId].alphamapWidth;
            vecRet.z = ((playerPos.z - terPosition.z) /
                        dataInfoList[lastTerrainId].size.z) * dataInfoList[lastTerrainId].alphamapHeight;
            return vecRet;
        }
        // =============================================================================================================
        private int GetActiveTerrainTextureIdx(Vector3 pos)
        {
            var terrainCord = ConvertToSplatMapCoordinate(pos);
            var ret = 0;
            for (var i = 0; i < dataInfoList[lastTerrainId].mNumTextures; i++)
            {
                if (ret < dataInfoList[lastTerrainId].mSplatmapData[(int) terrainCord.z, (int) terrainCord.x, i])
                    ret = i;
            }
            return ret;
        }
        // =============================================================================================================
        public int GetTerrainAtPosition(Vector3 pos)
        {
            if (!tracking) return 0;
            return disable ? 0 : GetActiveTerrainTextureIdx(pos);
        }
        // =============================================================================================================
    }
}