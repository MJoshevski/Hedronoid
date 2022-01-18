using UnityEngine;
using System.Collections.Generic;
using System;
using Hedronoid;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI { 

    public class AIBaseMotor : HNDGameObject, IAIMotor
    {
        [SerializeField]
        protected AIBaseNavigation m_Navigation;

        protected override void Awake()
        {
            if (!m_Navigation) m_Navigation = GetComponent<AIBaseNavigation>();
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected virtual void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
        }

        public virtual void Attack(Transform target)
        {
            throw new NotImplementedException();
        }

        public virtual void Move(Vector3 Direction)
        {
            throw new NotImplementedException();
        }

        public virtual void Interact(Transform target)
        {
            throw new NotImplementedException();
        }
    }
}