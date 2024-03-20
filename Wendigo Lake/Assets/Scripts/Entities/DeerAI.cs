using HietakissaUtils;
using UnityEngine.AI;
using UnityEngine;

public class DeerAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Bounds bounds;

    public void Photographed()
    {
        Vector3 point = GetRandomPos();
        agent.SetDestination(point);

        EventManager.Photography.PhotographDeer(this);
    }

    Vector3 GetRandomPos()
    {
        Transform cameraTransform = GameManager.Instance.PlayerCameraTransform;

        Vector3 point = transform.position;
        for (int i = 0; i < 100; i++)
        {
            point = Maf.RandomPointInBounds(bounds).SetY(bounds.center.y + bounds.size.y * 0.5f);
            Debug.DrawRay(point, Vector3.up, Color.green, 5f);

            if (Physics.Raycast(point, Vector3.down, out RaycastHit hit, 50f))
            {
                point = hit.point;
            }

            Debug.DrawLine(point + Vector3.up * 0.2f, cameraTransform.position, Color.red, 5f);
            if (Physics.Linecast(point + Vector3.up * 0.2f, cameraTransform.position, out hit) && Vector3.Distance(hit.point, cameraTransform.position) < 5f)
            {
                Debug.DrawRay(hit.point, Vector3.up, Color.yellow, 5f);
                continue;
            }
            else return point;
        }

        return point;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
