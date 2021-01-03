using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class ChaserSensor : AIBaseSensor
    {
        [SerializeField]
        private List<string> m_TagToChase = new List<string>();
        [SerializeField]
        private List<GameObject> m_ObjectToChase = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < m_TagToChase.Count; i++)
            {
                GameObject[] gochase = GameObject.FindGameObjectsWithTag(m_TagToChase[i]);
                for (int j = 0; j < gochase.Length; j++)
                    m_ObjectToChase.Add(gochase[j]);
            }
        }

        public virtual bool IsAnyTargetInReach(float distance)
        {
            for (int i = 0; i < m_ObjectToChase.Count; i++)
            {
                float tmpDist = Vector3.Distance(m_ObjectToChase[i].transform.position, transform.position);
                if (tmpDist <= distance)
                    return true;
            }
            return false;
        }

        public virtual Transform GetRandomTargetInReach(float distance)
        {
            List<Transform> TargetInReach = new List<Transform>();

            for (int i = 0; i < m_ObjectToChase.Count; i++)
            {
                float tmpDist = Vector3.Distance(m_ObjectToChase[i].transform.position, transform.position);
                if (tmpDist <= distance)
                    TargetInReach.Add(m_ObjectToChase[i].transform);
            }
            return TargetInReach.Count > 0 ? TargetInReach[UnityEngine.Random.Range(0, TargetInReach.Count)] : null;
        }

    }
}