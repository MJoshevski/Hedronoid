using UnityEngine;
using System.Collections;
using Hedronoid;
using System;

namespace Hedronoid.TriggerSystem
{
    public class ColliderCondition : HNDCondition
    {
        public enum ColliderFilter
        {
            TAG = 0,
            LAYER = 1,
            SPECIFIC = 2
        }

        public enum ConditionType
        {
            ENTER,
            INSIDE,
            EXIT,
            OUTSIDE
        }

        [SerializeField]
        private ColliderFilter m_ColliderFilter;
        public ColliderFilter ColliderFilterType { get { return m_ColliderFilter; } set { m_ColliderFilter = value; } }
        [SerializeField]
        private ConditionType m_ConditionType;
        public ConditionType Condition { get { return m_ConditionType; } set { m_ConditionType = value; } }
        [SerializeField]
        private string m_ColliderTag;
        public string ColliderTag { get { return m_ColliderTag; } set { m_ColliderTag = value; } }
        [SerializeField]
        private LayerMask m_ColliderLayer;
        public LayerMask ColliderLayer { get { return m_ColliderLayer; } set { m_ColliderLayer = value; } }
        [SerializeField]
        private Collider m_ColliderSpecific;
        public Collider ColliderSpecific { get { return m_ColliderSpecific; } set { m_ColliderSpecific = value; } }
        [SerializeField]
        private bool m_CheckSpeed;
        public bool CheckSpeed { get { return m_CheckSpeed; } set { m_CheckSpeed = value; } }
        [SerializeField]
        private bool m_ShouldBeAboveThreshold;
        public bool ShouldBeAboveThreshold { get { return m_ShouldBeAboveThreshold; } set { m_ShouldBeAboveThreshold = value; } }
        [SerializeField]
        private float m_SpeedThreshold;
        public float SpeedThreshold { get { return m_SpeedThreshold; } set { m_SpeedThreshold = value; } }

        // How many objects of the desired kind is inside the trigger. Only used for INSIDE and OUTSIDE condition type.
        private int m_CorrectObjectsInsideCount;

        protected override void Awake()
        {
            base.Awake();

            if (m_ConditionType == ConditionType.OUTSIDE)
            {
                SetConditionFulfilled(true, cachedGameObject);
            }
        }


        private void OnSensorTriggerEnter(Collider collider, GameObject sender)
        {
            OnTriggerEnter(collider);
        }

        private void OnSensorCollisionEnter(Collision collision, GameObject sender)
        {
            OnCollisionEnter(collision);
        }

        private void OnSensorTriggerExit(Collider collider, GameObject sender)
        {
            OnTriggerExit(collider);
        }

        private void OnSensorCollisionExit(Collision collision, GameObject sender)
        {
            OnCollisionExit(collision);
        }


        void Update()
        {
            // If the condition type is OUTSIDE and the collider gets disabled, we force the condition on.
            if (m_ConditionType == ConditionType.OUTSIDE)
            {
                if (!Fulfilled)
                {
                    if (cachedCollider == null || !cachedCollider.enabled)
                    {
                        m_CorrectObjectsInsideCount = 0;
                        SetConditionFulfilled(true, cachedGameObject);
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (m_CheckSpeed && other.attachedRigidbody != null)
            {
                if (m_ShouldBeAboveThreshold)
                {
                    if (other.attachedRigidbody.velocity.magnitude < m_SpeedThreshold)
                        return;
                }
                else
                {
                    if (other.attachedRigidbody.velocity.magnitude > m_SpeedThreshold)
                        return;
                }
            }

            switch (m_ColliderFilter)
            {
                case ColliderFilter.TAG:
                    if (String.IsNullOrEmpty(m_ColliderTag))
                        Debug.LogError("Collider Tag is null or empty");
                    if (!String.IsNullOrEmpty(other.tag) && other.tag == m_ColliderTag)
                        HandleCorrectEnterCollision(other.gameObject);
                    break;

                case ColliderFilter.LAYER:
                    if ((m_ColliderLayer.value & 1 << other.gameObject.layer) != 0)
                        HandleCorrectEnterCollision(other.gameObject);
                    break;

                case ColliderFilter.SPECIFIC:
                    if (other == m_ColliderSpecific)
                        HandleCorrectEnterCollision(other.gameObject);
                    break;
            }
        }

        void OnCollisionEnter(Collision other)
        {
            OnTriggerEnter(other.collider);
        }

        void OnTriggerExit(Collider other)
        {
            switch (m_ColliderFilter)
            {
                case ColliderFilter.TAG:
                    if (!String.IsNullOrEmpty(other.tag) && other.tag == m_ColliderTag)
                        HandleCorrectExitCollision(other.gameObject);
                    break;

                case ColliderFilter.LAYER:
                    if ((m_ColliderLayer.value & 1 << other.gameObject.layer) != 0)
                        HandleCorrectExitCollision(other.gameObject);
                    break;

                case ColliderFilter.SPECIFIC:
                    if (other == m_ColliderSpecific)
                        HandleCorrectExitCollision(other.gameObject);
                    break;
            }
        }

        void OnCollisionExit(Collision other)
        {
            OnTriggerExit(other.collider);
        }

        void HandleCorrectEnterCollision(GameObject other)
        {
            if (m_ConditionType == ConditionType.ENTER)
            {
                SetConditionFulfilled(false, other);
            }
            else if (m_ConditionType == ConditionType.INSIDE)
            {
                if (m_CorrectObjectsInsideCount == 0)
                    SetConditionFulfilled(true, other);
                m_CorrectObjectsInsideCount++;
            }
            else if (m_ConditionType == ConditionType.OUTSIDE)
            {
                if (m_CorrectObjectsInsideCount == 0)
                    SetConditionUnfulfilled(other);
                m_CorrectObjectsInsideCount++;
            }
        }

        void HandleCorrectExitCollision(GameObject other)
        {
            if (m_ConditionType == ConditionType.EXIT)
            {
                SetConditionFulfilled(false, other);
            }
            else if (m_ConditionType == ConditionType.INSIDE)
            {
                m_CorrectObjectsInsideCount--;
                if (m_CorrectObjectsInsideCount == 0)
                    SetConditionUnfulfilled(other);
            }
            else if (m_ConditionType == ConditionType.OUTSIDE)
            {
                m_CorrectObjectsInsideCount--;
                if (m_CorrectObjectsInsideCount == 0)
                    SetConditionFulfilled(true, other);
            }
        }
    }
}