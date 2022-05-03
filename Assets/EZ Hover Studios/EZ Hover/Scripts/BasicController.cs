using UnityEngine;

namespace EZHover
{
    public class BasicController : MonoBehaviour
    {
        HoverMovement hoverMovement;
        HoverLook hoverLook;

        private void Awake()
        {
            hoverMovement = GetComponentInChildren<HoverMovement>();
            hoverLook = GetComponentInChildren<HoverLook>();
        }

        void Update()
        {
            hoverMovement?.Move(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
            hoverLook?.Turn(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
        }
    }
}