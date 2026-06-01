using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic MonoBehaviour object pool.
/// Use for frequently spawned objects: VFX particles, ambient SFX emitters,
/// projectiles. Avoids GC allocation from repeated Instantiate/Destroy.
///
/// Usage:
///   var pool = ObjectPool.GetPool(prefab, initialSize: 10);
///   var obj  = pool.Get(position, rotation);
///   pool.Return(obj);
/// </summary>
public class ObjectPool : MonoBehaviour
{
    private static readonly Dictionary<GameObject, ObjectPool> _pools
        = new Dictionary<GameObject, ObjectPool>();

    private GameObject _prefab;
    private readonly Queue<GameObject> _available = new Queue<GameObject>();

    public static ObjectPool GetPool(GameObject prefab, int initialSize = 5)
    {
        if (_pools.TryGetValue(prefab, out var existing)) return existing;

        var go   = new GameObject($"Pool_{prefab.name}");
        var pool = go.AddComponent<ObjectPool>();
        pool._prefab = prefab;
        DontDestroyOnLoad(go);

        for (int i = 0; i < initialSize; i++)
            pool.CreateAndReturn();

        _pools[prefab] = pool;
        return pool;
    }

    public GameObject Get(Vector3 position = default, Quaternion rotation = default)
    {
        var obj = _available.Count > 0 ? _available.Dequeue() : Instantiate(_prefab);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _available.Enqueue(obj);
    }

    private void CreateAndReturn()
    {
        var obj = Instantiate(_prefab);
        obj.SetActive(false);
        _available.Enqueue(obj);
    }
}
