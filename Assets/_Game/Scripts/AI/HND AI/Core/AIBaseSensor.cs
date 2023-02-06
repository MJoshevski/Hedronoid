using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Hedronoid;
using Unity.Collections;
using Hedronoid.Core;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    [System.Serializable]
    public class SensorDebugSettings
    {
        public bool ShowInEditor;
        public bool ShowInBuild;
    }

    public class AIBaseSensor : HNDGameObject, IAISensor, IGameplaySceneContextInjector
    {
        public GameplaySceneContext GameplaySceneContext { get; set; }

        [Header("General Settings")]
        [SerializeField]
        protected float m_RayCastDistance = 10f;
        [SerializeField]
        protected Vector3 m_RayCastPositionOffset = Vector3.zero;
        [SerializeField]
        protected float m_RayCastInBetweenOffset = 1.2f;
        [SerializeField]
        protected SensorDebugSettings m_RaycastsDebugSettings;

        protected Collider[] m_ColliderBuffer = new Collider[10];
        protected List<Transform> m_TargetsInRange = new List<Transform>(10);
        protected Physics m_Physics;

        [Header("Culling settings")]
        [SerializeField]
        protected bool m_EnableCullingGizmo = true;
        [SerializeField]
        protected bool m_ShouldCullEnemy = true;
        public bool ShouldCullEnemy
        {
            get { return m_ShouldCullEnemy; }
            set { m_ShouldCullEnemy = value; }
        }
        [SerializeField]
        protected float m_EnemyCullingDistance = 100f;
        public float EnemyCullingDistance
        {
            get { return m_EnemyCullingDistance; }
            set { m_EnemyCullingDistance = value; }
        }

        [Header("Cone-Of-Sight Settings")]
        [SerializeField]
        protected SensorDebugSettings m_ConeDebugSettings;
        [SerializeField]
        protected bool m_RequireLineOfSight = true;
        [SerializeField]
        protected Vector3 m_ConeOriginOffset = Vector3.zero;
        [SerializeField]
        protected float m_ConeMaxDistance = 35f;
        public float ConeMaxDistance { get { return m_ConeMaxDistance; } }
        [ReadOnly]
        [SerializeField]
        protected float m_ConeRadius;
        [SerializeField]
        [Range(1f, 89f)]
        protected float m_ConeAngle = 15f;

        [Header("Areal Sensor Settings")]
        [SerializeField]
        [Tooltip("This is how far away we will detect the player or NPCs")]
        protected float m_SensorRange = 3f;
        public float SensorRange
        {
            get { return m_SensorRange; }
        }
        [SerializeField]
        protected SensorDebugSettings m_SensorDebugSettings;


        [SerializeField]
        [Tooltip("This is how far the cutoff will be once the player has already entered the sensor range")]
        protected float m_SensorCutoffRange = 20f;
        public float SensorCutoffRange
        {
            get { return m_SensorCutoffRange; }
        }
        [SerializeField]
        protected SensorDebugSettings m_SensorCutoffDebugSettings;


        [SerializeField]
        [Tooltip("This is how often we will check for new targets around us if we are patrolling.")]
        protected float m_sensorTimestep = 0.25f;
        public float SensorTimeStep
        {
            get { return m_sensorTimestep; }
        }

        // To Do Listen to player drop in and drop out

        protected List<Transform> m_Players = new List<Transform>(); // to do: change this to a player manager

        protected virtual void OnValidate()
        {
            m_EnemyCullingDistance = Mathf.Abs(m_EnemyCullingDistance);
            m_ConeRadius = Mathf.Tan(m_ConeAngle * Mathf.Deg2Rad) * m_ConeMaxDistance;
            m_ConeMaxDistance = Mathf.Max(m_ConeMaxDistance, 0);
            m_SensorRange = Mathf.Max(m_SensorRange, 0f);
            m_SensorCutoffRange = Mathf.Max(m_SensorCutoffRange, m_SensorCutoffRange);
        }

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            GameObject[] goplayers = GameObject.FindGameObjectsWithTag(HNDAI.Settings.PlayerTag);
            for (int i = 0; i < goplayers.Length; i++)
            {
                m_Players.Add(goplayers[i].transform);
            }
        }

        protected virtual void FixedUpdate()
        {
            OnFixedUpdateEditorGizmos();
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
                if (Physics.Raycast(transform.position + (transform.right * i * m_RayCastInBetweenOffset), transform.forward, out hit, m_RayCastDistance))
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
                if (Physics.Raycast(transform.position + (transform.up * i * m_RayCastInBetweenOffset), transform.forward, out hit, m_RayCastDistance))
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
                if (Physics.Raycast(transform.position + (transform.right * i * m_RayCastInBetweenOffset), transform.forward + (transform.right * i * m_RayCastInBetweenOffset), out hit, m_RayCastDistance))
                    if (hit.distance < distance)
                        distance = hit.distance;
            }

            for (int i = -1; i <= 1; i += 2)
            {
                if (Physics.Raycast(transform.position + (transform.up * i * m_RayCastInBetweenOffset), transform.forward, out hit, m_RayCastDistance))
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

        public virtual bool IsPlayerWithinDistance(float distance)
        {
            if (!this.enabled || !GameplaySceneContext || !GameplaySceneContext.Player) return false;

            float tmpDist = Vector3.Distance(GameplaySceneContext.Player.transform.position, transform.position);
            if (tmpDist <= distance)
                return true;

            return false;
        }

        public virtual bool IsPlayerOutOfCullingRange()
        {
            if (!IsPlayerWithinDistance(m_EnemyCullingDistance))
                return true;

            return false;
        }
        public virtual bool IsPlayerInLineOfSight(Transform player)
        {
            RaycastHit hit;
            Ray r = new Ray(transform.position, (player.position - transform.position).normalized);

            if (Physics.Raycast(r, out hit, m_ConeMaxDistance, ~0, QueryTriggerInteraction.Ignore))
                if ((HNDAI.Settings.PlayerLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
                    return true;

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

        public virtual Transform GetTargetWithinReach(float distance, bool requireConditions = false)
        {
            if (!this.enabled) return null;

            if (m_ShouldCullEnemy && IsPlayerOutOfCullingRange())
                return null;

            m_TargetsInRange.Clear();
            // First check if we have any players in range
            var players = Physics.OverlapSphereNonAlloc(transform.position, distance, m_ColliderBuffer, HNDAI.Settings.PlayerLayer);

            if (players > 0)
            {
                return GameplaySceneContext.Player.transform;
            }

            return null;
        }

        public Transform GetTargetInsideCone(Vector3 direction)
        {
            if (!this.enabled) return null;

            if (m_ShouldCullEnemy && IsPlayerOutOfCullingRange())
                return null;

            if (m_RequireLineOfSight && !IsPlayerInLineOfSight(GameplaySceneContext.Player.transform))
                return null;

            // First check if we have any players in the cone
            RaycastHit[] players = m_Physics.ConeCastNonAlloc(
                transform.position + m_ConeOriginOffset,
                m_ConeRadius,
                direction,
                m_ColliderBuffer.Length,
                m_ConeMaxDistance,
                HNDAI.Settings.PlayerLayer,
                m_ConeAngle);

            foreach (RaycastHit rh in players)
                if (rh.collider != null)
                    return GameplaySceneContext.Player.transform;

            return null;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (!this.enabled) return;

            if (m_ShouldCullEnemy && IsPlayerOutOfCullingRange() && Application.isPlaying)
                return;

            if (m_RaycastsDebugSettings.ShowInEditor)
            {
                Gizmos.color = Color.green;

                // Frontal detection lines - GREEN
                for (int i = -1; i <= 1; i++)
                {
                    Gizmos.DrawLine(transform.position + m_RayCastPositionOffset + (transform.right * i * m_RayCastInBetweenOffset), transform.forward * m_RayCastDistance + (transform.position + (transform.right * i * m_RayCastInBetweenOffset)));
                }

                for (int i = -1; i <= 1; i += 2)
                {
                    Gizmos.DrawLine(transform.position + m_RayCastPositionOffset + (transform.up * i * m_RayCastInBetweenOffset), transform.forward * m_RayCastDistance + (transform.position + (transform.up * i * m_RayCastInBetweenOffset)));
                }

                // Left/Right detection lines - RED
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + transform.right * m_RayCastDistance);
                Gizmos.DrawLine(transform.position, transform.position - transform.right * m_RayCastDistance);
            }

            if (m_ConeDebugSettings.ShowInEditor)
            {
                // Cone of vision with spherical base - YELLOW (no target) or RED (target)
                Popcron.Gizmos.ConeSpherical(
                    transform.position + m_ConeOriginOffset,
                    transform.rotation,
                    m_ConeRadius,
                    m_ConeMaxDistance,
                    m_ConeAngle,
                    !Application.isPlaying || GetTargetInsideCone(transform.forward) == null ? Color.yellow : Color.red);
            }

            if (m_SensorDebugSettings.ShowInEditor)
            {
                // Sensor sphere - CYAN
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, SensorRange);
            }

            if (m_SensorCutoffDebugSettings.ShowInEditor)
            {
                // Sensor cutoff sphere - BLUE
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, SensorCutoffRange);
            }

            if (m_EnableCullingGizmo)
            {
                // Culling gizmo sphere - BLACK
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(transform.position, EnemyCullingDistance);
            }

        }
#endif
        protected virtual void OnFixedUpdateEditorGizmos()
        {
            // MATEJ: For debugging purposes only, so we can see the cone of sight in Editor as well
            // as in builds should we choose to.

            // GENERAL RAYCASTS GIZMO
            if (m_RaycastsDebugSettings.ShowInBuild)
            {
                for (int i = -1; i <= 1; i++)
                {
                    Popcron.Gizmos.Line(transform.position + m_RayCastPositionOffset + (transform.right * i * m_RayCastInBetweenOffset), transform.forward * m_RayCastDistance + (transform.position + (transform.right * i * m_RayCastInBetweenOffset)), Color.green);
                }

                for (int i = -1; i <= 1; i += 2)
                {
                    Popcron.Gizmos.Line(transform.position + m_RayCastPositionOffset + (transform.up * i * m_RayCastInBetweenOffset), transform.forward * m_RayCastDistance + (transform.position + (transform.up * i * m_RayCastInBetweenOffset)), Color.green);
                }

                // Left/Right detection lines - RED
                Popcron.Gizmos.Line(transform.position, transform.position + transform.right * m_RayCastDistance, Color.red);
                Popcron.Gizmos.Line(transform.position, transform.position - transform.right * m_RayCastDistance, Color.red);
            }

            // Sensor sphere - CYAN
            if (m_SensorDebugSettings.ShowInBuild)
                Popcron.Gizmos.Sphere(transform.position, SensorRange, Color.cyan);

            // Sensor cutoff sphere - BLUE
            if (m_SensorCutoffDebugSettings.ShowInBuild)
                Popcron.Gizmos.Sphere(transform.position, SensorCutoffRange, Color.blue);
        }
    }
}
