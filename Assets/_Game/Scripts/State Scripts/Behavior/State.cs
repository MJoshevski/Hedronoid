﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu]
    public class State : ScriptableObject
    {
    	public StateActions[] onFixed;
        public StateActions[] onUpdate;
        public StateActions[] onEnter;
        public StateActions[] onExit;

        public int idCount;

		[SerializeField]
        public List<Transition> transitions = new List<Transition>();

        public void OnEnter(PlayerStateManager states)
        {
            ExecuteStartActions(states, onEnter);
            ExecuteActions(states, onEnter);
        }

        public void FixedTickStart(PlayerStateManager states)
        {
            ExecuteStartActions(states, onFixed);
            InitTransitions(states);
        }

        public void TickStart(PlayerStateManager states)
        {
            ExecuteStartActions(states, onUpdate);
            InitTransitions(states);
        }

        public void FixedTick(PlayerStateManager states)
		{
            ExecuteActions(states,onFixed);
            CheckTransitions(states);
        }

        public void Tick(PlayerStateManager states)
        {
            ExecuteActions(states, onUpdate);
            CheckTransitions(states);
        }

        public void OnExit(PlayerStateManager states)
        {
            ExecuteStartActions(states, onExit);
            ExecuteActions(states, onExit);
        }

        public void InitTransitions(PlayerStateManager states)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].disable)
                    continue;

                transitions[i].condition.InitCondition(states);
            }
        }

        public void CheckTransitions(PlayerStateManager states)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].disable)
                    continue;

                if (transitions[i].condition.CheckCondition(states))
                {
                    if (transitions[i].targetState != null)
                    {
                        states.currentState = transitions[i].targetState;
                        OnExit(states);
                        states.currentState.OnEnter(states);
                    }
                    return;
                }
            }
        }

        public void ExecuteStartActions(PlayerStateManager states, StateActions[] l)
        {
            for (int i = 0; i < l.Length; i++)
            {
                if (l[i] != null)
                    l[i].Execute_Start(states);
            }
        }

        public void ExecuteActions(PlayerStateManager states, StateActions[] l)
        {
            for (int i = 0; i < l.Length; i++)
            {
                if (l[i] != null)
                    l[i].Execute(states);
            }
        }

        public Transition AddTransition()
        {
            Transition retVal = new Transition();
            transitions.Add(retVal);
            retVal.id = idCount;
            idCount++;
            return retVal;
        }

        public Transition GetTransition(int id)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].id == id)
                    return transitions[i];
            }

            return null;
        }

		public void RemoveTransition(int id)
		{
			Transition t = GetTransition(id);
			if (t != null)
				transitions.Remove(t);
		}

    }
}