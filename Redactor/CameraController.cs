using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float panSpeed = 0.01f;
    [SerializeField] private float smoothTime = 0.15f;

    private float minZoom = 3f;
    private float maxZoom = 5f;
    private float minY = 210f;
    private float maxY = 230f;
    private float minPosY = 5f;
    private float maxPosY = 9f;


    private float targetY;
    private float currentYVelocity;
    private float targetZoom;
    private float currentZoomVelocity;
    private float targetPosY;
    private float currentPosYVelocity;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        targetY = transform.eulerAngles.y;
        targetZoom = mainCamera.orthographicSize;
        targetPosY = transform.position.y;
    }

    
    void Update()
    {
        HandleZoom();
        HandleRotation();
        HandlePan();
        // Плавное вращение
        float newY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetY, ref currentYVelocity, smoothTime);
        Vector3 angles = transform.eulerAngles;
        angles.y = newY;
        transform.eulerAngles = angles;

        // Плавный зум
        if (mainCamera.orthographic)
        {
            float newZoom = Mathf.SmoothDamp(mainCamera.orthographicSize, targetZoom, ref currentZoomVelocity, smoothTime);
            mainCamera.orthographicSize = newZoom;
        }

        // Плавное смещение по Y
        float newPosY = Mathf.SmoothDamp(transform.position.y, targetPosY, ref currentPosYVelocity, smoothTime);
        Vector3 pos = transform.position;
        pos.y = newPosY;
        transform.position = pos;
    }
    private void HandleZoom()
    {
         float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (mainCamera.orthographic)
            {
                targetZoom -= scroll * zoomSpeed * 0.1f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
    }
    private void HandleRotation()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            float mouseX = Mouse.current.delta.ReadValue().x;
            if (Mathf.Abs(mouseX) > 0.01f)
            {
                targetY += mouseX * rotationSpeed * Time.deltaTime * 0.01f;
                targetY = Mathf.Clamp(targetY, minY, maxY);
            }
        }
    }
    private void HandlePan()
{
    if (Mouse.current.middleButton.isPressed)
        {
            float mouseY = Mouse.current.delta.ReadValue().y;
            if (Mathf.Abs(mouseY) > 0.01f)
            {
                targetPosY -= mouseY * panSpeed;
                targetPosY = Mathf.Clamp(targetPosY, minPosY, maxPosY);
            }
        }
}
}
