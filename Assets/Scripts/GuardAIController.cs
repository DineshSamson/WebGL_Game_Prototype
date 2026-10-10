using Unity.VisualScripting;
using UnityEngine;

public class GuardAIController : MonoBehaviour
{
    public enum GuardState { Patrol, Suspicious, Chase}

    public Transform[] waypoints;
    public Animator animator;
    public Transform player;
    public VisionCone visionCone;

    public LayerMask wallMask;
    public float viewRange = 8f;
    public float viewAngle = 90f;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float turnSpeed = -180f;
    public float wallCheckDistance = 1.5f;

    public float catchDistance = 1f;
    public float loseSightTime = 3f;
    public float searchTime = 3f;


    GuardState state = GuardState.Patrol;
    int index;
    Rigidbody rb;
    float cosHalfAngle;
    Vector3 lastSeenPos;
    float loseTimer;
    float searchTimer;
    bool caught;

    static readonly int speedHash = Animator.StringToHash("Speed");

    static readonly Color patrolColor = new Color(1f, 1f, 0f, 0.35f);
    static readonly Color suspiciousColor = new Color(1f, 0.5f, 0f, 0.4f);
    static readonly Color chaseColor = new Color(1f, 0f, 0f, 0.45f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        cosHalfAngle = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);
    }

    private void Start()
    {
        ChangeState(GuardState.Patrol);
    }

    void FixedUpdate()
    {
        if (caught)
        {
            Stop();
            return;
        }

        switch (state) 
        {
            case GuardState.Patrol:
                Patrol();
                break;

            case GuardState.Suspicious:
                DoSuspicious();
                break;
            case GuardState.Chase:
                Chase();
                break;
        }

        //visionCone.SetColor(cansee)
    }

    void Patrol()
    {
        if(CanSeePlayer())
        {
            lastSeenPos = player.position;
            ChangeState(GuardState.Chase);
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Stop(); return;
        }

        Vector3 toWaypoin = waypoints[index].position - transform.position;
        toWaypoin.y = 0;

        if(toWaypoin.magnitude < 0.5f)
        {
            index = (index + 1) % waypoints.Length;
            return;
        }

        MoveTowards(waypoints[index].position, patrolSpeed);
    }

    void Chase()
    {
        if(CanSeePlayer())
        {
            loseTimer = 0;
            lastSeenPos = player.position;
        }
        else
        {
            loseTimer += Time.fixedDeltaTime;
            if(loseTimer >= loseSightTime)
            {
                ChangeState(GuardState.Suspicious);
                return;
            }
        }

        MoveTowards(lastSeenPos, chaseSpeed);

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        if(toPlayer.magnitude <= catchDistance)
        {
            caught = true;
            Debug.Log("Player Caught!");
        }
    }

    void DoSuspicious()
    {
        if(CanSeePlayer())
        {
            lastSeenPos = player.position;
            ChangeState(GuardState.Chase);
            return;
        }

        Vector3 toSpot = lastSeenPos - transform.position;
        toSpot.y = 0;

        if(toSpot.magnitude > 0.5f)
        {
            MoveTowards(lastSeenPos, patrolSpeed);
        }
        else
        {
            Stop();
            searchTimer += Time.fixedDeltaTime;
            if(searchTimer >= searchTime)
                ChangeState(GuardState.Patrol);
        }
    }

    void ChangeState(GuardState newState)
    {
        state = newState;
        loseTimer = 0f;
        searchTimer = 0;

        if (visionCone == null)
            return;

         switch(newState)
        {
            case GuardState.Patrol:
                visionCone.SetColor(patrolColor); break;
            case GuardState.Suspicious:
                visionCone.SetColor(suspiciousColor); break;
            case GuardState.Chase:
                visionCone.SetColor(chaseColor); break; 
        }
    }

    public void Investigate(Vector3 position)
    {
        if (state == GuardState.Chase)
            return;
        lastSeenPos = position;
        ChangeState(GuardState.Suspicious);
    }

    bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;

        if (toPlayer.sqrMagnitude > viewRange * viewRange)
            return false;

        if (Vector3.Dot(transform.forward, toPlayer.normalized) < cosHalfAngle)
            return false;

        Vector3 eye = transform.position + Vector3.up;
        Vector3 target = player.position + Vector3.up;

        if (Physics.Linecast(eye, target, wallMask))
            return false;

        return true;
    }

    public void MoveTowards(Vector3 target, float Speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir.magnitude < 0.5f)
        {
            index = (index + 1) % waypoints.Length;
            return;
        }
        dir.Normalize();

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, transform.forward, wallCheckDistance, wallMask))
            dir = transform.right;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.fixedDeltaTime);

        Vector3 vect = transform.forward * Speed;
        rb.linearVelocity = new Vector3(vect.x, rb.linearVelocity.y, vect.z);
        animator.SetFloat(speedHash, 1f);
    }



    private void OnDrawGizmosSelected()
    {
        if (waypoints == null)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                continue;
            }

            Transform next = waypoints[(i + 1) % waypoints.Length];
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            if (next != null)
            {
                Gizmos.DrawLine(waypoints[i].position, next.position);
            }
        }
    }

    void Stop()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        animator.SetFloat(speedHash, 0f);
    }

   
}
