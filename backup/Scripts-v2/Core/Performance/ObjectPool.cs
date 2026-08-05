using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Core.Performance
{
    public sealed class ObjectPool
    {
        private readonly Stack<GameObject>
            pool = new();

        public GameObject Get(
            GameObject prefab)
        {
            if(pool.Count > 0)
            {
                GameObject obj =
                    pool.Pop();

                obj.SetActive(true);

                return obj;
            }

            return Object.Instantiate(prefab);
        }

        public void Return(
            GameObject obj)
        {
            obj.SetActive(false);

            pool.Push(obj);
        }
    }
}
