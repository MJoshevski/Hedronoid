using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid.Player;
using Hedronoid.ObjectPool;
using Hedronoid.Weapons;
using Hedronoid.Particle;

namespace Hedronoid.Core
{
    public class GameplaySceneContext : BaseSceneContext
    {
        private PlayerFSM m_Player;
        public PlayerFSM Player
        {
            get
            {
                if (m_Player == null)
                {
                    m_Player = cachedGameObject.GetComponentInChildren<PlayerFSM>(true);
                }

                return m_Player;
            }
        }

        private ScenePlayerManager m_ScenePlayerManager;
        public ScenePlayerManager ScenePlayerManager
        {
            get
            {
                if (m_ScenePlayerManager == null)
                {
                    m_ScenePlayerManager = cachedGameObject.GetComponentInChildren<ScenePlayerManager>(true);
                }

                return m_ScenePlayerManager;
            }
        }

        private PlayerSpawner m_PlayerSpawner;
        public PlayerSpawner PlayerSpawner
        {
            get
            {
                if (m_PlayerSpawner == null)
                {
                    m_PlayerSpawner = cachedGameObject.GetComponentInChildren<PlayerSpawner>(true);
                }

                return m_PlayerSpawner;
            }
        }

        private BulletPoolManager m_BulletPoolManager;
        public BulletPoolManager BulletPoolManager
        {
            get
            {
                if (m_BulletPoolManager == null)
                {
                    m_BulletPoolManager = cachedGameObject.GetComponentInChildren<BulletPoolManager>(true);
                }

                return m_BulletPoolManager;
            }
        }

        private HNDGameState m_GameState;
        public HNDGameState GameState
        {
            get
            {
                if (m_GameState == null)
                {
                    m_GameState = cachedGameObject.GetComponentInChildren<HNDGameState>(true);
                }

                return m_GameState;
            }
        }

        private OrbitCamera m_orbitCamera;
        public OrbitCamera OrbitCamera
        {
            get
            {
                if (m_orbitCamera == null)
                {
                    m_orbitCamera = cachedGameObject.GetComponentInChildren<OrbitCamera>(true);
                }

                return m_orbitCamera;
            }
        }

        private ParticleManager m_particleManager;
        public ParticleManager ParticleManager
        {
            get
            {
                if (m_particleManager == null)
                {
                    m_particleManager = cachedGameObject.GetComponentInChildren<ParticleManager>(true);
                }

                return m_particleManager;
            }
        }
    }

    public interface IGameplaySceneContextInjector
    {
        GameplaySceneContext GameplaySceneContext { get; set; }
    }

    public static class SceneContextInjectorExtensions
    {
        public static void Inject(this IGameplaySceneContextInjector context, GameObject gameObject)
        {
            context.GameplaySceneContext = gameObject.GetComponentInParent<GameplaySceneContext>(true);

            if (context.GameplaySceneContext == null)
            {
                D.CoreWarning("Missing (or Disabled) SceneContext in scene!");
                return;
            }
        }
    }
}