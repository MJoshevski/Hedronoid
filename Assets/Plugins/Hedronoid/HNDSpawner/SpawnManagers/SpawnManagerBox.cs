using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Spawners
{
    public class SpawnManagerBox : SpawnManager
    {
        [Header("Box spawn settings")]
        [SerializeField]
        [Tooltip("Box area to spawn in.")]
        private Vector3 m_BoxToSpawnIn;

        protected override Vector3 GetUnvalidatedSpawnPoint()
        {
            return transform.position +
                transform.right * Random.Range(-m_BoxToSpawnIn.x * 0.5f, m_BoxToSpawnIn.x * 0.5f) +
                transform.up * Random.Range(-m_BoxToSpawnIn.y * 0.5f, m_BoxToSpawnIn.y * 0.5f) +
                transform.forward * Random.Range(-m_BoxToSpawnIn.z * 0.5f, m_BoxToSpawnIn.z * 0.5f);
        }

        protected override Vector3 GetRayStartPosition(Vector3 point)
        {
            return base.GetRayStartPosition(point) - transform.up * (m_BoxToSpawnIn.y / 2f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            DrawGizmoRayCastLine(transform.position, transform);

            Gizmos.color = Color.green;
            Vector3 pos = transform.position;

            // Calculate positions for corner at bottom left back corner
            Vector3 lowCornerX = transform.right * (-m_BoxToSpawnIn.x * 0.5f);
            Vector3 lowCornerY = transform.up * (-m_BoxToSpawnIn.y * 0.5f);
            Vector3 lowCornerZ = transform.forward * (-m_BoxToSpawnIn.z * 0.5f);
            Vector3 lowCorner = lowCornerX + lowCornerY + lowCornerZ;

            // Calculate positions for corner at top right forward corner
            Vector3 highCornerX = transform.right * m_BoxToSpawnIn.x * 0.5f;
            Vector3 highCornerY = transform.up * m_BoxToSpawnIn.y * 0.5f;
            Vector3 highCornerZ = transform.forward * m_BoxToSpawnIn.z * 0.5f;
            Vector3 highCorner = highCornerX + highCornerY + highCornerZ;

            // Draw edges from bottom left back corner
            Gizmos.DrawLine(pos + lowCorner, pos + highCornerX + lowCornerY + lowCornerZ);
            Gizmos.DrawLine(pos + lowCorner, pos + lowCornerX + highCornerY + lowCornerZ);
            Gizmos.DrawLine(pos + lowCorner, pos + lowCornerX + lowCornerY + highCornerZ);

            // Draw edges that does not connect to either corner
            Gizmos.DrawLine(pos + highCornerX + lowCornerY + lowCornerZ, pos + highCornerX + highCornerY + lowCornerZ);
            Gizmos.DrawLine(pos + highCornerX + lowCornerY + lowCornerZ, pos + highCornerX + lowCornerY + highCornerZ);
            Gizmos.DrawLine(pos + lowCornerX + lowCornerY + highCornerZ, pos + lowCornerX + highCornerY + highCornerZ);
            Gizmos.DrawLine(pos + lowCornerX + lowCornerY + highCornerZ, pos + highCornerX + lowCornerY + highCornerZ);
            Gizmos.DrawLine(pos + lowCornerX + highCornerY + lowCornerZ, pos + highCornerX + highCornerY + lowCornerZ);
            Gizmos.DrawLine(pos + lowCornerX + highCornerY + lowCornerZ, pos + lowCornerX + highCornerY + highCornerZ);

            // Draw edges from top right forward corner
            Gizmos.DrawLine(pos + highCornerX + highCornerY + lowCornerZ, pos + highCorner);
            Gizmos.DrawLine(pos + highCornerX + lowCornerY + highCornerZ, pos + highCorner);
            Gizmos.DrawLine(pos + lowCornerX + highCornerY + highCornerZ, pos + highCorner);
        }
#endif
    }
}