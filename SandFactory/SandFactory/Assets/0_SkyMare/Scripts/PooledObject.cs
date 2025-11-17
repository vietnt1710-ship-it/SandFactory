using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooled : MonoBehaviour
{
    public string poolName;
}
///// <summary>
///// Component helper để tự động return về pool
///// </summary>
//public class PooledObject : Pooled
//{
   
//    private float lifetime = 0f;

//    public void SetLifetime(float time)
//    {
//        lifetime = time;
//        if (lifetime > 0)
//        {
//            Invoke(nameof(ReturnToPool), lifetime);
//        }
//    }

//    public void ReturnToPool()
//    {
//        CancelInvoke();
//        ObjectPoolManager.I.Despawn(poolName, gameObject);
//    }

//    private void OnDisable()
//    {
//        CancelInvoke();
//    }
//}

///// <summary>
///// Ví dụ sử dụng - Script để spawn bullets
///// </summary>
//public class BulletSpawner : Pooled
//{
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private float fireRate = 0.5f;
//    private float nextFireTime = 0f;

//    private void Start()
//    {
//        // Tạo pool cho bullets
//        ObjectPoolManager.I.CreatePool("Bullets", bulletPrefab, 20);
//    }

//    private void Update()
//    {
//        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
//        {
//            Fire();
//            nextFireTime = Time.time + fireRate;
//        }
//    }

//    private void Fire()
//    {
//        GameObject bullet = ObjectPoolManager.I.Spawn("Bullets", transform.position, transform.rotation);

//        // Set lifetime cho bullet
//        PooledObject pooledObj = bullet.GetComponent<PooledObject>();
//        if (pooledObj != null)
//        {
//            pooledObj.poolName = "Bullets";
//            pooledObj.SetLifetime(3f); // Tự động return sau 3 giây
//        }
//    }
//}