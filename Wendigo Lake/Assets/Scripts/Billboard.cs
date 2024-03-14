using HietakissaUtils;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        //transform.LookAt(GameManager.Instance.PlayerCameraTransform.position.SetY(transform.position.y));
        Vector3 forward = -GameManager.Instance.PlayerCameraTransform.forward.SetY(0f);
        if (forward == Vector3.zero)
        {
            forward = Maf.Direction(transform.position, GameManager.Instance.PlayerTransform.position).SetY(0f);
            if (forward == Vector3.zero) forward = transform.forward;
        }
        transform.forward = forward;
    }
}
