using System.Collections.Generic;
using HietakissaUtils;
using UnityEngine.AI;
using UnityEngine;
using TMPro;

public class WendigoAI : MonoBehaviour
{
    [Header("Position Randomization Distances")]
    [SerializeField] float minWanderRadius = 2f;
    [SerializeField] float maxWanderRadius = 7f;

    [SerializeField] float minApproachRadius = 6f;
    [SerializeField] float maxApproachRadius = 20f;

    [Header("Movement")]
    [SerializeField] float reachedPosThreshold = 0.2f;

    [Header("Sounds")]
    [SerializeField] SoundCollectionSO approachSounds;
    [SerializeField] SoundCollectionSO aggroSounds;
    [SerializeField] SoundCollectionSO eatSounds;
    [SerializeField] SoundCollectionSO attackSounds;

    [Header("Other")]
    [SerializeField] float sightRange = 15f;
    [SerializeField] float maxHeightDifference = 2f;
    [SerializeField] float attackDistance = 1.3f;

    NavMeshQueryFilter navFilter;


    Transform playerTransform;
    NavMeshAgent agent;

    Transform target;
    Vector3 lastSeenPos;
    Vector3 roamTarget;
    float roamingWait;

    State state;
    State lastState;
    float flashed;

    int timesNotGoneToPlayer;


    List<DeerAI> deer = new List<DeerAI>();
    DeerAI seeDeer;

    [SerializeField] TextMeshPro text;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        state = State.Roaming;

