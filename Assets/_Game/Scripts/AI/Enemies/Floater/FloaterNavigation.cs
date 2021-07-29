using Hedronoid;
using Hedronoid.AI;
using Hedronoid.Core;
using Hedronoid.HNDFSM;
using System;
using UnityEngine;

namespace UnityMovementAI
{
    public class FloaterNavigation : AIBaseNavigation, IGameplaySceneContextInjector
    {
        public enum EFloaterStates
        {
            AttackTarget = EStates.Highest + 1,
        }

        public Transform target;
        public GameplaySceneContext GameplaySceneContext { get; set; }
        SteeringBasics steeringBasics;

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            CreateState(EFloaterStates.AttackTarget, OnAttackTargetUpdate, null, null);
        }
        public override void OnGoToTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnAttackTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public override void OnReturnToDefaultUpdate()
        {
            throw new NotImplementedException();
        }

        public override void OnDefaultMovementUpdate()
        {
            // Some default patrol movement or just a simple animation should go here
            throw new NotImplementedException();
        }
        protected override void Start()
        {
            base.Start();

            steeringBasics = GetComponent<SteeringBasics>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (GameplaySceneContext.Player.cachedTransform == null) return;

            target = GameplaySceneContext.Player.cachedTransform;

            Vector3 accel = steeringBasics.Seek(target.position);

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
    }
}