using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ChallengeKit
{
    public class UnityObjectPool
    {
        private string name;
        private UnityEngine.Object basePrefab;
        private Stack<UnityEngine.Object> available;

        // todo : 리소스 메니져는 나중에 ini나 다른 파일과 연동하여 해당 ObjectPool 의 baseSize를 조절 할 수 있도록 하겠다.
        private int baseSize = 10;
        private int currentSize = 0;

        // 오브젝트 풀은 현재 GameObject 대상으로만 지원한다.
        private bool bSupportObjectPool = false;
        private ResourceManager resourceManager;
        private Transform parent;

        public UnityObjectPool(string name, UnityEngine.Object basePrefab, ResourceManager resourceManager, Transform parent)
        {
            this.basePrefab = basePrefab;
            this.name = name;
            this.resourceManager = resourceManager;
            this.parent = parent;

            available = new Stack<UnityEngine.Object>();

            bSupportObjectPool = basePrefab is GameObject;

            SpawnUptoBaseSize();
        }

        private void SpawnUptoBaseSize()
        {
            if (basePrefab == null)
            {
                Debug.LogWarning("basePrefab was not Loaded, name : " + name);
                return;
            }

            if (bSupportObjectPool == false)
            {
                currentSize = 1;
                return;
            }

            for (int i = 0; i < baseSize; i++)
            {
                UnityEngine.Object spawned = resourceManager.Spawn(basePrefab);
                spawned.name = name;
                if (spawned is GameObject)
                {
                    GameObject current = (GameObject)spawned;
                    current.SetActive(false);

                    if(parent == null)
                    {
                        current.transform.SetParent(resourceManager.transform);
                    }
                    else
                    {
                        current.transform.SetParent(parent);
                    }
                    
                }
                available.Push(spawned);
            }
            currentSize = baseSize;
        }

        public UnityEngine.Object GetAvailableObject()
        {
            if (basePrefab == null)
            {
                Debug.LogWarning("basePrefab was not Loaded, name : " + name);
                return null;
            }

            if (bSupportObjectPool == false)
                return basePrefab;

            UnityEngine.Object result = null;

            if (available.Count > 0)
            {
                result = available.Pop();
            }
            else
            {
                int baseSize = Mathf.Max(1, currentSize);
                IncreaseSizeBy(baseSize / 2);
                result = available.Pop();
            }

            if (result is GameObject)
            {
                ( (GameObject)result ).SetActive(true);
            }

            return result;
        }

        private void IncreaseSizeBy(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                UnityEngine.Object spawned = resourceManager.Spawn(basePrefab);
                spawned.name = name;
                if (spawned is GameObject)
                {
                    GameObject current = (GameObject)spawned;
                    current.SetActive(false);

                    if (parent == null)
                    {
                        current.transform.SetParent(resourceManager.transform);
                    }
                    else
                    {
                        current.transform.SetParent(parent);
                    }
                }

                available.Push(spawned);
            }
            currentSize += amount;
        }

        public Define.Result CollectGameObject(GameObject collected)
        {
            collected.SetActive(false);
            collected.transform.SetParent(resourceManager.transform);
            available.Push(collected);

            return Define.Result.OK;
        }

        public override string ToString()
        {
            int used = currentSize - available.Count;
            return string.Format("[{0} : ({1}/{2})]", name, used, currentSize);
        }
    }

    public class UnityObjectCollection
    {
        private string category;
        private Dictionary<string, UnityObjectPool> pools;
        private ResourceManager resourceManager;

        public UnityObjectCollection(string category, ResourceManager resourceManager)
        {
            pools = new Dictionary<string, UnityObjectPool>();
            this.resourceManager = resourceManager;
            this.category = category;
        }

        public Define.Result SetPrefab<T>(string prefabName, UnityEngine.Object prefab, Transform parent) where T : UnityEngine.Object
        {
            if (!pools.ContainsKey(prefabName))
            {
                pools.Add(prefabName, new UnityObjectPool(prefabName, prefab, resourceManager, parent));

                return Define.Result.OK;
            }
            else
            {
                return Define.Result.REDUNDANT_INITIALIZATION;
            }
        }

        public T GetObject<T>(string prefabName) where T : UnityEngine.Object
        {
            if (!pools.ContainsKey(prefabName))
            {
                string TargetPath = string.Format("{0}/{1}/{2}", resourceManager.RootPath, category, prefabName);
                T basePrefab = (T)Resources.Load(TargetPath);
                pools.Add(prefabName, new UnityObjectPool(prefabName, basePrefab, resourceManager, resourceManager.transform));
            }
            UnityObjectPool targetObjectPool = pools[prefabName];

            return (T)targetObjectPool.GetAvailableObject();
        }

        public Define.Result CollectGameObject(GameObject collected)
        {
            if (!pools.ContainsKey(collected.name))
            {
                return Define.Result.NOT_INITIALIZED;
            }

            return pools[collected.name].CollectGameObject(collected);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(string.Format("<{0}>", category));
            stringBuilder.AppendLine();
            foreach (var item in pools)
            {
                stringBuilder.AppendLine(item.Value.ToString());
            }
            return stringBuilder.ToString();
        }
    }

    public class ResourceManager : MonoBehaviour
    {
        //public delegate void SpawnAction<T>(T spawned);

        private Dictionary<string, UnityObjectCollection> collections;

        [SerializeField]
        private string rootPath = "Prefabs";
        public string RootPath { get { return rootPath; } }

        public Define.Result Initialize()
        {
            //this.rootPath = rootPath;
            collections = new Dictionary<string, UnityObjectCollection>();

            return Define.Result.OK;
        }

        // Support for Unity Inspector Prefab Initialization. (recommend)
        public Define.Result SetPrefab<T>(string category, string prefabName, UnityEngine.Object prefab, Transform parent) where T : UnityEngine.Object
        {
            if (!collections.ContainsKey(category))
            {
                collections.Add(category, new UnityObjectCollection(category, this));
            }

            UnityObjectCollection TargetCollection = collections[category];

            return TargetCollection.SetPrefab<T>(prefabName, prefab, parent);
        }


        public T GetObject<T>(string category, string prefabName) where T : UnityEngine.Object
        {
            if (!collections.ContainsKey(category))
            {
                collections.Add(category, new UnityObjectCollection(category, this));
            }

            UnityObjectCollection TargetCollection = collections[category];

            return TargetCollection.GetObject<T>(prefabName);
        }

        public UnityEngine.Object Spawn(UnityEngine.Object prefab)
        {
            return Instantiate<UnityEngine.Object>(prefab);
        }

        public Define.Result CollectGameObject(string resourceCategory, GameObject collected)
        {
            if (!collections.ContainsKey(resourceCategory))
            {
                return Define.Result.NOT_INITIALIZED;
            }

            return collections[resourceCategory].CollectGameObject(collected);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Object report has generated, Click a log for details.");
            foreach (var collection in collections)
            {
                stringBuilder.AppendLine(collection.Value.ToString());
            }
            return stringBuilder.ToString();
        }

        public void ShowCurrentObjects()
        {
            Debug.Log(ToString());
        }
    }
}
