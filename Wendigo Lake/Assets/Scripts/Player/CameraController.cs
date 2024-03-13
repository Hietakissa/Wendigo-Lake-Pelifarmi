using HietakissaUtils;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using System.IO;

public class CameraController : MonoBehaviour
{
    [SerializeField] float sensitivity = 1f;
    [SerializeField] bool invertVertical;
    [SerializeField] bool invertHorizontal;

    [SerializeField] float maxLookAngle = 90f;

    [Header("References")]
    [SerializeField] Camera cam;
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


    [Header("Handheld Variables")]
    [SerializeField] RenderTexture outputTexture;
    [SerializeField] RawImage outputImage;
    [SerializeField] Camera handheldCam;
    [SerializeField] float imageDelay;

    [SerializeField] TextureFormat format;

    [Header("Flash")]
    [SerializeField] float flashCooldown;
    [SerializeField] Light flashLight;
    [SerializeField] Spring intensitySpring;
    [SerializeField] Image[] flashProgressImages;
    [SerializeField] GameObject flashIndicator;
    float flashProgress;
    bool flash;

    bool inCamera;


    void Awake()
    {
        ExitCamera();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        //yRot = transform.rotation.y;
        SetRotation(transform.rotation);

        currentFOV = baseFOV;
    }

    void LateUpdate()
    {
        GetInput();
        Rotate();
        Move();
        LerpFOV();

        HeldCamera();

        flashLight.intensity = intensitySpring.GetValue();
    }

    void HeldCamera()
    {
        if (Input.GetMouseButtonDown(1)) EnterCamera();
        else if (Input.GetMouseButtonUp(1)) ExitCamera();

        if (inCamera)
        {
            if (Input.GetMouseButtonDown(0)) CaptureImage();
            else if (Input.GetKeyDown(KeyCode.F) && flashProgress >= flashCooldown) SetFlash(!flash);
        }

        // bad bad code but no time
        if (flashProgress < flashCooldown)
        {
            flashProgress += Time.deltaTime;

            if (flashProgress >= flashCooldown)
            {
                flashProgress = flashCooldown;
            }

            foreach (Image image in flashProgressImages) image.fillAmount = flashProgress / flashCooldown;
        }
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


    void EnterCamera()
    {
        inCamera = true;
        UpdateCameraStates();

        EventManager.EnterCamera();
    }

    void ExitCamera()
    {
        inCamera = false;
        UpdateCameraStates();

        EventManager.ExitCamera();
    }

    void UpdateCameraStates()
    {
        cam.gameObject.SetActive(!inCamera);
        handheldCam.gameObject.SetActive(inCamera);
    }

    void SetFlash(bool state)
    {
        flash = state;

        flashIndicator.SetActive(flash);
    }


    void CaptureImage()
    {
        StartCoroutine(CaptureImageCor());



        IEnumerator CaptureImageCor()
        {
            if (flash)
            {
                SetFlash(false);

                intensitySpring.SetValue(intensitySpring.Max);
                flashProgress = 0f;
            }


            yield return new WaitForSeconds(imageDelay);


            handheldCam.targetTexture = outputTexture;
            handheldCam.Render();

            Texture2D image = ToTexture2D(outputTexture, format);
            outputImage.texture = image;
            handheldCam.targetTexture = null;
            

            CheckObjectsInPhoto();
            SaveImageAsPNG();


            void SaveImageAsPNG()
            {
                Color[] pixels = image.GetPixels();
                for (int i = 0; i < pixels.Length; i++) pixels[i] = pixels[i].gamma;
                image.SetPixels(pixels);
                //image.Apply();


                string directory = Path.Combine(Application.dataPath, "Image Test");
                string path = Path.Combine(directory, "Image1.png");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
        }
    }

    void CheckObjectsInPhoto()
    {
        int count = GameManager.Instance.photographableObjects.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            PhotographableObject photographableObject = GameManager.Instance.photographableObjects[i];

            if (!photographableObject.gameObject.activeSelf) continue;

            Vector3 point = handheldCam.WorldToViewportPoint(photographableObject.transform.position);
            if (PointInView(point) && DistanceCheck(photographableObject.transform.position) && LOSCheck(photographableObject.transform)) photographableObject.CapturedInImage(i);
        }

        //foreach (PhotographableObject photographableObject in GameManager.Instance.photographableObjects)
        //{
        //    Vector3 point = handheldCam.WorldToViewportPoint(photographableObject.transform.position);
        //    if (PointInView(point)) photographableObject.CapturedInImage();
        //}


        bool PointInView(Vector3 point)
        {
            return point.z > 0f && point.x >= 0f && point.x <= 1f && point.y >= 0f && point.y <= 1f;
        }

        bool DistanceCheck(Vector3 point)
        {
            return Vector3.Distance(transform.position, point) < 20f;
        }

        bool LOSCheck(Transform t)
        {
            return Physics.Linecast(transform.position, t.position, out RaycastHit hit) && hit.transform == t;
        }
    }

    Texture2D ToTexture2D(RenderTexture rTex, TextureFormat format = TextureFormat.RGB24)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, format, false);
        RenderTexture oldRT = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = oldRT;
        return tex;
    }
}