        navFilter.agentTypeID = GetComponent<NavMeshAgent>().agentTypeID;
        navFilter.areaMask = NavMesh.AllAreas;
    }

    void Start()
    {
        playerTransform = GameManager.Instance.PlayerTransform;

        roamTarget = PickTarget();
        Debug.Log($"picked point: {roamTarget}");
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlayerAlive || GameManager.Instance.HasWonGame) return;

        if (flashed > 0f)
        {
            state = State.Roaming;
            flashed -= Time.deltaTime;
        }
        else if (target) state = State.Chasing;
        
        // bad enum state machine, but not much time
        switch (state)
        {
            case State.Roaming:
                Debug.DrawLine(transform.position, roamTarget);
                Debug.DrawRay(roamTarget, Vector3.up * 10f, Color.red);

                //if (Vector3.Distance(transform.position, GameManager.Instance.PlayerTransform.position) < 12f && IsPointOnNavmesh(GameManager.Instance.PlayerTransform.position));
                //{
                //    state = State.Chasing;
                //    target = GameManager.Instance.PlayerTransform;
                //}

                if (MoveToPos(roamTarget))
                {
                    roamingWait -= Time.deltaTime;

                    if (roamingWait <= 0f)
                    {
                        roamTarget = PickTarget();
                        Debug.Log($"picked point: {roamTarget}");

                        roamingWait = Random.Range(0.4f, 2f);
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

                //if (target == null)
                //{
                //    // workaround because after eating a deer the wendigo stays chasing for some reason
                //    state = State.Roaming;
                //    return;
                //}

                if (lastState != State.Chasing) EventManager.PlaySoundAtPosition(aggroSounds, transform.position);

                if (target == playerTransform && flashed <= 0f && Vector3.Distance(transform.position, playerTransform.position) < attackDistance)
                {
                    Debug.Log($"reached player?");
                    EventManager.PlaySoundAtPosition(attackSounds, transform.position);
                    EventManager.PlayerDied();
                    //QOL.Quit();
                }
                else if (CanSeeTransform(target))
                {
                    if (MoveToPos(target.position))
                    {
                        target = null;

                        if (seeDeer)
                        {
                            EventManager.Photography.DeerDied(seeDeer);
                            deer.Remove(seeDeer);
                            Destroy(seeDeer.gameObject);

                            EventManager.PlaySoundAtPosition(attackSounds, transform.position);
                            EventManager.PlaySoundAtPosition(eatSounds, transform.position);
                        }
                        state = State.Roaming;
                    }
                }
                else
                {
                    if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 15f, navFilter))
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


            if (CanSeeTransform(currentDeer.transform))
            {
                target = currentDeer.transform;
                state = State.Chasing;
                seeDeer = currentDeer;
                break;
            }
        }

        //if (!target && CanSeePoint(playerTransform.position)) target = playerTransform;
        if (!target && CanSeeTransform(playerTransform)) target = playerTransform;

        string debugString = $"state: {state}\ntarget: {(target ? target.name : "null")}\nsees player: {CanSeeTransform(playerTransform)}\nDistance:{Vector3.Distance(transform.position, GameManager.Instance.PlayerTransform.position).RoundToDecimalPlaces(1)}";
        //Debug.Log(debugString);
        text.text = debugString;

        lastState = state;
    }


    bool CanSeePoint(Vector3 point)
    {
        Vector3 posWithOffset = transform.position + Vector3.up;
        if (Vector3.Distance(posWithOffset, point) > sightRange) return false;
        if (Vector3.Dot(transform.forward, Maf.Direction(transform.position, point)) < 0.5f) return false;

        // linecast to target point doesn't hit anything, or the hit point is within 1 unit of the target point
        if (Physics.Linecast(posWithOffset, point + Vector3.up, out RaycastHit hit))
        {
            return Vector3.Distance(point, hit.point) < 0.3f;
        }
        else return true;
    }

    bool CanSeeTransform(Transform sampleTransform)
    {
        Vector3 posWithOffset = transform.position + Vector3.up;
        if (Vector3.Distance(posWithOffset, sampleTransform.position) > sightRange) return false;
        if (Vector3.Dot(transform.forward, Maf.Direction(transform.position, sampleTransform.position)) < 0.5f) return false;

        // linecast to target point doesn't hit anything, or the hit point is within 1 unit of the target point
        if (Physics.Linecast(posWithOffset, sampleTransform.position + Vector3.up, out RaycastHit hit))
        {
            return Vector3.Distance(sampleTransform.position, hit.point) < 0.3f || hit.transform == sampleTransform;
        }
        else return true;
    }

    //bool IsPointOnNavmesh(Vector3 point) => NavMesh.SamplePosition(point, out NavMeshHit hit, 15f, agent.areaMask);
    bool IsPointOnNavmesh(Vector3 point) => NavMesh.SamplePosition(point, out NavMeshHit hit, 15f, navFilter);

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

        if (toPlayer && timesNotGoneToPlayer >= 7)
        {
            timesNotGoneToPlayer = 0;

            EventManager.PlaySoundAtPosition(approachSounds, transform.position);
            return GetRandomPositionAroundPoint(playerTransform.position, minApproachRadius, maxApproachRadius);
        }
        else
        {
            timesNotGoneToPlayer++;
            return GetRandomPositionAroundPoint(transform.position, minWanderRadius, maxWanderRadius);
        }
    }

    Vector3 GetRandomPositionAroundPoint(Vector3 point, float minRadius, float maxRadius)
    {
        for (int i = 0; i < 50; i++)
        {
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 randomOffset = Maf.GetRandomDirection().SetY(50f) * radius;
            Vector3 raycastPos = point + randomOffset;

            //if (Physics.Raycast(raycastPos, Vector3.down, out RaycastHit hit) && Mathf.Abs(transform.position.y - hit.point.y) <= maxHeightDifference) return hit.point;
            if (Physics.Raycast(raycastPos, Vector3.down, out RaycastHit hit) && NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 10f, navFilter)) return navHit.position;
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


    public void Photographed(ImageParams imageParams)
    {
        Debug.Log($"Wendigo photographed, used flash: {imageParams.UsedFlash}");

        if (imageParams.UsedFlash)
        {
            Vector3 dirAwayFromPlayer = Maf.Direction(GameManager.Instance.PlayerTransform.position, transform.position);
            Vector3 point = GetRandomPositionAroundPoint(dirAwayFromPlayer * Random.Range(15f, 25f), 10f, 20f);
            roamTarget = point;
            roamingWait = 5f;
            flashed = 8f;
        }
    }


    void Photography_OnPhotographDeer(DeerAI deer)
    {
        if (!this.deer.Contains(deer)) this.deer.Add(deer);
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
