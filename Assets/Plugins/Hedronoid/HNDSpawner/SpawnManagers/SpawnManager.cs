using UnityEngine;
using System.Collections;
using System;

namespace Hedronoid.Spawners
{
    /// <summary>
    /// Abstract class that all spawners inherit from.
    /// Responsible for finding free spawn points.
    /// </summary>
    public abstract class SpawnManager : MonoBehaviour
    {
        /// <summary>
        /// Number of items to equally distribute
        /// </summary>
        protected int m_NumberOfItems;

        [Header("General")]
        [Tooltip("Spawn Rotation configuration.")]
        [SerializeField]
        private ESpawnRotation m_SpawnRotation = ESpawnRotation.SpecifiedForwardDirection;
        public enum ESpawnRotation { SpecifiedForwardDirection, TowardsCenter, TowardsTransform }

        [SerializeField]
        [Tooltip("Only used if Spawn Rotation is set to SpecifiedRotation. Sets spawns to have its forward direction set to this.")]
        private Vector3 m_SpecifiedForwardDirection = Vector3.forward;

        [Tooltip("Only used if Spawn Rotation is set to TowardsTransform. Sets spawns to look towards this transform.")]
        [SerializeField]
        private Transform m_LookTowardsTransform;

        [Header("(Optional) Prevent spawning on other obstacles with colliders")]
        [Tooltip("Prevent spawning on colliders within the layermask. The layermask is used for ray and sphere casting.")]
        [SerializeField]
        protected LayerMask m_ObstacleLayerMask;

        [Tooltip("Within the layermask the obstacles must have this tag to be considered. If this tag is empty all colliders found will be considered as obstacles.")]
        [SerializeField]
        protected string m_ObstacleTag;

        protected bool FindFreeSpawnPointUsingPhysics
        {
            get
            {
                return m_ObstacleLayerMask.value != 0;
            }
        }

        [Tooltip("Radius of the sphere cast to use when finding free points. Sphere Cast is enabled only if is > 0.")]
        [SerializeField]
        protected float m_SphereCastRadius = 0;

        [Tooltip("Range of the raycast/spherecast.")]
        [SerializeField]
        protected float m_RaycastRange = 2;

        [Tooltip("Maximum amount of raycast retries allowed for every single spawn before skipping collision checks.")]
        [SerializeField]
        private int m_nonOccupiedAttempts = 3;

        /// <summary>
        /// Find a free point, by raycasting or spherecasting and checking the tag. 
        /// If the tag is empty all colliders hit will be considered blocking the point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected virtual bool IsPointFree(Vector3 point)
        {
            if (!FindFreeSpawnPointUsingPhysics)
            {
                return true;
            }

            RaycastHit[] hits;
            Ray ray = GetRay(point, transform);

            if (m_SphereCastRadius > 0)
                hits = Physics.SphereCastAll(ray, m_SphereCastRadius, m_RaycastRange, m_ObstacleLayerMask);
            else
                hits = Physics.RaycastAll(ray, m_RaycastRange, m_ObstacleLayerMask);

            bool isFree = hits.Length == 0;

            if (!isFree && !string.IsNullOrEmpty(m_ObstacleTag)) // If tag is not empty/null, check by tag
            {
                isFree = true;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (m_ObstacleTag.Equals(hits[i].collider.tag))
                    {
                        isFree = false;
                        break;
                    }
                }
            }

            if (!isFree)
            {
                D.GameLog("Spawn point is not free: " + hits[0].collider.gameObject.name);
            }

            Debug.DrawLine(ray.origin, ray.origin + (ray.direction * m_RaycastRange), isFree ? Color.green : Color.red, 5, false);
            return isFree;
        }

        /// <summary>
        /// Returns a valid spawn point.
        /// Depending on configurations this position can be checked with raycasts and tags
        /// </summary>
        /// <returns></returns>
        public NNTransformValues GetValidSpawnPoint()
        {
            Vector3 spawnPosition = GetUnvalidatedSpawnPoint();
            int m_nonOccupiedAttemptsLeft = m_nonOccupiedAttempts;

            while (!IsPointFree(spawnPosition) && m_nonOccupiedAttemptsLeft-- > 0)
            {
                spawnPosition = GetUnvalidatedSpawnPoint();
            }
            if (m_nonOccupiedAttemptsLeft < 0)
            {
                D.CoreWarning("Spawner " + GetType().Name + " on gameObject " + gameObject.name + " could not find enough non occupied spots, and were forced to spawn on top of other gmjs.");
            }

            return new NNTransformValues() { Position = spawnPosition, Rotation = GetRotation(spawnPosition) };
        }

        /// <summary>
        /// Sets rotation of a transform to a rotation based on the m_SpawnRotation value
        /// </summary>
        /// <param name="spawnTransform">The spawn transform</param>
        private Quaternion GetRotation(Vector3 spawnPosition)
        {
            if (m_SpawnRotation == ESpawnRotation.SpecifiedForwardDirection)
            {
                return Quaternion.LookRotation(m_SpecifiedForwardDirection);
            }
            else if (m_SpawnRotation == ESpawnRotation.TowardsCenter)
            {
                return Quaternion.LookRotation(-spawnPosition + (this.transform.position));
            }
            else if (m_SpawnRotation == ESpawnRotation.TowardsTransform && m_LookTowardsTransform != null)
            {
                return Quaternion.LookRotation(-spawnPosition + (m_LookTowardsTransform.position));
            }
            else
            {
                return Quaternion.identity;
            }
        }

        /// <summary>
        /// Returns the next spawn point which has not been evaluated if valid.
        /// Valid depends on configuration, this might be evaluated using raycasts and tags.
        /// </summary>
        /// <returns></returns>
        protected abstract Vector3 GetUnvalidatedSpawnPoint();

#if UNITY_EDITOR
        /// <summary>
        /// Draws the raycast/spherecast as performed for collision check
        /// </summary>
        protected void DrawGizmoRayCastLine(Vector3 raycastStartPoint, Transform tr)
        {
            Gizmos.color = Color.red;
            Ray ray = GetRay(raycastStartPoint, tr);
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * m_RaycastRange);
            if (m_SphereCastRadius > 0.1f)
                Gizmos.DrawSphere(ray.origin + ray.direction * m_RaycastRange, m_SphereCastRadius);
        }
#endif

        /// <summary>
        /// Generates the ray for the ray/spherecast.
        /// Can be overwritten to change how the ray is generated for both the gizmo drawing and the actual ray/spherecast.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Generates the ray for the ray/spherecast</returns>
        protected virtual Ray GetRay(Vector3 point, Transform tr)
        {
            return new Ray(GetRayStartPosition(point) + (tr.up * m_RaycastRange), -tr.up * m_RaycastRange);
        }

        /// <summary>
        /// Returns the start point of the ray.
        /// Can be overwritten to change how the ray is generated for both the gizmo drawing and the actual ray/spherecast.
        /// Can be used add an offset to the start point.
        /// </summary>
        /// <param name="point">Center of the ray</param>
        /// <returns>Returns the start point of the ray</returns>
        protected virtual Vector3 GetRayStartPosition(Vector3 point)
        {
            return point;
        }

        [ContextMenu("GetSpawnPointDebug")]
        public void GetSpawnPointDebug()
        {
            GetValidSpawnPoint();
        }
    }
}