
using UnityEngine;


public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 8f;
    [SerializeField] float lifeTime = 4f;
    [SerializeField] LayerMask hitMask;

    float deathTime;
    TrailRenderer trail;
    System.Action<Bullet> release;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    public void Fire(Vector3 position, Vector3 direction)
    {
        enabled = true;
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
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.instance.Lose();
            gameObject.SetActive(false);
            return;
        }

        if((hitMask.value & (1 << other.gameObject.layer)) != 0)
            gameObject.SetActive(false);
    }
}
