using HietakissaUtils;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] FaceTowards faceTowards = FaceTowards.Forward;

    void Update()
    {
        //transform.LookAt(GameManager.Instance.PlayerCameraTransform.position.SetY(transform.position.y));

        Vector3 forward = -GameManager.Instance.PlayerCameraTransform.forward.SetY(0f);
        if (forward == Vector3.zero)
        {
            forward = Maf.Direction(transform.position, GameManager.Instance.PlayerTransform.position).SetY(0f);
            if (forward == Vector3.zero) forward = transform.forward;
        }

        if (faceTowards == FaceTowards.Forward) transform.forward = forward;
        else transform.forward = -forward;
    }
}

enum FaceTowards
{
    Forward,
    Back
}
