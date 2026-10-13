using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Camera cam;
    public Animator animator;
    public float speed = 5f;
    public float turnSpeed = 720f;
    public float aimDeadZone = 0.5f;

    Rigidbody rb;
    Vector3 moveInput;
    bool moving;
    Vector3 aimPoint;

    static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if(cam == null)
            cam = GetComponent<Camera>();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if(kb == null || mouse == null)
        {
            moving = false;
            return;
        }

        moving = kb.wKey.isPressed || kb.upArrowKey.isPressed;

        Plane floor = new Plane(Vector3.up, transform.position);
        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());

        if(floor.Raycast(ray, out float distance))
            aimPoint = ray.GetPoint(distance);
    
        animator.SetFloat(SpeedHash, moving ? 1f: 0f, 0.1f, Time.deltaTime);    
    }

    private void FixedUpdate()
    {
        Vector3 dir = aimPoint - transform.position;
        dir.y = 0f;

        if(dir.sqrMagnitude > aimDeadZone * aimDeadZone)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, turnSpeed * Time.fixedDeltaTime));
        }

        Vector3 v = moving ? transform.forward * speed : Vector3.zero;
        rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
    }
}
