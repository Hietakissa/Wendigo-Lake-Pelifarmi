using System.Collections.Generic;
using HietakissaUtils;
using UnityEngine.AI;
using UnityEngine;
using TMPro;
using HietakissaUtils.QOL;

public class WendigoAI : MonoBehaviour
{
    [Header("Position Randomization Distances")]
    [SerializeField] float minWanderRadius = 2f;
    [SerializeField] float maxWanderRadius = 7f;

    [SerializeField] float minApproachRadius = 6f;
    [SerializeField] float maxApproachRadius = 20f;

    [Header("Movement")]
    [SerializeField] float reachedPosThreshold = 0.2f;

    [Header("Other")]
    [SerializeField] float sightRange = 15f;
    [SerializeField] float maxHeightDifference = 2f;
    [SerializeField] float attackDistance = 1.3f;


    Transform playerTransform;
    NavMeshAgent agent;

    Transform target;
    Vector3 lastSeenPos;
    Vector3 roamTarget;
    float roamingWait;

    State state;

    List<DeerAI> deer = new List<DeerAI>();
    DeerAI seeDeer;

    [SerializeField] TextMeshPro text;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        state = State.Roaming;
    }

    void Start()
    {
        playerTransform = GameManager.Instance.PlayerTransform;

        roamTarget = PickTarget();
        Debug.Log($"picked point: {roamTarget}");
    }

    void Update()
    {
        if (target) state = State.Chasing;

        // bad enum state machine, but not much time
        switch (state)
        {
            case State.Roaming:
                Debug.DrawLine(transform.position, roamTarget);
                Debug.DrawRay(roamTarget, Vector3.up * 10f, Color.red);

                if (MoveToPos(roamTarget))
                {
                    roamingWait -= Time.deltaTime;

                    if (roamingWait <= 0f)
                    {
                        roamTarget = PickTarget();
                        Debug.Log($"picked point: {roamTarget}");

                        roamingWait = Random.Range(1f, 3f);
                    }
                }
            break;

            case State.Investigating:
                if (MoveToPos(lastSeenPos))
                {
                    // reached investigation area
                    Debug.Log($"Reached lastSeenPos, switching to roaming");
                    state = State.Roaming;
                }
            break;

            case State.Chasing:

                if (target == playerTransform && Vector3.Distance(transform.position, playerTransform.position) < attackDistance)
                {
                    Debug.Log($"reached player?");
                    QOL.Quit();
                }
                else if (CanSeePoint(target.position))
                {
                    if (MoveToPos(target.position))
                    {
                        target = null;

                        if (seeDeer)
                        {
                            Destroy(seeDeer.gameObject);
                            state = State.Roaming;
                        }
                    }
                }
                else
                {
                    if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    {
                        //lastSeenPos = target.position;
                        lastSeenPos = hit.position;
                        state = State.Investigating;
                        target = null;
                    }
                    else
                    {
                        state = State.Roaming;
                    }
                }
            break;
        }


        for (int i = deer.Count - 1; i >= 0; i--)
        {
            DeerAI currentDeer = deer[i];

            if (!currentDeer)
            {
                deer.RemoveAt(i);
                continue;
            }


            if (CanSeePoint(currentDeer.transform.position))
            {
                target = currentDeer.transform;
                state = State.Chasing;
                seeDeer = currentDeer;
                break;
            }
        }

        if (!target && CanSeePoint(playerTransform.position)) target = playerTransform;

        string debugString = $"state: {state}\ntarget: {(target ? target.name : "null")}\nsees player: {CanSeePoint(playerTransform.position)}";
        //Debug.Log(debugString);
        text.text = debugString;
    }


    bool CanSeePoint(Vector3 point)
    {
        Vector3 posWithOffset = transform.position + Vector3.up;
        if (Vector3.Distance(posWithOffset, point) > sightRange) return false;

        // linecast to target point doesn't hit anything, or the hit point is within 1 unit of the target point
        if (Physics.Linecast(posWithOffset, point + Vector3.up, out RaycastHit hit))
        {
            return Vector3.Distance(point, hit.point) < 2f;
        }
        else return true;

        //return !Physics.Linecast(transform.position + Vector3.up, point + Vector3.up, out RaycastHit hit) || (hit.collider && Vector3.Distance(point, hit.point) < 1f);
    }

    bool MoveToPos(Vector3 pos)
    {
        if (Vector3.Distance(transform.position, pos) < reachedPosThreshold) return true;

        agent.destination = pos;
        return false;
    }

    Vector3 PickTarget()
    {
        bool toPlayer = Maf.RandomBool(20);
        Debug.Log($"target is to player: {toPlayer}");

        if (toPlayer) return GetRandomPositionAroundPoint(playerTransform.position, minApproachRadius, maxApproachRadius);
        else return GetRandomPositionAroundPoint(transform.position, minWanderRadius, maxWanderRadius);

        Vector3 GetRandomPositionAroundPoint(Vector3 point, float minRadius, float maxRadius)
        {
            for (int i = 0; i < 50; i++)
            {
                float radius = Random.Range(minRadius, maxRadius);
                Vector3 randomOffset = Maf.GetRandomDirection().SetY(50f) * radius;
                Vector3 raycastPos = point + randomOffset;

                //if (Physics.Raycast(raycastPos, Vector3.down, out RaycastHit hit) && Mathf.Abs(transform.position.y - hit.point.y) <= maxHeightDifference) return hit.point;
                if (Physics.Raycast(raycastPos, Vector3.down, out RaycastHit hit) && NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas)) return navHit.position;
            }

            //if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            //{
            //    //lastSeenPos = target.position;
            //    lastSeenPos = hit.position;
            //    state = State.Investigating;
            //    target = null;
            //}

            Debug.Log($"GetRandomPositionAroundPoint exit early!");
            return point;
        }
    }


    void Photography_OnPhotographDeer(DeerAI deer)
    {
        this.deer.Add(deer);
    }


    void OnEnable()
    {
        EventManager.Photography.OnPhotographDeer += Photography_OnPhotographDeer;
    }

    void OnDisable()
    {
        EventManager.Photography.OnPhotographDeer -= Photography_OnPhotographDeer;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastSeenPos, 0.4f);
    }
}

enum State
{
    Roaming,
    Investigating,
    Chasing
}
