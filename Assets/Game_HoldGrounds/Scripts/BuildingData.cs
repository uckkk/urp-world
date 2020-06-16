using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Holds data for each blue print and building data.
    /// We could do in a number of different ways, the blue print color change:
    /// - Changing its Material in real time (instead of the whole object), but this could result in more performance
    /// impact.
    /// - Changing material color and transparency with Shader Graph. This could be much better indeed.
    /// </summary>
    [System.Serializable]
    public class BuildingData
    {
        [Tooltip("Just a name to show in our inspector.")]
        public string nameAlias;
        [Tooltip("Type of the prop to check and build.")]
        public PropData propData;
        [Tooltip("The prefab to be instantiated as a building.")]
        public GameObject prefabToCreate;
        [Tooltip("The bluePrint transform to position while trying to build. Must be in the scene already.")]
        public Transform bluePrint;
        [Tooltip("The bluePrint collision format. Must be in the scene already.")]
        public Transform bluePrintCollision;
        
        private MeshRenderer[] _allRenderers;
        private bool _locked;

        // =============================================================================================================
        /// <summary>
        /// Prepare our blueprint to be used.
        /// </summary>
        public void PrepareBluePrint()
        {
            _allRenderers = bluePrint.GetComponentsInChildren<MeshRenderer>();
        }
        // =============================================================================================================
        public void SetRenderersMaterial(bool isLocked, Material mat)
        {
            if (_locked != isLocked)
            {
                _locked = isLocked;
                for (var i = 0; i < _allRenderers.Length; i++)
                    _allRenderers[i].material = mat;
            }
        }
        // =============================================================================================================
        public void ToggleBluePrint(bool show)
        {
            bluePrint.gameObject.SetActive(show);
        }
        // =============================================================================================================
    }
}