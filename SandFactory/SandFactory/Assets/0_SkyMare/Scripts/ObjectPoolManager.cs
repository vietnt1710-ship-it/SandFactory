/// <summary>
/// Object Pool Manager - Singleton để quản lý nhiều pools
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [System.Serializable]
    public class PoolInfo
    {
        public string poolName;
        public GameObject prefab;
        public int initialSize = 10;
    }

    [SerializeField] private List<PoolInfo> pools = new List<PoolInfo>();
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (PoolInfo poolInfo in pools)
        {
            CreatePool(poolInfo.poolName, poolInfo.prefab, poolInfo.initialSize);
        }
    }

    public void CreatePool(string poolName, GameObject prefab, int size)
    {
        if (poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool {poolName} đã tồn tại!");
            return;
        }

        Queue<GameObject> objectPool = new Queue<GameObject>();
        Transform poolParent = new GameObject($"Pool_{poolName}").transform;
        poolParent.SetParent(transform);

        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent);
            Pooled pd = obj.GetComponent<Pooled>();
            if(pd != null) pd.poolName = poolName;
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(poolName, objectPool);
    }

    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool {poolName} không tồn tại!");
            return null;
        }

        GameObject obj;
        if (poolDictionary[poolName].Count == 0)
        {
            // Tạo object mới nếu pool rỗng
            PoolInfo poolInfo = pools.Find(p => p.poolName == poolName);
            obj = Instantiate(poolInfo.prefab, transform.Find($"Pool_{poolName}"));
            Pooled pd = obj.GetComponent<Pooled>();
            if (pd != null) pd.poolName = poolName;
        }
        else
        {
            obj = poolDictionary[poolName].Dequeue();
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }
    public GameObject Spawn(string poolName, Vector3 position)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool {poolName} không tồn tại!");
            return null;
        }

        GameObject obj;
        if (poolDictionary[poolName].Count == 0)
        {
            // Tạo object mới nếu pool rỗng
            PoolInfo poolInfo = pools.Find(p => p.poolName == poolName);
            obj = Instantiate(poolInfo.prefab, transform.Find($"Pool_{poolName}"));
            Pooled pd = obj.GetComponent<Pooled>();
            if (pd != null) pd.poolName = poolName;
        }
        else
        {
            obj = poolDictionary[poolName].Dequeue();
        }

        obj.transform.position = position;
        obj.SetActive(true);

        return obj;
    }

    //public GameObject Spawn(string poolName)
    //{
    //    return Spawn(poolName, Vector3.zero, Quaternion.identity);
    //}

    public void Despawn(string poolName, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool {poolName} không tồn tại!");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDictionary[poolName].Enqueue(obj);
    }

    public void ClearPool(string poolName)
    {
        if (!poolDictionary.ContainsKey(poolName)) return;

        while (poolDictionary[poolName].Count > 0)
        {
            GameObject obj = poolDictionary[poolName].Dequeue();
            Destroy(obj);
        }
        poolDictionary.Remove(poolName);

        Transform poolParent = transform.Find($"Pool_{poolName}");
        if (poolParent != null)
        {
            Destroy(poolParent.gameObject);
        }
    }
}