using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic Object Pool class để tái sử dụng objects
/// </summary>
public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Queue<T> objects = new Queue<T>();
    private Transform parent;

    public ObjectPool(T prefab, int initialSize = 10, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        // Tạo số lượng object ban đầu
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private T CreateNewObject()
    {
        T newObj = Object.Instantiate(prefab, parent);
        newObj.gameObject.SetActive(false);
        objects.Enqueue(newObj);
        return newObj;
    }

    public T Get()
    {
        // Nếu pool rỗng, tạo object mới
        if (objects.Count == 0)
        {
            CreateNewObject();
        }

        T obj = objects.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        T obj = Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        objects.Enqueue(obj);
    }

    public void Clear()
    {
        while (objects.Count > 0)
        {
            T obj = objects.Dequeue();
            Object.Destroy(obj.gameObject);
        }
    }
}



