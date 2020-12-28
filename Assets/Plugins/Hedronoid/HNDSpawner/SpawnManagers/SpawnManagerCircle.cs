using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hedronoid.Spawners
{
    public class SpawnManagerCircle : SpawnManager, ISpawnEquallyDistributed
    {
        [Header("Circle spawn settings")]
        [SerializeField]
        [Tooltip("Recommended material is Sprites-Default. Material sued to highlight the circle in inspector")]
        private Material m_InspectorMaterial;

        [SerializeField]
        [Tooltip("Size of the inner circle")]
        private float m_SpawnRadiusMin = 5.0f;
        [SerializeField]
        [Tooltip("Size of the outer circle")]
        private float m_SpawnRadiusMax = 10.0f;

        [SerializeField]
        [Tooltip("Circle spawn offset reduces the degrees of the circle")]
        private float m_OffsetDegrees = 0f;

        [Header("Spawn with equal distribution")]
        [Tooltip("Spawn items equally distributed. NOTICE: Amount of items to be distributed is provided by the object spawner controller variable 'm_SpawnersNumberOfItems'")]
        [SerializeField]
        private bool m_SpawnEquallyDistributed;

        private float m_AngleDelta;
        private float m_CurAngle = 0.0f;
        private static readonly int m_CornersInCircle = 30;

        public void SetAmountOfEquallyDistributedItems(int numberOfItems)
        {
            m_AngleDelta = ((Mathf.PI * 2.0f) - m_OffsetDegrees * Mathf.Deg2Rad) / numberOfItems;
            m_CurAngle = Mathf.Deg2Rad * m_OffsetDegrees;
        }

        protected override Vector3 GetUnvalidatedSpawnPoint()
        {
            float distance = UnityEngine.Random.Range(m_SpawnRadiusMin, m_SpawnRadiusMax);
            if (m_SpawnEquallyDistributed)
            {
                var spawnPosition = transform.position + distance * (transform.right * Mathf.Sin(m_CurAngle) + transform.forward * Mathf.Cos(m_CurAngle));
                IncreaseAngle();
                return spawnPosition;
            }
            else
            {
                var randomAngle = Random.Range(m_OffsetDegrees, 360) * Mathf.Deg2Rad;
                return transform.position + distance * (transform.right * Mathf.Sin(randomAngle) + transform.forward * Mathf.Cos(randomAngle));
            }
        }

        /// <summary>
        /// Helper function that helps increase the angle when the spawns is set to be equally distributed
        /// </summary>
        private void IncreaseAngle()
        {
            m_CurAngle += m_AngleDelta;
            if (m_CurAngle > (Mathf.PI * 2.0f))
                m_CurAngle = Mathf.Deg2Rad * m_OffsetDegrees;
        }

        /// <summary>
        /// Used to reset equal distribution
        /// </summary>
        public void ResetEqualDistribution()
        {
            m_CurAngle = 0.0f;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Draws the raycast, and a sphere as indicator of the spherecast, and the spawn area sphere
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            DrawGizmoRayCastLine(transform.position, transform);
            DrawGizmoCircle();
        }

        /// <summary>
        /// Draws a circle that covers the spawn area
        /// </summary>
        private void DrawGizmoCircle()
        {
            Color color = new Color(0f, 1f, 0f, 0.4f);
            Mesh mesh = new Mesh();

            // Create vertex positions for the sphere
            List<Vector3> vertexPositions = new List<Vector3>();
            var centerPos = this.transform.position;
            for (int i = 0; i < m_CornersInCircle; i++)
            {
                var angle = Mathf.Deg2Rad * (m_OffsetDegrees + i * (360f - m_OffsetDegrees) / m_CornersInCircle);
                var minPos = centerPos + (transform.right * Mathf.Sin(angle) + transform.forward * Mathf.Cos(angle)) * m_SpawnRadiusMin;
                vertexPositions.Add(minPos);
                var maxPos = centerPos + (transform.right * Mathf.Sin(angle) + transform.forward * Mathf.Cos(angle)) * m_SpawnRadiusMax;
                vertexPositions.Add(maxPos);
            }
            // Add the initial position to close sphere
            var initalAngle = 0;
            var initialMinPos = centerPos + (transform.right * Mathf.Sin(initalAngle) + transform.forward * Mathf.Cos(initalAngle)) * m_SpawnRadiusMin;
            vertexPositions.Add(initialMinPos);
            var initialMaxPos = centerPos + (transform.right * Mathf.Sin(initalAngle) + transform.forward * Mathf.Cos(initalAngle)) * m_SpawnRadiusMax;
            vertexPositions.Add(initialMaxPos);

            // Triangulate the sphere positions
            var vertices = new List<Vector3>();
            for (int i = 0; i < vertexPositions.Count - 2; i++)
            {
                vertices.Add(vertexPositions[i]);
                vertices.Add(vertexPositions[i + 1]);
                vertices.Add(vertexPositions[i + 2]);
            }
            mesh.SetVertices(vertices);

            // Set triangles
            var triangles = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                triangles[i] = i;
            }
            mesh.triangles = triangles;

            // Recalculate normals
            mesh.RecalculateNormals();

            // Sadly the same color has to be set for all vertices
            Color[] colors = new Color[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                colors[i] = color;
            mesh.colors = colors;

            // Set material that supports color
            if (m_InspectorMaterial == null)
            {
                m_InspectorMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
            m_InspectorMaterial.SetPass(0);

            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
        }
#endif

    }
}