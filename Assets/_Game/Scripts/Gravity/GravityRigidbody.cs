﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [RequireComponent(typeof(Rigidbody))]
    public class GravityRigidbody : HNDMonoBehaviour
    {
        Rigidbody body;
        float floatDelay;
        [SerializeField]
        bool floatToSleep = false;

        protected override void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
        }

        void FixedUpdate()
        {
            if (floatToSleep)
            {
                if (body.IsSleeping())
                {
                    floatDelay = 0f;
                    return;
                }

                if (body.velocity.sqrMagnitude < 0.0001f)
                {
                    floatDelay += Time.deltaTime;
                    if (floatDelay >= 1f)
                    {
                        return;
                    }
                }
                else floatDelay = 0f;
            }

            body.AddForce(GravityService.GetGravity(body.position));
        }
    }
}
