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

        public GameplaySceneContext GameplaySceneContext { get; set; }

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

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
            p.transform.position = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).position;
            p.transform.rotation = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).rotation;
            m_Player = p.gameObject;
        }

        private void CreateCamera()
        {
            GameObject c = Instantiate(m_OrbitCameraPrefab, GameplaySceneContext.cachedTransform);
            c.name = "OrbitCamera";
            c.transform.position = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).position;
            c.transform.rotation = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).rotation;
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
            Transform activeSpawnPoint = GameplaySceneContext.PlayerSpawner.ActiveSpawnPoint;

            OrbitCamera orbitCamera;

            m_OrbitCamera.TryGetComponent(out orbitCamera);
            if (!orbitCamera) orbitCamera = GetComponentInChildren<OrbitCamera>();
            if (!orbitCamera)
            {
                D.GameError("ScenePlayerManager doesn't have an orbit camera prefab referenced or can't locate 'OrbitCamera.cs' on referenced prefab. Returning...");
                return;
            }

            m_OrbitCamera.transform.position = activeSpawnPoint.position;
            m_OrbitCamera.transform.rotation = activeSpawnPoint.rotation;

            D.GameLogFormat("{0} re-spawned at {1}.", m_OrbitCamera.name, activeSpawnPoint.name);
        }
        private void RespawnPlayerAtCheckpoint()
        {
            if (!m_Player) CreatePlayer();
            Transform activeSpawnPoint = GameplaySceneContext.PlayerSpawner.ActiveSpawnPoint;

            PlayerFSM playerFSM;

            m_Player.TryGetComponent(out playerFSM);
            if (!playerFSM) playerFSM = GetComponentInChildren<PlayerFSM>();
            if (!playerFSM)
            {
                D.GameError("ScenePlayerManager doesn't have a player prefab referenced or can't locate 'PlayerFSM.cs' on referenced prefab. Returning...");
                return;
            }

            m_Player.transform.position = activeSpawnPoint.position;
            m_Player.transform.rotation = activeSpawnPoint.rotation;

            D.GameLogFormat("{0} re-spawned at {1}.", m_Player.name, activeSpawnPoint.name);
        }
    }
}