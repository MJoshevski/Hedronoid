using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedronoid.Spawners
{
    /// <summary>
    /// Spawns on Transforms.
    /// </summary>
    public class SpawnManagerFreeChildPoint : SpawnManager, ISpawnEquallyDistributed
    {
        [Header("Child point spawn settings")]
        [Tooltip("Points to spawn in.")]
        [SerializeField]
        private List<Transform> m_SpawnPoints = new List<Transform>();

        [Tooltip("Add all children as spawn points.")]
        [SerializeField]
        private bool m_AddChildsAsSpawnPoints;

        [Tooltip("Determines if the points should be shuffled instead of how it is ordered in Spawn Points.")]
        [SerializeField]
        private bool m_ShufflePoints;

#if UNITY_EDITOR
        [Header("Inspector gizmo drawing")]
        [Tooltip("If enabled the gizmo is always drawn for this spawner. Otherwise it is only drawn when selected by the inspector.")]
        [SerializeField]
        private bool m_AlwaysDrawGizmo;
#endif

        int m_SpawnPointIndex = 0;

        /// <summary>
        /// Picks up all children transforms if configured to
        /// </summary>
        private void Awake()
        {
            if (m_AddChildsAsSpawnPoints)
            {
                var childs = transform.GetComponentsInChildren<Transform>(false);
                for (int i = 0; i < childs.Length; ++i)
                {
                    // Ignore parent.
                    if (childs[i] != transform)
                    {
                        m_SpawnPoints.Add(childs[i]);
                    }
                }
            }

            ResetEqualDistribution();
        }

        public List<Transform> GetAllSpawnPoints()
        {
            return m_SpawnPoints.ToList();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Always draws spheres and line showing forward vector
        /// </summary>
        private void OnDrawGizmos()
        {
            if (m_AlwaysDrawGizmo)
            {
                DrawGizmo();
            }
        }

        /// <summary>
        /// Draws spheres and line showing forward vector when gmj with this component is selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!m_AlwaysDrawGizmo)
            {
                DrawGizmo();
            }
        }

        private void DrawGizmo()
        {
            var spawnPoints = m_SpawnPoints.Count == 0 ? transform.GetComponentsInChildren<Transform>(false).ToList() : m_SpawnPoints;

            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint == this.transform) continue;
                if (spawnPoint == null) continue;

                Color color = Color.green;
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawSphere(spawnPoint.position, 0.6f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward);
                DrawGizmoRayCastLine(spawnPoint.position, spawnPoint);
            }
        }
#endif

        /// <summary>
        /// Returns the next point in the spawn points list
        /// </summary>
        /// <returns></returns>
        protected override Vector3 GetUnvalidatedSpawnPoint()
        {
            if (m_SpawnPoints.Count == 0)
            {
                D.CoreWarning("No free spawn points.");
                return Vector3.zero;
            }

            var index = m_SpawnPointIndex;
            m_SpawnPointIndex = (m_SpawnPointIndex + 1) % m_SpawnPoints.Count;
            return m_SpawnPoints[index].transform.position;
        }

        /// <summary>
        /// Sets the amount of items to equally distribute
        /// </summary>
        /// <param name="numberOfItems"></param>
        public void SetAmountOfEquallyDistributedItems(int numberOfItems)
        {
            if (m_ShufflePoints)
            {
                m_SpawnPoints.Shuffle();
            }
        }

        /// <summary>
        /// Resets the equally distribution
        /// </summary>
        public void ResetEqualDistribution()
        {
            m_SpawnPointIndex = 0;
        }
    }
}