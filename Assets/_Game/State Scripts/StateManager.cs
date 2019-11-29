using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    public class StateManager : MonoBehaviour
    {
        public float health;
        
        public State currentState;

        public MovementVariables movementVariables;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public Transform Transform;
        [HideInInspector]
        public new Rigidbody Rigidbody;

        [HideInInspector]
        public Animator Animator;
        public AnimatorHashes animHashes;
        public AnimatorData animData;

        public bool isJumping;
        public bool isGrounded;
        [HideInInspector]
        public float timeSinceJump;

        private void Start()
        {
            Transform = this.transform;
            Rigidbody = GetComponent<Rigidbody>();
            Animator = GetComponentInChildren<Animator>();
            animHashes = new AnimatorHashes();
            animData = new AnimatorData(Animator);
        }

        private void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;

            if (currentState != null)
            {
                currentState.FixedTick(this);
            }
        }

        private void Update()
        {
            delta = Time.deltaTime;

            if(currentState != null)
            {
                currentState.Tick(this);
            }
        }
    }
}
