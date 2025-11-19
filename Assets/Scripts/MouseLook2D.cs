using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook2D : MonoBehaviour
{
    [Header("Target to Rotate")]
    [SerializeField] private Transform targetTransform;

    [SerializeField] private bool flipCharacterSprite = true;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Update()
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("MouseLook2D: Target Transform is not assigned!");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("MouseLook2D: No camera tagged 'MainCamera' found!");
            return;
        }

        // Get mouse position directly from the new Input System
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0;

        Vector3 directionToMouse = mouseWorldPosition - targetTransform.position;

        float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

        targetTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (flipCharacterSprite && spriteRenderer != null)
        {
            if (directionToMouse.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (directionToMouse.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
    }
}
