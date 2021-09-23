using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using Hedronoid.Core;
using Hedronoid.Player;
using Hedronoid.Events;

namespace Hedronoid.Gravity
{
    public class GravityManager : HNDMonoBehaviour, IGameplaySceneContextInjector
    {
        public GameplaySceneContext GameplaySceneContext { get; set; }

        public List<GravitySource> gravitySourcesInScene = new List<GravitySource>();
        public List<string> gravityParentNamesInScene;

        private PlayerFSM player;
        private List<GravitySource> activeSources = new List<GravitySource>();
        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            HNDEvents.Instance.AddListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            HNDEvents.Instance.RemoveListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HNDEvents.Instance.RemoveListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
        }
        private void OnPlayerCreatedAndInitialized(PlayerCreatedAndInitialized e)
        {
            player = GameplaySceneContext.Player;
        }
        protected virtual void FixedUpdate()
        {
            if (!player) return;

            activeSources = GravityService.GetActiveGravitySources();

            if (activeSources.Count > 0)
                GravityService.PrioritizeActiveOverlappedGravities(
                    player.transform.position,
                    player.movementVariables.MoveDirection);
        }
        public void ScanForGravitySources()
        {
            gravitySourcesInScene = FindObjectsOfType<GravitySource>().ToList();
            gravityParentNamesInScene = new List<string>(gravitySourcesInScene.Count);
            gravityParentNamesInScene.Clear();

            foreach (GravitySource gs in gravitySourcesInScene)
                gravityParentNamesInScene.Add(gs.transform.parent.name);
        }
    }
}
