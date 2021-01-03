using UnityEngine;
using System.Collections;
using Hedronoid;
using Hedronoid.ObjectPool;

namespace Hedronoid.TriggerSystem
{
    public class ReturnToPoolManagerAction : HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        protected override void PerformAction(GameObject other)
        {
            var poolObj = other.GetComponent<HNDPoolObject>();

            if (poolObj != null)
                poolObj.ReturnOrDestroy();
            else
            {
                poolObj = other.GetComponentInChildren<HNDPoolObject>();
                if(poolObj != null) poolObj.ReturnOrDestroy();
            }
        }
    }
}