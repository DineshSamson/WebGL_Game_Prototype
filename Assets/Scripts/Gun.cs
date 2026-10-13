using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    [SerializeField] Bullet[] bullets;
    [SerializeField] Transform firePoint;
    public float fireInterval = 2f;

    ObjectPool<Bullet> pool;
    float nextFireTime;

    [SerializeField] Transform player;
    [SerializeField] float aimHeight = 1;
    [SerializeField] float turnSpeed = 360f;
    [SerializeField] float lockTime = 0.4f;
    [SerializeField] float range = 15f;

    private void Start()
    {
        for(int i = 0; i < bullets.Length; i++) 
        {
            bullets[i].enabled = false;
        }
    }

    private void Update()
    {
        float t = Time.time;

        Vector3 to = player.position + Vector3.up * aimHeight - transform.position;
        if(to.sqrMagnitude > range * range)
        {
            return;
        }
        
        if(t < nextFireTime - lockTime && to.sqrMagnitude > 0.001f)
        {
            Quaternion want = Quaternion.LookRotation(to);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime);
        }

        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + fireInterval;
        Shoot();
    }

    void Shoot()
    {
        for(int i =0;  i < bullets.Length; i++)
        {
            if (!bullets[i].gameObject.activeSelf) 
            {
                bullets[i].Fire(firePoint.position, firePoint.forward);
                return;
            }
        }
    }
}
