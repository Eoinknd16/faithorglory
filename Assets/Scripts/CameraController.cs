using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float cameraHeight = 76f;
    public float cameraAngle = 45f;
    public float zoomSpeed = 5f; // Speed for zooming in/out
    public float zoomLimitMin = 5f;
    public float zoomLimitMax = 50f;
    public float rotationSpeed = 3f; // Speed for rotating camera
    public float panSpeed = 0.1f; // Speed multiplier for mouse pan
    public float currentZoom = 50f; // Current camera height

    [Header("Map Size")]
    public float mapWidth = 100f;  // Example map width
    public float mapHeight = 100f; // Example map height

    private Vector2 moveInput;
    public PlayerInput playerInput;
    private InputAction moveAction;

    
    private float currentRotationX = 45f; // Current camera rotation around the Y axis
    private float currentRotationY = 0f; // Current camera rotation around the X axis (tilt)

    private bool isRightMousePressed = false; // Track if right mouse button is held down
    private bool isMiddleMousePressed = false; // Track if middle mouse button is held down
    private Vector2 lastMousePosition;

    void Awake()
    {
        // Get reference to PlayerInput
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }

    void Start()
    {
        // Set initial camera angle and height
        transform.rotation = Quaternion.Euler(cameraAngle, 45f, 0f);
        Vector3 pos = transform.position;

        // Center the camera on the map before any tiles are placed
        Vector3 mapCenter = new Vector3(mapWidth / 2f, cameraHeight, mapHeight / 2f);
        transform.position = mapCenter;

        // Optionally, adjust the zoom level based on the map size to make sure the whole map fits in view
        currentZoom = Mathf.Max(mapWidth, mapHeight);
        Camera.main.orthographicSize = currentZoom;
    }

    void Update()
    {
        // WASD movement
        moveInput = moveAction.ReadValue<Vector2>();

        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;

        Vector3 movement = (forward * moveInput.y + right * moveInput.x) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Middle mouse pan
        HandlePan(forward, right);

        // Lock height to the desired value
        transform.position = new Vector3(transform.position.x, currentZoom, transform.position.z);

        // Handle zooming (scroll wheel)
        HandleZoom();

        // Handle camera rotation (right mouse click)
        HandleRotation();
    }

    private void HandleZoom()
    {
        // Get the scroll wheel input (scroll up/down)
        float scroll = Mouse.current.scroll.ReadValue().y;

        // Adjust the zoom level
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, zoomLimitMin, zoomLimitMax);

        // Apply the zoom to the camera height
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, currentZoom, pos.z);
    }

    private void HandleRotation()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            if (!isRightMousePressed)
            {
                isRightMousePressed = true;
            }

            float mouseX = Mouse.current.delta.x.ReadValue();
            float mouseY = Mouse.current.delta.y.ReadValue();

            currentRotationX -= mouseY * rotationSpeed;
            currentRotationY += mouseX * rotationSpeed;

            currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);

            transform.rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        }
        else
        {
            isRightMousePressed = false;
        }
    }

    private void HandlePan(Vector3 forward, Vector3 right)
    {
        if (Mouse.current.middleButton.isPressed)
        {
            if (!isMiddleMousePressed)
            {
                isMiddleMousePressed = true;
                lastMousePosition = Mouse.current.position.ReadValue();
            }
            else
            {
                Vector2 currentMousePosition = Mouse.current.position.ReadValue();
                Vector2 delta = currentMousePosition - lastMousePosition;
                lastMousePosition = currentMousePosition;

                Vector3 panMovement = -right * delta.x * panSpeed - forward * delta.y * panSpeed;
                transform.position += panMovement;
            }
        }
        else
        {
            isMiddleMousePressed = false;
        }
    }
}
