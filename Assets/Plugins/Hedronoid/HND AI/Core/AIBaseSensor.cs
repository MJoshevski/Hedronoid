using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Hedronoid;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class AIBaseSensor : HNDGameObject, IAISensor
    {
        [SerializeField]
        protected float m_RayCastDistance = 10f;
        [SerializeField]
        protected float m_RayCastOffset = 1.2f;

        // To Do Listen to player drop in and drop out

        protected List<Transform> m_Players = new List<Transform>(); // to do: change this to a player manager

        protected override void Awake()
        {
            base.Awake();
            GameObject[] goplayers = GameObject.FindGameObjectsWithTag(HNDAI.Settings.PlayerTag);
            for (int i = 0; i < goplayers.Length; i++)
            {
                m_Players.Add(goplayers[i].transform);
            }
        }

        public Transform ClosestPlayer()
        {
            float distance = Mathf.Infinity;
            Transform player = null;
            for (int i = 0; i < m_Players.Count; i++)
            {
                float tmpDist = Vector3.Distance(m_Players[i].position, transform.position);
                if (tmpDist < distance)
                {
                    distance = tmpDist;
                    player = m_Players[i];
                }
            }
            return player;
        }

        public float DistanceToClosestPlayer()
        {
            float distance = Mathf.Infinity;
            for (int i = 0; i < m_Players.Count; i++)
            {
                float tmpDist = Vector3.Distance(m_Players[i].position, transform.position);
                if (tmpDist < distance)
                    distance = tmpDist;
            }
            return distance;
        }


        public GameObject GetObjectInFront()
        {
            RaycastHit hit;
            float distance = Mathf.Infinity;
            GameObject frontObject = null;

            for (int i = -1; i <= 1; i++)
            {
                if (Physics.Raycast(transform.position + (transform.right * i * m_RayCastOffset), transform.forward, out hit, m_RayCastDistance))
                {
                    if (hit.distance < distance)
                    {
                        distance = hit.distance;

                        if(gameObject.layer != hit.transform.gameObject.layer)
                            frontObject = hit.transform.gameObject;
                    }
                }
            }

            for (int i = -1; i <= 1; i += 2)
            {
                if (Physics.Raycast(transform.position + (transform.up * i * m_RayCastOffset), transform.forward, out hit, m_RayCastDistance))
                {
                    if (hit.distance < distance)
                    {
                        distance = hit.distance;
                        if (gameObject.layer != hit.transform.gameObject.layer)
                            frontObject = hit.transform.gameObject;
                    }
                }
            }

            return frontObject;
        }

        public float DistanceFrontalCollision()
        {
            RaycastHit hit;
            float distance = Mathf.Infinity;

            for (int i = -1; i <= 1; i++)
            {
                if (Physics.Raycast(transform.position + (transform.right * i * m_RayCastOffset), transform.forward + (transform.right * i * m_RayCastOffset), out hit, m_RayCastDistance))
                    if (hit.distance < distance)
                        distance = hit.distance;
            }

            for (int i = -1; i <= 1; i += 2)
            {
                if (Physics.Raycast(transform.position + (transform.up * i * m_RayCastOffset), transform.forward, out hit, m_RayCastDistance))
                    if (hit.distance < distance)
                        distance = hit.distance;
            }

            return distance;
        }

        public float DistanceLeftCollision()
        {
            RaycastHit hit;
            float distance = Mathf.Infinity;
            if (Physics.Raycast(transform.position, -transform.right, out hit, m_RayCastDistance))
                distance = hit.distance;

            return distance;
        }

        public float DistanceRightCollision()
        {
            RaycastHit hit;
            float distance = Mathf.Infinity;
            if (Physics.Raycast(transform.position, transform.right, out hit, m_RayCastDistance))
                distance = hit.distance;

            return distance;
        }

        public virtual bool IsAnyPlayerInReach(float distance)
        {
            for (int i = 0; i < m_Players.Count; i++)
            {
                float tmpDist = Vector3.Distance(m_Players[i].position, transform.position);
                if (tmpDist <= distance)
                    return true;
            }
            return false;
        }

        public virtual bool IsPlayerInReach(int player, float distance)
        {
            throw new NotImplementedException();

            // for (int i = 0; i < m_Players.Count; i++)
            // {
            //     if (m_Players[i].GetComponent<CharacterBase>().PlayerID == player)
            //     {
            //         float tmpDist = Vector3.Distance(m_Players[i].position, transform.position);
            //         if (tmpDist <= distance)
            //             return true;
            //     }
            // }

            // return false;
        }

        public virtual Transform GetRandomPlayerInReach(float distance)
        {
            List<Transform> playersInReach = new List<Transform>();

            for (int i = 0; i < m_Players.Count; i++)
            {
                float tmpDist = Vector3.Distance(m_Players[i].position, transform.position);
                if (tmpDist <= distance)
                    playersInReach.Add(m_Players[i]);
            }
            return playersInReach.Count > 0 ? playersInReach[UnityEngine.Random.Range(0, playersInReach.Count)] : null;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            for (int i = -1; i <= 1; i++)
            {
                Gizmos.DrawLine(transform.position + (transform.right * i * m_RayCastOffset), transform.forward * m_RayCastDistance + (transform.position + (transform.right * i * m_RayCastOffset)));
            }

            for (int i = -1; i <= 1; i += 2)
            {
                Gizmos.DrawLine(transform.position + (transform.up * i * m_RayCastOffset), transform.forward * m_RayCastDistance + (transform.position + (transform.up * i * m_RayCastOffset)));
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * m_RayCastDistance);
            Gizmos.DrawLine(transform.position, transform.position - transform.right * m_RayCastDistance);
        }
#endif
    }
}
