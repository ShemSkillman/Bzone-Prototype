using UnityEngine;

namespace EZHover
{
    public class Look : MonoBehaviour
    {
        [SerializeField] float verticalTurnSpeed = 5f;
        [SerializeField] float horizontalTurnSpeed = 5f;

        Rigidbody rb;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            float x = Mathf.Clamp(Input.GetAxisRaw("Mouse X"), -1, 1);
            rb.AddTorque(Vector3.up * x * horizontalTurnSpeed);

            float y = Mathf.Clamp(Input.GetAxisRaw("Mouse Y"), -1, 1);

            var right = new Vector3(transform.right.x, 0f, transform.right.z);
            rb.AddTorque(right * y * verticalTurnSpeed * -1);
        }
    }
}