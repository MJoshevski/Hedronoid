using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hedronoid.HNDFSM
{
    /// <summary>
    /// Inherit from this class whenever you want an object that uses a Finite State Machine (FSM). New states are added by calling CreateState with a name and a reference to the functions needed (update, enter and exit).
    /// </summary>
    public class HNDFiniteStateMachine : HNDGameObject
    {
        public delegate void OnStateChanged(FSMState newState);
        public event OnStateChanged StateChangedEvent;

        [SerializeField]
        [HNDReadOnly]
        private string _currentState;

        protected Dictionary<int, FSMState> stateList = new Dictionary<int, FSMState>();

        protected FSMState currentState;
        public FSMState CurrentState
        {
            get { return currentState; }
        }

        public string CurrentStateString
        {
            get { return _currentState; }
        }

        protected FSMState defaultState;
        protected float stateTime
        {
            get
            {
                if (currentState != null)
                    return currentState.stateTime;
                return 0f;
            }
        }

        protected float stateTimeUnscaled
        {
            get
            {
                if (currentState != null)
                    return currentState.stateTimeUnscaled;
                return 0.0f;
            }
        }

        public float CurrentStateTime
        {
            get { return stateTime; }
        }

        public float CurrentStateTimeUnscaled
        {
            get { return stateTimeUnscaled; }
        }


        /// <summary>
	    /// Creates a new state.
	    /// </summary>
	    /// <returns>The state.</returns>
	    /// <param name="name">Enum of the state.</param>
	    /// <param name="onUpdate">Method to be called when the state is updated.</param>
	    /// <param name="onEnter">Method to be called when the state is entered.</param>
	    /// <param name="onExit">Method to be called when the state is exited.</param>
        protected virtual FSMState CreateState(System.Enum name, FSMState.OnUpdateState onUpdate, FSMState.OnEnterState onEnter, FSMState.OnExitState onExit)
        {
            int value = name.ToInt();

            if (stateList.ContainsKey(value))
            {
                Debug.LogWarning(cachedGameObject.name + " does already have a state called '" + name + "'. Please choose another name", cachedGameObject);
                return stateList[value];
            }
            FSMState newState = new FSMState(name);
            newState.onUpdateState += onUpdate;
            newState.onEnterState += onEnter;
            newState.onExitState += onExit;
            stateList.Add(value, newState);

            return newState;
        }

        protected virtual FSMState CreateState(System.Enum name, FSMState.OnUpdateState onUpdate, FSMState.OnEnterState onEnter, FSMState.OnExitState onExit,
            FSMState.OnFixedUpdateState onFixedUpdate, FSMState.OnLateUpdateState onLateUpdate)
        {
            FSMState newState = CreateState(name, onUpdate, onEnter, onExit);
            newState.onFixedUpdateState += onFixedUpdate;
            newState.onLateUpdateState += onLateUpdate;

            return newState;
        }

        /// <summary>
	    /// Creates a new state with coroutine OnEnter.
	    /// </summary>
	    /// <returns>The state.</returns>
	    /// <param name="name">Enum of the state.</param>
	    /// <param name="onUpdate">Method to be called when the state is updated.</param>
	    /// <param name="onEnter">Method to be called when the state is entered.</param>
	    /// <param name="onExit">Method to be called when the state is exited.</param>
        protected virtual FSMState CreateCoroutineState(System.Enum name, FSMState.OnUpdateState onUpdate, FSMState.OnEnterStateCoroutine onEnter, FSMState.OnExitState onExit)
        {
            FSMState newState = CreateState(name, onUpdate, null, onExit);
            newState.onEnterStateCoroutine += onEnter;

            return newState;
        }

        protected virtual FSMState CreateCoroutineState(System.Enum name, FSMState.OnUpdateState onUpdate, FSMState.OnEnterStateCoroutine onEnter, FSMState.OnExitState onExit,
            FSMState.OnFixedUpdateState onFixedUpdate, FSMState.OnLateUpdateState onLateUpdate)
        {
            FSMState newState = CreateState(name, onUpdate, null, onExit);
            newState.onEnterStateCoroutine += onEnter;
            newState.onFixedUpdateState += onFixedUpdate;
            newState.onLateUpdateState += onLateUpdate;

            return newState;
        }

        /// <summary>
        /// Add a previously created state.
        /// </summary>
        /// <param name="state">The state to add.</param>
        protected void AddState(FSMState state)
        {
            int value = state.GetStateRawValue();

            if (stateList.ContainsKey(value))
            {
                Debug.LogWarning(cachedGameObject.name + " does already have a state called '" + state.name + "'. Cannot add new state with this name.", cachedGameObject);
                return;
            }
            stateList.Add(value, state);
            // If this is the first state to be added, we set it as the default state
            if (stateList.Count == 1)
                defaultState = state;
        }

        protected virtual void Update()
        {
            if (currentState != null)
            {
                if (currentState.onUpdateState != null)
                    currentState.onUpdateState();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (currentState != null && currentState.onFixedUpdateState != null)
                currentState.onFixedUpdateState();
        }

        protected virtual void LateUpdate()
        {
            if (currentState != null && currentState.onLateUpdateState != null)
            {
                currentState.onLateUpdateState();
            }
        }

        public bool HasState(System.Enum newState)
        {
            return stateList.ContainsKey(newState.ToInt());
        }

        public bool HasState(FSMState newState)
        {
            return stateList.ContainsValue(newState);
        }

        public virtual void ChangeState(System.Enum newState)
        {
            int value = newState.ToInt();

            if (stateList.ContainsKey(value))
            {
                ChangeState(stateList[value]);
            }
            else
            {
                Debug.LogError("No state named '" + newState + "' was found for game object '" + gameObject.name + "'. Changing to null state!", gameObject);

                if (currentState != null && currentState.onExitState != null)
                    currentState.onExitState(null);

                if (currentState.EnterCoroutine != null)
                    StopCoroutine(currentState.EnterCoroutine);

                currentState = null;
                _currentState = "";
            }
        }

        public virtual void ChangeState(FSMState newState)
        {
            // Don't do anything if we switch to the same state
            if (currentState == newState)
            {
                return;
            }

            // D.NetError("Changing to client state : " + newState.name.ToString() );

            if (currentState != null)
            {
                if (currentState.onExitState != null)
                    currentState.onExitState(newState);

                if (currentState.EnterCoroutine != null)
                    StopCoroutine(currentState.EnterCoroutine);
            }

            if (newState == null)
            {
                currentState = null;
                return;
            }

            if (!stateList.ContainsValue(newState))
                Debug.LogWarning("Changing to a state that is not registered in the state list! This might not be what you want to do.");
            FSMState oldState = currentState;
            currentState = newState;

            _currentState = currentState.name.ToString();
            if (currentState != null && currentState.onEnterState != null)
                currentState.onEnterState(oldState);

            if (currentState != null && currentState.onEnterStateCoroutine != null)
                currentState.EnterCoroutine = StartCoroutine(currentState.onEnterStateCoroutine(oldState));

            if (StateChangedEvent != null)
                StateChangedEvent(newState);
        }

        public bool IsInState(System.Enum stateEnum)
        {
            if (currentState != null && currentState.EqualsAsInt(stateEnum.ToInt()))
            {
                return true;
            }

            return false;
        }

        public bool IsInState(int stateInt)
        {
            if (currentState != null && currentState.EqualsAsInt(stateInt))
            {
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws a string to the screen using Unitys GUI system with Handels.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="worldPos">World position of the text (will be transformed in to screen space).</param>
        /// <param name="color">Color of the text.</param>
        void EditorDrawString(string text, Vector3 worldPos, Color color)
        {
            Handles.BeginGUI();

            var view = UnityEditor.SceneView.currentDrawingSceneView;

            if (!view) return;

            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            if ((screenPos.y < 0) || (screenPos.y > Screen.height) || (screenPos.x < 0) || (screenPos.x > Screen.width))
            {
                Handles.EndGUI();
                return;
            }

            Color prevColor = GUI.color;

            GUIStyle boxLeftAligned = new GUIStyle(GUI.skin.box);
            boxLeftAligned.alignment = TextAnchor.UpperLeft;
            GUI.color = color;
            Vector3 size = GUI.skin.box.CalcSize(new GUIContent(text));
            GUI.Box(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text, boxLeftAligned);
            GUI.color = prevColor;

            Handles.EndGUI();
        }

        /// <summary>
        /// Returns a string describing the server player.
        /// Used by OnDrawGizmos() to draw player information on screen.
        /// </summary>
        /// <returns>Information about the player.</returns>
        protected virtual string GetEditorDebugString()
        {
            return "Name: " + gameObject.name + "\n" +
                "State: " + ((CurrentState != null && CurrentState.name != null) ? CurrentState.name.ToString() : "none");
        }

        /// <summary>
        /// Base editor gizmo functionality.
        /// Draws information about the server player.
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (cachedTransform == null || CurrentState == null)
                return;

            EditorDrawString(GetEditorDebugString(), cachedTransform.position, Color.red);
        }
#endif
    }

    [System.Serializable]
    public class FSMState
    {
        public System.Enum name;

        public delegate void OnEnterState(FSMState fromState);
        public OnEnterState onEnterState;

        public delegate IEnumerator OnEnterStateCoroutine(FSMState fromState);
        public OnEnterStateCoroutine onEnterStateCoroutine;

        public delegate void OnUpdateState();
        public OnUpdateState onUpdateState;

        public delegate void OnFixedUpdateState();
        public OnFixedUpdateState onFixedUpdateState;

        public delegate void OnLateUpdateState();
        public OnLateUpdateState onLateUpdateState;

        public delegate void OnExitState(FSMState toState);
        public OnExitState onExitState;

        public float stateTime { get; set; }
        public float stateTimeUnscaled { get; set; }

        public Coroutine EnterCoroutine;

        private int m_IntValue = Int32.MinValue;

        public FSMState(System.Enum name)
        {
            this.name = name;
            m_IntValue = Convert.ToInt32(name);

            onEnterState = OnEnter;
            onExitState = OnExit;
            onUpdateState = OnUpdate;
            onFixedUpdateState = OnFixedUpdate;
            onLateUpdateState = OnLateUpdate;
        }

        public FSMState()
        {
            onEnterState = OnEnter;
            onExitState = OnExit;
            onUpdateState = OnUpdate;
            onFixedUpdateState = OnFixedUpdate;
            onLateUpdateState = OnLateUpdate;
        }

        public virtual void OnUpdate()
        {
            stateTime += Time.deltaTime;
            stateTimeUnscaled += Time.unscaledDeltaTime;
        }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }

        public virtual void OnEnter(FSMState fromState)
        {
            stateTime = 0f;
            stateTimeUnscaled = 0.0f;

        }

        public virtual void OnExit(FSMState toState)
        {
            stateTime = 0f;
            stateTimeUnscaled = 0.0f;
        }

        public int GetStateRawValue()
        {
            if (m_IntValue == Int32.MinValue)
            {
                m_IntValue = name.ToInt();
            }
            return m_IntValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is FSMState)
            {
                if (GetStateRawValue() == ((FSMState)obj).GetStateRawValue())
                {
                    return true;
                }
            }
            else if (obj is System.Enum)
            {
                if (GetStateRawValue() == ((System.Enum)obj).ToInt())
                {
                    return true;
                }
            }

            return base.Equals(obj);
        }

        public bool EqualsAsInt(int value)
        {
            return GetStateRawValue() == value;
        }

        public override int GetHashCode()
        {
            return GetStateRawValue();
        }

        public static bool operator ==(FSMState a, System.Enum b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(FSMState a, System.Enum b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return !object.ReferenceEquals(b, null);
            }

            return !a.Equals(b);
        }

        public static bool operator ==(FSMState a, int b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return false;
            }

            return a.EqualsAsInt(b);
        }

        public static bool operator !=(FSMState a, int b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return true;
            }

            return !a.EqualsAsInt(b);
        }

        public override string ToString()
        {
            return name != null ? name.ToString() : "NULL";
        }

    }
}