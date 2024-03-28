using HietakissaUtils;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] Material _material;
    [SerializeField] MeshRenderer meshRend;
    [SerializeField] TextureAndDirection[] directions;

    Material mat;

    [SerializeField] float animationSpeed;
    int currentAnimationIndex;
    float animationTime;

    void Awake()
    {
        mat = new Material(_material);
        mat.SetColor("_EmissionColor", Color.white);
        meshRend.material = mat;
    }

    void Update()
    {
        Vector3 viewVector = -Maf.Direction(GameManager.Instance.PlayerTransform.position, transform.position).SetY(0f);
        viewVector = transform.InverseTransformDirection(viewVector);
        
        int closestIndex = 0;
        float closestProximity = 0f;

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 direction = directions[i].Direction;
            float proximity = Vector3.Dot(direction, viewVector);

            if (proximity > closestProximity)
            {
                closestProximity = proximity;
                closestIndex = i;
            }
        }

        animationTime += animationSpeed * Time.deltaTime;
        if (animationTime >= 1f)
        {
            animationTime -= 1f;

            //currentAnimationIndex++;
            currentAnimationIndex = (currentAnimationIndex + 1) % 2;
        }

        mat.mainTexture = directions[closestIndex].Texture[currentAnimationIndex];
        mat.SetTexture("_EmissionMap", directions[closestIndex].Texture[currentAnimationIndex]);
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < 8; i++)
        {
            Gizmos.DrawRay(transform.position + Vector3.up * i * 0.5f, transform.parent.forward);
        }
        //Gizmos.DrawRay(transform.position, transform.parent.forward);
        //Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.parent.forward);
        //Gizmos.DrawRay(transform.position + Vector3.up, transform.parent.forward);
    }
}

[System.Serializable]
struct TextureAndDirection
{
    [SerializeField] string name;
    [field: SerializeField] public Texture2D[] Texture;
    [field: SerializeField] public Vector3 Direction;
}
