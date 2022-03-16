using UnityEngine;

namespace Custom
{
    public class CameraController : MonoBehaviour
    {
        [Tooltip("The speed at which the camera will move in the xz plane.")]
        [Range(float.Epsilon, 100f)]
        [SerializeField]
        private float panSpeed = 100f;

        [Tooltip("The speed at which the camera will move in the y direction.")]
        [Range(float.Epsilon, 100000f)]
        [SerializeField]
        private float scrollSpeed = 5000f;

        [Tooltip("Minimum/maximum values imposed in the y coordinate.")] [SerializeField]
        private float yMin, yMax;

        [Tooltip("The speed at which the camera will rotate around its y axis.")]
        [Range(float.Epsilon, 1000f)]
        [SerializeField]
        private float rotationSpeed = 80f;

        private void Update()
        {
            var trans = transform;
            var pos = trans.position;

            // Get the forward/backward input:
            var forward = trans.forward;
            forward.y = 0;
            if (Input.GetKey("s"))
                forward *= -1;
            else if (!Input.GetKey("w"))
                forward *= 0;

            // Get the lateral input:
            var right = trans.right;
            right.y = 0;
            if (Input.GetKey("a"))
                right *= -1;
            else if (!Input.GetKey("d"))
                right *= 0;

            // Get the up/down input:
            var scroll = Input.GetAxis("Mouse ScrollWheel");

            // Combine and apply the input motion to the camera:
            pos += Time.deltaTime * panSpeed * (forward + right);
            pos.y += scroll * scrollSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, yMin, yMax);
            trans.position = pos;

            // Rotate the camera around the y global axis:
            var rot = Vector3.zero;
            if (Input.GetKey("q"))
                rot.y = -Time.deltaTime * rotationSpeed;
            if (Input.GetKey("e"))
                rot.y = Time.deltaTime * rotationSpeed;
            trans.Rotate(rot, Space.World);
        }
    }
}