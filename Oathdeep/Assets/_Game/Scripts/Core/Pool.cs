using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Prefab-keyed pool. Spawned objects carry a PooledObject so they can return themselves.</summary>
    public static class Pool
    {
        static readonly Dictionary<GameObject, Stack<GameObject>> Free = new Dictionary<GameObject, Stack<GameObject>>();
        static Transform root;

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, position, rotation).GetComponent<T>();
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!Free.TryGetValue(prefab, out var stack)) Free[prefab] = stack = new Stack<GameObject>();

            GameObject go = null;
            while (stack.Count > 0 && go == null) go = stack.Pop();

            if (go == null)
            {
                go = Object.Instantiate(prefab, position, rotation, Root());
                go.AddComponent<PooledObject>().Prefab = prefab;
            }
            else
            {
                go.transform.SetPositionAndRotation(position, rotation);
                go.SetActive(true);
            }
            return go;
        }

        public static void Despawn(GameObject go)
        {
            if (go == null) return;
            var pooled = go.GetComponent<PooledObject>();
            if (pooled == null || pooled.Prefab == null) { Object.Destroy(go); return; }
            if (!go.activeSelf) return;
            go.SetActive(false);
            Free[pooled.Prefab].Push(go);
        }

        /// <summary>Drops every pooled instance; call when the scene is rebuilt.</summary>
        public static void Clear()
        {
            Free.Clear();
            if (root != null) Object.Destroy(root.gameObject);
            root = null;
        }

        static Transform Root()
        {
            if (root == null) root = new GameObject("[Pool]").transform;
            return root;
        }
    }

    public sealed class PooledObject : MonoBehaviour
    {
        public GameObject Prefab;
    }
}
