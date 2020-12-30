using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using Hedronoid.HNDFSM;
using Hedronoid.Events;
namespace Hedronoid.Core
{
    public class BaseGameState : HNDFiniteStateMachine, IGameplaySceneContextInjector
    {
        public GameplaySceneContext GameplaySceneContext { get; set; }

        public enum EGenericGameStates
        {
            INTRO,
            GAMEPLAY,
            END
        }

        protected FSMState m_IntroState;
        protected FSMState m_GameplayState;
        protected FSMState m_EndState;

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            HNDEvents.Instance.AddListener<StartLevel>(OnStartLevel);

            m_IntroState = CreateCoroutineState(EGenericGameStates.INTRO, OnUpdateIntro, OnEnterIntro, OnExitIntro);
            m_GameplayState = CreateCoroutineState(EGenericGameStates.GAMEPLAY, OnUpdateGameplay, OnEnterGameplay, OnExitGameplay);
            m_EndState = CreateCoroutineState(EGenericGameStates.END, OnUpdateEnd, OnEnterEnd, OnExitEnd);
        }

        private void OnStartLevel(StartLevel e)
        {
            ChangeState(EGenericGameStates.INTRO);
        }

        #region INTRO
        private IEnumerator OnEnterIntro(FSMState fromState)
        {
            yield return null;
        }

        private void OnUpdateIntro()
        {
        }

        private void OnExitIntro(FSMState toState)
        {
            HNDEvents.Instance.Raise(new IntroEnded());
        }
        #endregion

        #region GAMEPLAY
        private IEnumerator OnEnterGameplay(FSMState fromState)
        {
            yield return new WaitForEndOfFrame();
        }

        private void OnUpdateGameplay()
        {
        }

        private void OnExitGameplay(FSMState toState)
        {
        }
        #endregion

        #region END
        private IEnumerator OnEnterEnd(FSMState fromState)
        {
            yield return new WaitForEndOfFrame();
        }

        private void OnUpdateEnd()
        {
        }

        private void OnExitEnd(FSMState toState)
        {
        }
        #endregion
    }
}