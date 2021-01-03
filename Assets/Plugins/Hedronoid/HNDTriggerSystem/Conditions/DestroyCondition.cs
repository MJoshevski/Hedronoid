using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hedronoid;

namespace Hedronoid.TriggerSystem
{
    public class DestroyCondition : HNDCondition
    {

        protected override void OnDisable()
        {
            base.OnDisable();

            SetConditionFulfilled(true, cachedGameObject);
        }
    }
}