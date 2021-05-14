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

        public GameplaySceneContext GameplaySceneContext { get; set; }

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            HNDEvents.Instance.AddListener<StartLevel>(OnStartLevel);
        }

        private void OnStartLevel(StartLevel e)
        {
            StartCoroutine(InitializePlayersCoroutine());
        }

        private IEnumerator InitializePlayersCoroutine()
        {
            if (!Player) Player = FindObjectOfType<PlayerFSM>().cachedGameObject;

            yield return new WaitForEndOfFrame();
            //HNDEvents.Instance.Raise(new PlayersCreatedAndInitialized());
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

        //private void CreatePlayer()
        //{
        //    HNDGameObject p = Instantiate(PlayerPrefab, GameplaySceneContext.cachedTransform);
        //    p.name = "Player";
        //    p.cachedTransform.position = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).position;
        //    p.cachedTransform.rotation = GameplaySceneContext.PlayerSpawner.GetSpawnPoint(0).rotation;
        //    Player = p;
        //}
        
        private void RespawnPlayerAtCheckpoint()
        {
            //if (!Player) CreatePlayer();
            Transform activeSpawnPoint = GameplaySceneContext.PlayerSpawner.ActiveSpawnPoint;

            float prevFollowSpeed = GameplaySceneContext.OrbitCamera.followSpeed;
            GameplaySceneContext.OrbitCamera.followSpeed = 900f;

            Player.transform.position = activeSpawnPoint.position;
            Player.transform.rotation = activeSpawnPoint.rotation;

            GameplaySceneContext.OrbitCamera.followSpeed = prevFollowSpeed;

            D.GameLogFormat("{0} re-spawned at {1}.", Player.name, activeSpawnPoint.name);
        }
    }
}