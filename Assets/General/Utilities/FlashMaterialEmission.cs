using UnityEngine;

namespace General.Utilities
{
    /// <summary>
    /// Just make the emission of a material to flash.
    /// </summary>
    public class FlashMaterialEmission : MonoBehaviour
    {
        [Header("======== BLINK EFFECT")]
        [SerializeField] private Renderer[] myRenderers;
        [SerializeField] private float blinkSpeed = 2;
        [SerializeField] [ColorUsage(true, true)] private Color blinkColor = Color.white;
        [SerializeField] private float blinkFloor = 0;
        [SerializeField] private float blinkCeiling = 1.5f;
        [SerializeField] private float minimumLight = 0.2f;
        [SerializeField] private bool emissionIsOn;
        private Material[] myMaterials;
        private float emissionSmooth;
        private Color desiredColor;
        private readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        
        // =============================================================================================================
        private void Start()
        {
            myMaterials = new Material[myRenderers.Length];
            for (var i = 0; i < myRenderers.Length; i++)
                myMaterials[i] = myRenderers[i].GetComponent<Renderer>().material;
        }
        // =============================================================================================================
        private void OnBecameInvisible()
        {
            emissionIsOn = false;
        }
        // =============================================================================================================
        private void OnBecameVisible()
        {
            emissionIsOn = true;
        }
        // =============================================================================================================
        private void Update()
        {
            if (!emissionIsOn)
                return;
            emissionSmooth = blinkFloor + Mathf.PingPong (Time.time * blinkSpeed, blinkCeiling - blinkFloor);
            desiredColor = blinkColor * (Mathf.LinearToGammaSpace (emissionSmooth) + minimumLight);
            for (var i = 0; i < myRenderers.Length; i++)
                myMaterials[i].SetColor (EmissionColor, desiredColor);
        }
        // =============================================================================================================
    }
}