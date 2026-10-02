using UnityEngine;

[RequireComponent(typeof(UnityEngine.CharacterController))]
public class DownhillBike : MonoBehaviour
{
    [Header("Visual Model Reference")]
    [SerializeField] private Transform bikeVisual;

    [Header("Wheel Raycast Offsets")]
    [Tooltip("Distance from center to front wheel along Z axis")]
    [SerializeField] private float frontWheelOffsetZ = 1.0f;
    [Tooltip("Distance from center to back wheel along Z axis")]
    [SerializeField] private float backWheelOffsetZ = -1.0f;
    [Tooltip("Height above ground to start the ray")]
    [SerializeField] private float rayOriginHeight = 1.0f;
    [Tooltip("How far down to cast")]
    [SerializeField] private float rayDistance = 4.0f;

    [Header("Speed & Slope Dynamics")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float slopeAccelerationFactor = 35f;
    [SerializeField] private float drag = 0.99f;
    [SerializeField] private float stickForce = 25f;
    [SerializeField] private float visualAlignSpeed = 15f;
    [SerializeField] private LayerMask roadLayer = ~0; // Defaults to Everything

    private UnityEngine.CharacterController _controller;
    private float _currentSpeed;

    private void Awake()
    {
        _controller = (UnityEngine.CharacterController)GetComponent(typeof(UnityEngine.CharacterController));
        _currentSpeed = baseSpeed;

        if (bikeVisual == null && transform.childCount > 0)
        {
            bikeVisual = transform.GetChild(0);
        }
    }

    private void Update()
    {
        // 1. Raycast positions for front and rear contact points
        Vector3 frontRayStart = transform.position + (transform.forward * frontWheelOffsetZ) + (Vector3.up * rayOriginHeight);
        Vector3 backRayStart = transform.position + (transform.forward * backWheelOffsetZ) + (Vector3.up * rayOriginHeight);

        bool frontHit = Physics.Raycast(frontRayStart, Vector3.down, out RaycastHit hitFront, rayDistance, roadLayer);
        bool backHit = Physics.Raycast(backRayStart, Vector3.down, out RaycastHit hitBack, rayDistance, roadLayer);

        Vector3 moveVelocity;

        if (frontHit && backHit)
        {
            // Vector pointing directly from back wheel contact to front wheel contact
            Vector3 groundSlopeForward = (hitFront.point - hitBack.point).normalized;
            Vector3 averageNormal = ((hitFront.normal + hitBack.normal) * 0.5f).normalized;

            // Align visual model to slope line
            if (bikeVisual != null)
            {
                Quaternion targetRot = Quaternion.LookRotation(groundSlopeForward, averageNormal);
                bikeVisual.rotation = Quaternion.Slerp(bikeVisual.rotation, targetRot, visualAlignSpeed * Time.deltaTime);
            }

            // Acceleration calculation
            float slopeSteepness = Mathf.Clamp01(-groundSlopeForward.y);
            _currentSpeed += slopeSteepness * slopeAccelerationFactor * Time.deltaTime;

            // Move along the calculated slope line with ground stick
            moveVelocity = (groundSlopeForward * _currentSpeed) + (Vector3.down * stickForce);
        }
        else
        {
            // Airborne fallback
            moveVelocity = (transform.forward * _currentSpeed) + (Vector3.down * 40f);
        }

        _currentSpeed *= Mathf.Pow(drag, Time.deltaTime * 60f);
        _currentSpeed = Mathf.Max(_currentSpeed, baseSpeed);

        _controller.Move(moveVelocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw debug rays in the Scene view to verify contact
        Vector3 front = transform.position + (transform.forward * frontWheelOffsetZ) + (Vector3.up * rayOriginHeight);
        Vector3 back = transform.position + (transform.forward * backWheelOffsetZ) + (Vector3.up * rayOriginHeight);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(front, Vector3.down * rayDistance);
        Gizmos.DrawRay(back, Vector3.down * rayDistance);
    }
}