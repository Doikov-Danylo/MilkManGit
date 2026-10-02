using UnityEngine;
using UnityEngine.InputSystem; // Import the new Input System namespace

public class FirstPersonLook : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivityX = 0.1f;
    [SerializeField] private float mouseSensitivityY = 0.1f;

    [Header("Clamping Limits")]
    [SerializeField] private float maxPitchAngle = 70f;
    [SerializeField] private float minPitchAngle = -70f;
    [SerializeField] private float maxYawAngle = 80f;

    private float _yaw = 0f;
    private float _pitch = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // Read raw mouse delta from the new Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Note: delta is already per-frame pixel displacement, so Time.deltaTime is not needed
        float mouseX = mouseDelta.x * mouseSensitivityX;
        float mouseY = mouseDelta.y * mouseSensitivityY;

        _yaw += mouseX;
        _pitch -= mouseY;

        // Clamp rotation relative to the bike's frame
        _yaw = Mathf.Clamp(_yaw, -maxYawAngle, maxYawAngle);
        _pitch = Mathf.Clamp(_pitch, minPitchAngle, maxPitchAngle);

        // Apply local rotation relative to parent
        transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}