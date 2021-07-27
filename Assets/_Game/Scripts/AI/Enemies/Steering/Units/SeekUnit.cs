using Hedronoid;
using Hedronoid.Core;
using UnityEngine;

namespace UnityMovementAI
{
    public class SeekUnit : HNDMonoBehaviour, IGameplaySceneContextInjector
    {
        public Transform target;
        public GameplaySceneContext GameplaySceneContext { get; set; }
        SteeringBasics steeringBasics;

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);
        }
        protected override void Start()
        {
            base.Start();


            steeringBasics = GetComponent<SteeringBasics>();
        }

        void FixedUpdate()
        {
            if (GameplaySceneContext.Player.cachedTransform == null) return;

            target = GameplaySceneContext.Player.cachedTransform;

            Vector3 accel = steeringBasics.Seek(target.position);

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
    }
}