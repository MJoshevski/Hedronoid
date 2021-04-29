using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using UnityEngine.SceneManagement;
using Hedronoid.Events;

namespace Hedronoid.Core
{
    public class GameController : HNDMonoSingleton<GameController>
    {
        protected GameController() { } // guarantee this will be always a singleton only - can't use the constructor!

        private IEnumerator m_LoadSceneCR;

        private bool IsSceneGamePlay
        {
            get
            {
                return FindObjectOfType<BaseSceneContext>();
            }
        }

        public void LoadScene(int id)
        {
            if (m_LoadSceneCR != null)
            {
                D.CoreError("Attempting to load scene while other load progress is still running. Loading scene with id = " + id + " canceled.");
                return;
            }

            m_LoadSceneCR = LoadSceneCR(id);
            UI.LoadScreenManager.Instance.FadeToBlack(() => StartCoroutine(m_LoadSceneCR));
        }

        private IEnumerator LoadSceneCR(int id)
        {
            // Do some unloading here, if needed.            
            HNDEvents.Instance.Raise(new LevelUnloaded());
            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();
            SceneManager.LoadSceneAsync(id);
            m_LoadSceneCR = null;
        }

        protected override void Start()
        {
            base.Start();

            // This part is for debugging, if the game is started directly in a gameplay scene
            if (IsSceneGamePlay)
            {
                StartCoroutine(DelayedStart());
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (ShortcutController.Instance.IsShortcutPressed(ShortcutController.EShortcuts.RESET_SCENE))
            {
                LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0.1f);

            HNDEvents.Instance.Raise(new StartLevel());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HNDEvents.Instance.Raise(new StartLevel());

            UI.LoadScreenManager.Instance.FadeFromBlack();
        }
    }
}