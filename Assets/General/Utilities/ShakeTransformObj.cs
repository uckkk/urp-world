using System.Collections;
using UnityEngine;

namespace General.Utilities
{
    /// <summary>
    /// It will make a Transform to shake, for example - can be used to shake a camera.
    /// </summary>
    public class ShakeTransformObj : MonoBehaviour
    {
        [Tooltip("Enable or disable shake effect.")]
        [SerializeField] private bool shakeEnabled;
        
        [Tooltip("If loop is enabled, the shakeDuration will not be used.")]
        [SerializeField] private bool shakeLoop;
        
        [Tooltip("Transform of the object to shake. Grabs the gameObject's transform if null")]
        [SerializeField] private Transform objToShake;
	
        [Tooltip("How long the object should shake for.")]
        [SerializeField] private float shakeDuration;
	
        [Tooltip("Amplitude of the shake. A larger value shakes the object harder.")]
        [SerializeField] private float shakeAmount = 3;
        
        [Tooltip("The more, the less faster. Use short values for faster speed.")]
        [SerializeField] private float shakeSpeed = 4;
        
        [Tooltip("Make it shake double the amount.")]
        [SerializeField] [ReadOnly] private bool doubleShake;
	
        //Cached
        private Vector3 _originalPos;
        private Vector3 _desiredPos;
        
        // =============================================================================================================
        private void Awake()
        {
            if (objToShake == null)
            {
                objToShake = GetComponent(typeof(Transform)) as Transform;
            }
        }
        // =============================================================================================================
        private void OnEnable()
        {
            _originalPos = objToShake.localPosition;
        }
        // =============================================================================================================
        private void Update()
        {
            if (!shakeEnabled)
                return;
            if (!shakeLoop)
                if (!(shakeDuration > 0)) return;
            
            _desiredPos = (doubleShake ? shakeAmount * 4 : shakeAmount) * Random.insideUnitSphere;
            objToShake.localPosition = Vector3.Lerp(objToShake.localPosition, _originalPos + _desiredPos,
                Time.deltaTime * (doubleShake ? shakeSpeed * 2 : shakeSpeed));
            if (!shakeLoop)
                shakeDuration -= Time.deltaTime;
        }
        // =============================================================================================================
        /// <summary>
        /// Set normal shake time to be decreased normally.
        /// </summary>
        public void SetShakeTime(float durationTime, bool makeItDouble)
        {
            shakeDuration = durationTime;
            doubleShake = makeItDouble;
        }
        // =============================================================================================================
        /// <summary>
        /// Shake the obj transform in a time frame, independent of the shakeEnabled.
        /// </summary>
        /// <param name="timeToShake"></param>
        /// <returns></returns>
        private IEnumerator ShakeByTimer(float timeToShake)
        {
            shakeDuration = timeToShake;

            while (shakeDuration > 0)
            {
                objToShake.localPosition = _originalPos + Random.insideUnitSphere * shakeAmount;
                shakeDuration -= 0.1f;
                yield return new WaitForFixedUpdate();
            }
            objToShake.localPosition = _originalPos;
        }
        // =============================================================================================================
    }
}