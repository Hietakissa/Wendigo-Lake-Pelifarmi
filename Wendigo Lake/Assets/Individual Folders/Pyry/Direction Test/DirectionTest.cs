using HietakissaUtils;
using UnityEngine;

public class DirectionTest : MonoBehaviour
{
    [SerializeField] Material _material;
    [SerializeField] MeshRenderer meshRend;
    [SerializeField] Texture2D[] textures;
    Vector3[] directions = new Vector3[4];

    Material mat;

    void Awake()
    {
        mat = new Material(_material);
        mat.SetColor("_EmissionColor", Color.white);
        meshRend.material = mat;

        //directions[0] = (Vector3.forward + Vector3.left).normalized;  // front left
        directions[0] = Vector3.forward;                              // front
        //directions[2] = (Vector3.forward + Vector3.right).normalized; // front right
        directions[1] = Vector3.left;                                 // left
        directions[2] = Vector3.right;                                // right
        //directions[5] = (Vector3.back + Vector3.left).normalized;     // back left
        directions[3] = Vector3.back;                                 // back
        //directions[7] = (Vector3.back + Vector3.right).normalized;    // back right
    }

    float seconds;
    void Update()
    {
        //seconds += Time.deltaTime;
        //seconds %= textures.Length;
        //
        //mat.mainTexture = textures[seconds.RoundDown()];

        //Vector3 viewVector = -GameManager.Instance.PlayerCameraTransform.forward.SetY(0f);
        Vector3 viewVector = -Maf.Direction(GameManager.Instance.PlayerTransform.position, transform.position).SetY(0f);
        int closestIndex = 0;
        float closestProximity = 0f;

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 direction = directions[i];
            float proximity = Vector3.Dot(direction, viewVector);

            if (proximity > closestProximity)
            {
                closestProximity = proximity;
                closestIndex = i;
            }
        }

        mat.mainTexture = textures[closestIndex];
        mat.SetTexture("_EmissionMap", textures[closestIndex]);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.parent.forward);
    }
}
