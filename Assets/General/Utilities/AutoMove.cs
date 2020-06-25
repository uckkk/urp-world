using UnityEngine;

namespace General.Utilities
{
    /// <summary>
    /// Just make something move forward forever
    /// </summary>
    public class AutoMove : MonoBehaviour
    {
        [SerializeField] private Vector3 speedPerAxis;
        [SerializeField] private bool moveRelativeToRotation = false;

        private Space mode;

        private void Start()
        {
            mode = moveRelativeToRotation ? Space.Self : Space.World;   
        }
        private void Update()
        {
            transform.Translate(speedPerAxis * Time.deltaTime, mode);
        }
    }
}