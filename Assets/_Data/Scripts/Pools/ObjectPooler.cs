using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class ObjectPoolItem
{
    public GameObject objectToPool;
    public string poolName;
    public int amountToPool;
    public bool shouldExpand = true;
    public bool inEditor = true;
}

public class ObjectPooler : Singleton<ObjectPooler>
{
    public const string DefaultRootObjectPoolName = "Pooled Objects";

    public string rootPoolName = DefaultRootObjectPoolName;
    public List<PoolBehaviour> pooledObjects = new List<PoolBehaviour>();
    private List<ObjectPoolItem> itemsToPool;
    private PoolerScriptable PoolSettings;

    private void Awake()
    {
        LoadFromScriptable();
    }

    private void LoadFromScriptable()
    {
        PoolSettings = Resources.Load("Scriptable/PoolSettings") as PoolerScriptable;
        itemsToPool = PoolSettings.itemsToPool;
    }

    private void Start()
    {
        if (!Application.isPlaying) return;
        ClearNullElements();

        foreach (var item in itemsToPool)
        {
            if (item == null) continue;
            if (item.objectToPool == null) continue;
            var pooledCount = pooledObjects.Count(i => i.name == item.objectToPool.name);
            for (int i = 0; i < item.amountToPool - pooledCount; i++)
            {
                CreatePooledObject(item);
            }
        }
    }

    private void ClearNullElements()
    {
        pooledObjects.RemoveAll(i => i == null);
    }

    private GameObject GetParentPoolObject(string objectPoolName)
    {
   
        GameObject parentObject = GameObject.Find(objectPoolName);
        if (parentObject == null)
        {
            parentObject = new GameObject();
            parentObject.name = objectPoolName;

            if (objectPoolName != rootPoolName)
                parentObject.transform.parent = transform;
        }

        return parentObject;
    }

    public void HideObjects(string tag)
    {
        var objects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<PoolBehaviour>().Where(i => i.name == tag);

        foreach (var item in objects)
            item.gameObject.SetActive(false);
    }

    public void PutBack(GameObject obj)
    {
        obj.SetActive(false);
    }

    public GameObject GetPooledObject(string tag, Object activatedBy = null, bool active = true, bool canBeActive = false)
    {
        ClearNullElements();

        PoolBehaviour obj = null;
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (pooledObjects[i] == null) continue;
            if ((!pooledObjects[i].gameObject.activeSelf || canBeActive) && pooledObjects[i].name == tag)
            {
                obj = pooledObjects[i];
                
                if (obj) break;
            }
        }

        if (itemsToPool == null) LoadFromScriptable();
        if (!obj)
        {
            foreach (var item in itemsToPool)
            {
                if (item != null && item.objectToPool == null) continue;
                if (item.objectToPool.name == tag)
                {
                    if (item.shouldExpand)
                    {
                        obj = CreatePooledObject(item);
                        break;
                    }
                }
            }
        }
       
        if (obj != null)
        {
            obj.gameObject.SetActive(active);
            return obj.gameObject;
        }

        return null;
    }

    private PoolBehaviour CreatePooledObject(ObjectPoolItem item)
    {
        // Debug.Log("Create item");

        GameObject obj = Instantiate(item.objectToPool);
        var parentPoolObject = GetParentPoolObject(item.poolName);
        obj.transform.parent = parentPoolObject.transform;
        var poolBehaviour = obj.AddComponent<PoolBehaviour>();
        poolBehaviour.name = item.objectToPool.name;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            PrefabUtility.RevertPrefabInstance(obj, InteractionMode.AutomatedAction);
        }
#endif

        obj.SetActive(false);
        pooledObjects.Add(poolBehaviour);


        return poolBehaviour;
    }

    public void DestroyObjects(string tag)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (pooledObjects[i].name == tag)
            {
                DestroyImmediate(pooledObjects[i]);
            }
        }
        ClearNullElements();
    }
}

