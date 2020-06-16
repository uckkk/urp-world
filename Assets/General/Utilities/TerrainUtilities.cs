using UnityEngine;

namespace General.Utilities
{
    /// <inheritdoc />
    /// <summary>
    /// Many terrain utilities to be used in-game.
    /// There must be only one of this script in scene.
    /// </summary>
    public class TerrainUtilities : MonoBehaviour
    {
        public static TerrainUtilities Instance { get; private set; }
        
        [Tooltip("WARNING: if marked as true, it will load back the saved terrain data in Resources folder.")]
        [SerializeField] private bool restoreTerrain;
        [Tooltip("The terrain which you will apply all effects")]
        [SerializeField] private Terrain selectedTerrain;

        private const string backupFolder = "Backups/";
        
        //Cache
        private TerrainData myTerrainData;
        private TerrainData backupTerrain;
        private bool cancelRestore;
        
        // =============================================================================================================
        private void OnEnable()
        {
            if (restoreTerrain)
                BackupTerrainData();
        }
        // =============================================================================================================
        private void OnApplicationQuit()
        {
            if (restoreTerrain && !cancelRestore)
                RestoreTerrainData();
        }
        // =============================================================================================================
        private void Awake()
        {
            //Only one per scene
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        // =============================================================================================================
        /// <summary>
        /// Restore the terrain data saved in Resources folder (if empty, nothing will happen).
        /// You must create a clone (a copy) of TerrainData yourself in Resources folder.
        /// The name of the cloned TerrainData SHOULD be the same as the one using in scene.
        /// Only useful in Editor, in Build versions, the terrain data is not saved after been modified.
        /// </summary>
        private void BackupTerrainData()
        {
            if (selectedTerrain == null)
            {
                Debug.LogError("ERROR: no terrain!");
                return;
            }
 
            myTerrainData = selectedTerrain.terrainData;
 
            // This is the backup name/path of the cloned TerrainData.
            var tdBackupName = backupFolder + myTerrainData.name;
            backupTerrain = Resources.Load<TerrainData>(tdBackupName);
            if (backupTerrain == null)
            {
                Debug.LogWarning("No TerrainData backup in a Resources"+backupFolder+" folder. Restore canceled.");
                cancelRestore = true;
                return;
            }

            RestoreTerrainData();
        }
        // =============================================================================================================
        /// <summary>
        /// Restore terrain data to original values.
        /// Only useful in Editor, in Build versions, the terrain data is not saved.
        /// </summary>
        private void RestoreTerrainData()
        {
            // Terrain collider
            myTerrainData.SetHeights(0, 0, backupTerrain.GetHeights(0, 0, backupTerrain.heightmapResolution, backupTerrain.heightmapResolution));
            // Textures
            myTerrainData.SetAlphamaps(0, 0, backupTerrain.GetAlphamaps(0, 0, backupTerrain.alphamapWidth, backupTerrain.alphamapHeight));
            // Trees
            myTerrainData.treeInstances = backupTerrain.treeInstances;
            // Grasses
            myTerrainData.SetDetailLayer(0, 0, 0, backupTerrain.GetDetailLayer(0, 0, backupTerrain.detailWidth, backupTerrain.detailHeight, 0));
        }
        // =============================================================================================================
        /// <summary>
        /// Paint a texture into the selected terrain.
        /// </summary>
        /// <param name="setTarget">Where I am supposed to paint as a reference to the terrain?</param>
        /// <param name="paintGridSize">Size of the grid for painting</param>
        /// <param name="textureQty">Number of textures</param>
        /// <param name="textureId">ID of the texture</param>
        public void PaintTexture(Transform setTarget, int paintGridSize, int textureQty, int textureId)
        {
            myTerrainData = selectedTerrain.terrainData;
            int mapMaxX = paintGridSize;
            int mapMaxZ = paintGridSize;
            float[,,] map = new float[mapMaxX, mapMaxZ, textureQty];
            // For each point on the alphamap...
            for (var y = 0; y < mapMaxZ; y++)
            {
                for (var x = 0; x < mapMaxX; x++)
                {
                    map[x, y, textureId] = 0;
                    map[x, y, textureId] = 1;
                }
            }
            var tTransform = selectedTerrain.transform;
            var position1 = setTarget.position - new Vector3(0.5f, 0, 0.5f);
            var position2 = tTransform.position;
            var mapX = (int)(((position1.x - position2.x) / myTerrainData.size.x)
                             * myTerrainData.alphamapWidth);
            var mapY = (int)(((position1.z - position2.z) / myTerrainData.size.z)
                             * myTerrainData.alphamapHeight);
            myTerrainData.SetAlphamaps(mapX, mapY, map);
        }
        // =============================================================================================================
        /// <summary>
        /// Remove detail meshes from terrain at the desired position.
        /// </summary>
        /// <param name="setTarget">Where I am supposed to remove grass as a reference to the terrain?</param>
        public void RemoveGrass(Transform setTarget)
        {
            myTerrainData = selectedTerrain.terrainData;
            var position = setTarget.position;
            var radius = setTarget.localScale.magnitude;
            var terrainDetailMapSize = myTerrainData.detailResolution;
  
            var prPxSize = terrainDetailMapSize / myTerrainData.size.x;
  
            var texturePoint3D = position - selectedTerrain.transform.position;
            texturePoint3D = texturePoint3D * prPxSize;
  
            //Debug.Log(texturePoint3D);
  
            float[] xymaxmin = new float[4];
            xymaxmin[0] = texturePoint3D.z + radius;
            xymaxmin[1] = texturePoint3D.z - radius;
            xymaxmin[2] = texturePoint3D.x + radius;
            xymaxmin[3] = texturePoint3D.x - radius;
  
            int[,] map = myTerrainData.GetDetailLayer(0,0, myTerrainData.detailWidth, myTerrainData.detailHeight, 0);
  
            for (var y = 0; y < myTerrainData.detailHeight; y++) {
                for (var x = 0; x < myTerrainData.detailWidth; x++) {
    
                    if(xymaxmin[0] > x && xymaxmin[1] < x && xymaxmin[2] > y && xymaxmin[3] < y)
                        map[x,y] = 0;
                }
            }
            myTerrainData.SetDetailLayer(0,0,0,map);
        }
        // =============================================================================================================
    }
}