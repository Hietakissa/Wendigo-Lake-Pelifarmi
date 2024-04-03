using HietakissaUtils;
using UnityEngine.AI;
using UnityEngine;

public class DeerAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Bounds bounds;

    [SerializeField] SpriteAnimator animator;

    Vector3 targetPos;
    float wait;


    void Awake()
    {
        SetRandomPosition();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, targetPos) < 1.2f)
        {
            animator.enabled = false;
            wait -= Time.deltaTime;
            if (wait <= 0f)
            {
                targetPos = GetRandomPositionAroundPoint(transform.position, 10f, 25f);
                wait = Random.Range(0.5f, 3f);
            }
        }
        else
        {
            agent.SetDestination(targetPos);
            animator.enabled = true;
        }
    }

    public void SetRandomPosition()
    {
        const float EXTENTS = 100f;
        Vector3 point = new Vector3(Random.Range(-EXTENTS, EXTENTS), 100f, Random.Range(-EXTENTS, EXTENTS));
        Vector3 position = GetRandomPositionAroundPoint(point, 20f, 80f);
        transform.position = position;
        targetPos = position;
        wait = Random.Range(3f, 7f);
    }


    public void Photographed()
    {
        wait = 5f;
        targetPos = GetRandomPositionAroundPoint(transform.position, 12f, 30f);

        EventManager.Photography.PhotographDeer(this);
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
                /*if (LOSBetweenPoints(point + Vector3.up * 0.2f, navHit.position + Vector3.up * 0.2f)) continue;
                else */return navHit.position;
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
