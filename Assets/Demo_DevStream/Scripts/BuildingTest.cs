using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTest : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private Renderer[] myRenderers;
    private bool changeToggle;
    
    // =============================================================================================================
    private void Update()
    {
        if (!Input.GetButtonDown("Fire1"))
            return;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 100))
        {
            myRenderers = hitInfo.transform.GetComponentsInChildren<Renderer>();
            for (var x = 0; x < myRenderers.Length; x++)
            {
                for (var i = 0; i < myRenderers[x].materials.Length; i++)
                {
                    changeToggle = !changeToggle;
                    myRenderers[x].materials[i].SetFloat("_Highlight", changeToggle ? 1 : 0);
                }
            }
        }
    }
    // =============================================================================================================
}
