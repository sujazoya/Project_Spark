using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Core.Performance
{
    public sealed class PoolManager
        : MonoBehaviour
    {
        private readonly Dictionary<
            string,
            ObjectPool> pools =
                new();

        public ObjectPool GetPool(
            string id)
        {
            if(!pools.TryGetValue(
                id,
                out ObjectPool pool))
            {
                pool =
                    new ObjectPool();

                pools.Add(
                    id,
                    pool);
            }

            return pool;
        }
    }
}
