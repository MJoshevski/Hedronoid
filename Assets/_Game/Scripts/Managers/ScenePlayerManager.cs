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
        public GameObject Player;
        public GameObject PlayerPrefab;

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
            if (!Player) CreatePlayer();

            yield return new WaitForEndOfFrame();
            HNDEvents.Instance.Raise(new PlayerCreatedAndInitialized());            
        }

        void Update()
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    GameplaySceneContext.PlayerSpawner.ActiveSpawnPoint = 
                        GameplaySceneContext.PlayerSpawner.GetSpawnPoint(i);

                    RespawnPlayerAtCheckpoint();
                    return;
                }
            }
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
            GameObject p = Instantiate(PlayerPrefab, GameplaySceneContext.cachedTransform);
            p.name = "Player";
            PlayerFSM playerFSM = p.GetComponentInChildren<PlayerFSM>();
            playerFSM.transform.position = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).position;
            playerFSM.transform.rotation = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).rotation;
            Player = p.gameObject;
        }

        private void RespawnPlayerAtCheckpoint()
        {
            if (!Player) CreatePlayer();
            Transform activeSpawnPoint = GameplaySceneContext.PlayerSpawner.ActiveSpawnPoint;

            PlayerFSM playerFSM = Player.GetComponentInChildren<PlayerFSM>();
            playerFSM.transform.position = activeSpawnPoint.position;
            playerFSM.transform.rotation = activeSpawnPoint.rotation;

            GameplaySceneContext.OrbitCamera.transform.position = activeSpawnPoint.position;
            GameplaySceneContext.OrbitCamera.transform.rotation = activeSpawnPoint.rotation;
            GameplaySceneContext.OrbitCamera.transform.forward = activeSpawnPoint.forward;

            D.GameLogFormat("{0} re-spawned at {1}.", Player.name, activeSpawnPoint.name);
        }
    }
}