using UnityEngine;

/// <summary>
/// Underwater swim controller.
/// - Mouse movement rotates the player's facing (pitch + yaw, free-look like a submarine/diver).
/// - W/S move forward/backward along where you're facing.
/// - A/D strafe left/right relative to facing.
/// - Space ascends (world up), Shift descends (world down) -- independent of look pitch,
///   so vertical movement stays predictable even while looking up/down.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class UnderwaterPlayerController : MonoBehaviour
{
    [Header("Look")]
    public float mouseSensitivity = 2f;
    public bool invertY = false;
    public float minPitch = -85f;
    public float maxPitch = 85f;
    public bool lockCursor = true;

    [Header("Movement")]
    public float swimSpeed = 4f;
    public float verticalSpeed = 3f;
    public float acceleration = 6f;      // how quickly you reach target velocity
    public float waterDrag = 2f;         // passive deceleration, simulates water resistance

    [Header("Tilt Feel (optional, purely visual)")]
    public bool bankOnStrafe = true;
    public float maxBankAngle = 15f;
    public float bankSpeed = 4f;

    private Rigidbody rb;
    private float yaw;
    private float pitch;
    private Vector3 currentVelocity;
    private float currentBank;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f; // we handle drag manually for more direct control feel

        Vector3 startEuler = transform.eulerAngles;
        yaw = startEuler.y;
        pitch = startEuler.x;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleLook();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        yaw += mouseX;
        pitch += mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion targetRot = Quaternion.Euler(pitch, yaw, currentBank);
        transform.rotation = targetRot;
    }

    void HandleMovement()
    {
        // Build input-relative direction using the player's current facing (full 3D, since underwater)
        float forwardInput = 0f;
        if (Input.GetKey(KeyCode.W)) forwardInput += 1f;
        if (Input.GetKey(KeyCode.S)) forwardInput -= 1f;

        float strafeInput = 0f;
        if (Input.GetKey(KeyCode.D)) strafeInput += 1f;
        if (Input.GetKey(KeyCode.A)) strafeInput -= 1f;

        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalInput += 1f;   // ascend
        if (Input.GetKey(KeyCode.LeftShift)) verticalInput -= 1f; // descend

        // W/S and A/D move relative to where the camera/player is actually facing (mouse-driven),
        // including pitch, so swimming forward while looking up actually swims upward too.
        Vector3 desiredDir =
            transform.forward * forwardInput +
            transform.right * strafeInput;

        // Vertical (Space/Shift) is always world-space up/down, independent of look pitch,
        // so it stays predictable regardless of where you're facing.
        desiredDir += Vector3.up * verticalInput * (verticalSpeed / Mathf.Max(swimSpeed, 0.0001f));

        Vector3 targetVelocity = desiredDir.sqrMagnitude > 1f ? desiredDir.normalized : desiredDir;
        targetVelocity.x *= swimSpeed;
        targetVelocity.z *= swimSpeed;
        // scale vertical component separately since it was pre-weighted above
        targetVelocity.y = desiredDir.y * swimSpeed;

        bool hasInput = forwardInput != 0f || strafeInput != 0f || verticalInput != 0f;

        if (hasInput)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Water resistance gradually stops you when no input is held
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, waterDrag * Time.fixedDeltaTime);
        }

        rb.linearVelocity = currentVelocity;

        // Optional: slight bank/roll into strafe direction for a more organic swim feel
        if (bankOnStrafe)
        {
            float targetBank = -strafeInput * maxBankAngle;
            currentBank = Mathf.Lerp(currentBank, targetBank, bankSpeed * Time.fixedDeltaTime);
        }
    }
}