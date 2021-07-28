using Hedronoid;
using Hedronoid.Core;
using Hedronoid.HNDFSM;
using System;
using UnityEngine;

namespace UnityMovementAI
{
    public class SeekUnit : HNDFiniteStateMachine, IGameplaySceneContextInjector
    {
        public enum EStates
        {
            GoToTarget,
            AttackTarget,
            ReturnToDefault,
            DefaultMovement,

            Highest
        }

        public Transform target;
        public GameplaySceneContext GameplaySceneContext { get; set; }
        SteeringBasics steeringBasics;

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            CreateState(EStates.DefaultMovement, OnDefaultMovementUpdate, null, null);

            CreateState(EStates.GoToTarget, OnGoToTargetUpdate, null, null);
            CreateState(EStates.AttackTarget, OnAttackTargetUpdate, null, null);
            CreateState(EStates.ReturnToDefault, OnReturnToDefaultUpdate, null, null);

        }

        public virtual void OnGoToTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnAttackTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnReturnToDefaultUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnDefaultMovementUpdate()
        {
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