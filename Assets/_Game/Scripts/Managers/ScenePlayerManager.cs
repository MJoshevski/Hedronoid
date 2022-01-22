 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hedronoid;
using Hedronoid.Player;
using Hedronoid.Events;

namespace Hedronoid.Core
{
    public class ScenePlayerManager : HNDGameObject, IGameplaySceneContextInjector
    {
        public GameObject Player { get { return m_Player; } }
        private GameObject m_Player = null;

        public GameObject PlayerPrefab { get { return m_PlayerPrefab; } }
        [SerializeField]
        private GameObject m_PlayerPrefab;

        public GameObject OrbitCamera { get { return m_OrbitCamera; } }
        private GameObject m_OrbitCamera = null;

        public GameObject OrbitCameraPrefab { get { return m_OrbitCameraPrefab; } }
        [SerializeField]
        private GameObject m_OrbitCameraPrefab;

        private PlayerSpawner playerSpawner;

        public GameplaySceneContext GameplaySceneContext { get; set; }

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            playerSpawner = GameplaySceneContext.PlayerSpawner;

            HNDEvents.Instance.AddListener<StartLevel>(OnStartLevel);
        }

        private void OnStartLevel(StartLevel e)
        {
            StartCoroutine(InitializePlayerCoroutine());
        }

        private IEnumerator InitializePlayerCoroutine()
        {
            if (!m_Player) CreatePlayer();
            if (!m_OrbitCamera) CreateCamera();


            yield return new WaitForEndOfFrame();
            HNDEvents.Instance.Raise(new PlayerCreatedAndInitialized());            
        }

        void Update()
        {
            //Matej: DEBUG PURPOSES ONLY!!!
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    GameplaySceneContext.PlayerSpawner.SetActiveCheckpoint(
                        GameplaySceneContext.PlayerSpawner.GetSpawnPoint(i).gameObject);

                    RespawnAtCheckpoint();
                    return;
                }
            }
            //
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HNDEvents.Instance.RemoveListener<StartLevel>(OnStartLevel);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            HNDEvents.Instance.RemoveListener<StartLevel>(OnStartLevel);

        }

        private void CreatePlayer()
        {
            GameObject p = Instantiate(m_PlayerPrefab, GameplaySceneContext.cachedTransform);
            p.name = "Player";

            if (playerSpawner.ActiveSpawnPoint == null)
            {
                p.transform.position = playerSpawner.GetSpawnPoint(0).position;
                p.transform.rotation = playerSpawner.GetSpawnPoint(0).rotation;
                p.transform.forward = playerSpawner.GetSpawnPoint(0).forward;
            }
            else
            {
                p.transform.position = playerSpawner.ActiveSpawnPoint.position;
                p.transform.rotation = playerSpawner.ActiveSpawnPoint.rotation;
                p.transform.forward = playerSpawner.ActiveSpawnPoint.forward;
            }

            m_Player = p.gameObject;
        }

        private void CreateCamera()
        {
            GameObject c = Instantiate(m_OrbitCameraPrefab, GameplaySceneContext.cachedTransform);
            c.name = "OrbitCamera";

            if (playerSpawner.ActiveSpawnPoint == null)
            {
                c.transform.position = playerSpawner.GetSpawnPoint(0).position;
                c.transform.rotation = playerSpawner.GetSpawnPoint(0).rotation;
                c.transform.forward = playerSpawner.GetSpawnPoint(0).forward;
            }
            else
            {
                c.transform.position = playerSpawner.ActiveSpawnPoint.position;
                c.transform.rotation = playerSpawner.ActiveSpawnPoint.rotation;
                c.transform.forward = playerSpawner.ActiveSpawnPoint.forward;

            }
            m_OrbitCamera = c.gameObject;
        }

        public void RespawnAtCheckpoint()
        {
            RespawnCameraAtCheckpoint();
            RespawnPlayerAtCheckpoint();
        }

        private void RespawnCameraAtCheckpoint()
        {
            if (!m_OrbitCamera) CreateCamera();
            Transform activeSpawnPoint = playerSpawner.ActiveSpawnPoint;

            OrbitCamera orbitCamera;

            m_OrbitCamera.TryGetComponent(out orbitCamera);
            if (!orbitCamera) orbitCamera = m_OrbitCamera.GetComponentInChildren<OrbitCamera>();
            if (!orbitCamera)
            {
                D.GameError("ScenePlayerManager doesn't have an orbit camera prefab referenced or can't locate 'OrbitCamera.cs' on referenced prefab. Returning...");
                return;
            }

            m_OrbitCamera.transform.position = activeSpawnPoint.position;
            m_OrbitCamera.transform.rotation = activeSpawnPoint.rotation;
            m_OrbitCamera.transform.forward = activeSpawnPoint.forward;

            D.GameLogFormat("{0} re-spawned at {1}.", m_OrbitCamera.name, activeSpawnPoint.name);
        }
        private void RespawnPlayerAtCheckpoint()
        {
            if (!m_Player) CreatePlayer();
            Transform activeSpawnPoint = playerSpawner.ActiveSpawnPoint;

            PlayerFSM playerFSM;

            m_Player.TryGetComponent(out playerFSM);
            if (!playerFSM) playerFSM = m_Player.GetComponentInChildren<PlayerFSM>();
            if (!playerFSM)
            {
                D.GameError("ScenePlayerManager doesn't have a player prefab referenced or can't locate 'PlayerFSM.cs' on referenced prefab. Returning...");
                return;
            }

            m_Player.transform.position = activeSpawnPoint.position;
            m_Player.transform.rotation = activeSpawnPoint.rotation;
            m_Player.transform.forward = activeSpawnPoint.forward;

            D.GameLogFormat("{0} re-spawned at {1}.", m_Player.name, activeSpawnPoint.name);
        }
    }
}