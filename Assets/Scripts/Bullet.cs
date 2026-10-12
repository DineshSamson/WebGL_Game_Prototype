
using UnityEngine;


public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 8f;
    [SerializeField] float lifeTime = 4f;

    float deathTime;
    TrailRenderer trail;
    System.Action<Bullet> release;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    public void Init(System.Action<Bullet> releaseCallback)
    {
        release = releaseCallback;
    }

    public void Fire(Vector3 position, Vector3 direction)
    {
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
        gameObject.SetActive(true);
        if(trail)
            trail.Clear();
        deathTime = Time.time + lifeTime;
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        if (Time.time >= deathTime)
            release(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.instance.Lose();
        }
        release(this);
    }
}
