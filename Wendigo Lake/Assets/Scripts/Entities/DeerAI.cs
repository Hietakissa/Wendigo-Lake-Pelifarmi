using HietakissaUtils;
using UnityEngine.AI;
using UnityEngine;

public class DeerAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Bounds bounds;

    public void Photographed()
    {
        //Vector3 point = GetRandomPos();
        Debug.Log($"deer photo");

        Vector3 point = GetRandomPositionAroundPoint(transform.position, 12f, 30f);
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

    Vector3 GetRandomPositionAroundPoint(Vector3 point, float minRadius, float maxRadius)
    {
        for (int i = 0; i < 50; i++)
        {
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 randomOffset = (Maf.GetRandomDirection() * radius).SetY(50f);
            Vector3 raycastPos = point + randomOffset;

            //if (LOSBetweenPoints(point + Vector3.up * 0.2f, GameManager.Instance.PlayerTransform.position + Vector3.up * 0.2f)) continue;

            if (Physics.Raycast(raycastPos, Vector3.down, out RaycastHit hit) && NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                if (LOSBetweenPoints(point + Vector3.up * 0.2f, navHit.position + Vector3.up * 0.2f)) continue;
                else return navHit.position;
            }
            else continue;
        }

        Debug.Log($"Deer GetRandomPositionAroundPoint exit early!");
        return point;
    }

    bool LOSBetweenPoints(Vector3 pointA, Vector3 pointB)
    {
        if (!Physics.Linecast(pointA, pointB, out RaycastHit hit) || Vector3.Distance(hit.point, pointB) < 1.3f) return true;
        else return false;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
