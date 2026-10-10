using UnityEngine;

public class GuardAIController : MonoBehaviour
{

    public Transform[] waypoints;
    public Animator animator;
    public LayerMask wallMask;
    public float speed = 2f;
    public float turnSpeed = -180f;
    public float wallCheckDistance = 1.5f;

    int index;
    Rigidbody rb;
    static readonly int speedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 dir = waypoints[index].position - transform.position;
        dir.y = 0;

        if(dir.magnitude < 0.5f ) 
        {
            index = (index + 1) % waypoints.Length;
            return;
        }

        dir.Normalize();

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, transform.forward, wallCheckDistance, wallMask))
            dir = transform.right;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.fixedDeltaTime);

        Vector3 vect = transform.forward * speed;
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
}
