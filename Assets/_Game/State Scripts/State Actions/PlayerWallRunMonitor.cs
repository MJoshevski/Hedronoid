﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Player Wall Run Monitor")]
    public class PlayerWallRunMonitor : StateActions
    {
        private bool _wallRunning;
        private CollisionVariables collisionVariables;
        private WallRunVariables wallRunVariables;

        public override void Execute_Start(PlayerStateManager states)
        {
            collisionVariables = states.collisionVariables;
            wallRunVariables = states.wallRunVariables;
        }

        public override void Execute(PlayerStateManager state)
        {
            var arrayFlags = collisionVariables.ArrayFlags;
            var arrayHits = collisionVariables.ArrayHits;

            for (int i = 2; i < arrayFlags.Length; i++)
            {
                if (arrayFlags[i])
                {
                    var contact = arrayHits[i].normal;
                    var dot = Vector3.Dot(GravityService.Instance.GravityUp, contact);

                    if (Mathf.Approximately(0f, dot))
                    {
                        wallRunVariables.WallRunning = true;
                        return;
                    }
                }
            }
            wallRunVariables.WallRunning = false;
        }
    }
}
