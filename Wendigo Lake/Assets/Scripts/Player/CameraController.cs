using HietakissaUtils;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float sensitivity = 1f;
    [SerializeField] bool invertVertical;
    [SerializeField] bool invertHorizontal;

    [SerializeField] float maxLookAngle = 90f;

    [Header("References")]
    [SerializeField] Transform cameraHolder;
    [SerializeField] MovementController player;

    [Header("Y Damping")]
    [SerializeField] float baseYDamp = 27f;
    [SerializeField] float crouchYDamp = 14f;

    [Header("FOV")]
    [SerializeField] float baseFOV = 95f;
    [SerializeField] float crouchFOV = 93f;
    [SerializeField] float sprintFOV = 105f;
    [SerializeField] float FOVDamping = 9f;

    float currentFOV;
    float xRot, yRot;

    Camera cam;


    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        //yRot = transform.rotation.y;
        SetRotation(transform.rotation);

        cam = GetComponent<Camera>();
        currentFOV = baseFOV;
    }

    void LateUpdate()
    {
        GetInput();
        Rotate();
        Move();
        LerpFOV();
    }

    void GetInput()
    {
        yRot += Input.GetAxisRaw("Mouse X") * (invertHorizontal ? -sensitivity : sensitivity);
        xRot += Input.GetAxisRaw("Mouse Y") * (invertVertical ? sensitivity : -sensitivity);

        ClampRotation();
    }

    void Rotate()
    {
        transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
        player.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
    }

    void SetRotation(Quaternion rot)
    {
        Vector3 euler = rot.eulerAngles;

        xRot = euler.x;
        yRot = euler.y;

        if (xRot > 270) xRot -= 360;

        ClampRotation();
    }

    void ClampRotation()
    {
        xRot = Mathf.Clamp(xRot, -maxLookAngle, maxLookAngle);

        if (yRot > 360f) yRot -= 360f;
        else if (yRot < -360f) yRot += 360f;
    }

    void Move()
    {
        bool crouching = player.IsCrouching();

        float damping;
        if (crouching) damping = crouchYDamp;
        else damping = baseYDamp;

        //transform.position = cameraHolder.position;

        float dampedY = Mathf.Lerp(transform.position.y, cameraHolder.position.y, damping * Time.deltaTime);
        transform.position = cameraHolder.position.SetY(dampedY);
        
        //transform.position = Vector3.Lerp(transform.position, transform.position.SetY(cameraHolder.position.y), damping * Time.deltaTime);
    }

    void LerpFOV()
    {
        float targetFOV;

        if (player.IsSprinting()) targetFOV = sprintFOV;
        else if (player.IsCrouching()) targetFOV = crouchFOV;
        else targetFOV = baseFOV;

        currentFOV = Mathf.Lerp(currentFOV, targetFOV, FOVDamping * Time.deltaTime);
        cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(currentFOV, cam.aspect);
    }
}
