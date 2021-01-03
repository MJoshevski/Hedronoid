using UnityEngine;
using System.Collections;
using Hedronoid;
using Hedronoid.ObjectPool;

namespace Hedronoid.TriggerSystem
{
    public class InstantiateGameObjectAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        public enum EObjectPositioning
        {
            OBJECT_POSITION,
            WORLD_POSITION,
            LOCAL_POSITION,
            TRIGGER_POSITION,
            TRIGGERING_OBJECT_POSITION
        }

        public enum EObjectOrientation
        {
            WORLD_ROTATION,
            LOCAL_ROTATION,
            TRIGGERING_OBJECT_ROTATION,
            SPAWN_OBJECT_ROTATION
        }

        // NOTE:
        // We're hiding most of the parameters in this script, because it has a custom inspector that draws these things in a more understandable manner.
        [Header("'Instantiate Game Object' Specific Settings")]
        [SerializeField]
        private bool m_UsePool;
        [SerializeField]
        private HNDPoolManager m_PoolManager;
        [SerializeField]
        private int m_InstantiateCount = 1;
        [HideInInspector]
        [SerializeField]
        private GameObject m_InstantiateObject;
        public GameObject InstantiateObject { get { return m_InstantiateObject; } set { m_InstantiateObject = value; } }
        [HideInInspector]
        [SerializeField]
        private bool m_CloneTriggeringObject;
        public bool CloneTriggeringObject { get { return m_CloneTriggeringObject; } set { m_CloneTriggeringObject = value; } }
        [HideInInspector]
        [SerializeField]
        private Transform m_NewParent;
        public Transform NewParent { get { return m_NewParent; } set { m_NewParent = value; } }
        [HideInInspector]
        [SerializeField]
        private bool m_UseTriggeringObjectParent;
        public bool UseTriggeringObjectParent { get { return m_UseTriggeringObjectParent; } set { m_UseTriggeringObjectParent = value; } }
        [HideInInspector]
        [SerializeField]
        private EObjectPositioning m_ObjectPositioning;
        public EObjectPositioning ObjectPositioning { get { return m_ObjectPositioning; } set { m_ObjectPositioning = value; } }
        [HideInInspector]
        [SerializeField]
        private Transform m_ObjectPos;
        public Transform ObjectPos { get { return m_ObjectPos; } set { m_ObjectPos = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_WorldPos;
        public Vector3 WorldPos { get { return m_WorldPos; } set { m_WorldPos = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_LocalPos;
        public Vector3 LocalPos { get { return m_LocalPos; } set { m_LocalPos = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_TriggerPosOffset;
        public Vector3 TriggerPosOffset { get { return m_TriggerPosOffset; } set { m_TriggerPosOffset = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_TriggeringObjectPosOffset;
        public Vector3 TriggeringObjectPosOffset { get { return m_TriggeringObjectPosOffset; } set { m_TriggeringObjectPosOffset = value; } }
        [HideInInspector]
        [SerializeField]
        private EObjectOrientation m_ObjectOrientation;
        public EObjectOrientation ObjectOrientation { get { return m_ObjectOrientation; } set { m_ObjectOrientation = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_WorldRotation;
        public Vector3 WorldRotation { get { return m_WorldRotation; } set { m_WorldRotation = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_LocalRotation;
        public Vector3 LocalRotation { get { return m_LocalRotation; } set { m_LocalRotation = value; } }

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            for (int i = 0; i < m_InstantiateCount; i++)
            {
                if (m_InstantiateObject != null)
                {
                    CreateObject(m_InstantiateObject, triggeringObject.transform, m_UsePool);
                }
                if (m_CloneTriggeringObject)
                {
                    CreateObject(triggeringObject, triggeringObject.transform, false);
                }
            }
        }

        void CreateObject(GameObject go, Transform other, bool fromPool)
        {
            if (go != null)
            {
                GameObject newGo = null;
                if (m_UsePool)
                {
                    if (m_PoolManager != null)
                    {
                        newGo = m_PoolManager.RentObject(go);
                    }
                }
                
                if (!m_UsePool || newGo == null)
                {
                    newGo = GameObject.Instantiate(go) as GameObject;
                }

                Transform newTrans = newGo.transform;
                if (m_UseTriggeringObjectParent)
                    newTrans.parent = other.parent;
                else
                    newTrans.parent = m_NewParent;

                if (m_ObjectPositioning == EObjectPositioning.WORLD_POSITION)
                {
                    newTrans.position = m_WorldPos;
                }
                else if (m_ObjectPositioning == EObjectPositioning.LOCAL_POSITION)
                {
                    newTrans.localPosition = m_LocalPos;
                }
                else if (m_ObjectPositioning == EObjectPositioning.TRIGGER_POSITION)
                {
                    newTrans.position = cachedTransform.position + m_TriggerPosOffset;
                }
                else if (m_ObjectPositioning == EObjectPositioning.TRIGGERING_OBJECT_POSITION)
                {
                    newTrans.position = other.position + m_TriggeringObjectPosOffset;
                }
                else if (m_ObjectPositioning == EObjectPositioning.OBJECT_POSITION)
                {
                    newTrans.localPosition = m_ObjectPos.position;
                }


                if (m_ObjectOrientation == EObjectOrientation.WORLD_ROTATION)
                {
                    newTrans.eulerAngles = m_WorldRotation;
                }
                else if (m_ObjectOrientation == EObjectOrientation.LOCAL_ROTATION)
                {
                    newTrans.localEulerAngles = m_LocalRotation;
                }
                else if (m_ObjectOrientation == EObjectOrientation.TRIGGERING_OBJECT_ROTATION)
                {
                    newTrans.rotation = other.rotation;
                }
                else if (m_ObjectOrientation == EObjectOrientation.SPAWN_OBJECT_ROTATION)
                {
                    newTrans.rotation = m_ObjectPos.rotation;
                }
            }
        }
    }
}