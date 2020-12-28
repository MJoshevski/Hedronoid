using UnityEngine;
using System.Collections.Generic;
using System;

namespace Hedronoid.Events
{
    /// <summary>
    /// Event system.
    /// Subscribe to events, raise events...
    /// </summary>
    public class NNEvents
    {
        static NNEvents instance = null;
        public static NNEvents Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NNEvents();
                }
                return instance;
            }
        }

        public delegate void NNEventDelegate<T>(T e) where T : NNBaseEvent;
        private delegate void NNEventDelegate(NNBaseEvent e);

        private Dictionary<System.Type, NNEventDelegate> delegates = new Dictionary<System.Type, NNEventDelegate>();
        private Dictionary<System.Delegate, NNEventDelegate> delegateLookup = new Dictionary<System.Delegate, NNEventDelegate>();

        /// <summary>
        /// Adds listener for a specific event.
        /// Don't forget to call RemoveListener when you no logner care about the event, like in OnDestroy().
        /// Otherwise listener will live on even after gameobject is destroyed.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="del">Delegate method</param>
        public void AddListener<T>(NNEventDelegate<T> del) where T : NNBaseEvent
        {
            // Early-out if we've already registered this delegate
            if (delegateLookup.ContainsKey(del))
                return;

            //cache the delegate in its Target, so that it can be removed when the Target dies
            if (del.Target is HNDMonoBehaviour)
                ((HNDMonoBehaviour)del.Target).CacheEventListener(typeof(T), del);

            // Create a new non-generic delegate which calls our generic one.
            // This is the delegate we actually invoke.
            NNEventDelegate internalDelegate = (e) => del((T)e);
            delegateLookup[del] = internalDelegate;

            NNEventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel))
            {
                delegates[typeof(T)] = tempDel += internalDelegate;
            }
            else
            {
                delegates[typeof(T)] = internalDelegate;
            }
        }

        /// <summary>
        /// Removes listener for a specific event
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="del">Delegate method</param>
        public void RemoveListener<T>(NNEventDelegate<T> del) where T : NNBaseEvent
        {
            RemoveListener(typeof(T), del);
        }

        /// <summary>
        /// Removes listener for a specific event
        /// </summary>
        /// <typeparam name="t">Event type</typeparam>
        /// <param name="del">Delegate method</param>
        public void RemoveListener(System.Type t, System.Delegate del)
        {
            NNEventDelegate internalDelegate;
            if (delegateLookup.TryGetValue(del, out internalDelegate))
            {
                NNEventDelegate tempDel;
                if (delegates.TryGetValue(t, out tempDel))
                {
                    tempDel -= internalDelegate;
                    if (tempDel == null)
                    {
                        delegates.Remove(t);
                    }
                    else
                    {
                        delegates[t] = tempDel;
                    }
                }
                delegateLookup.Remove(del);
            }
        }

        /// <summary>
        /// Raises the event and notifies the delegates
        /// </summary>
        /// <param name="e">Event</param>
        public void Raise(NNBaseEvent e)
        {
            NNEventDelegate del;
            if (delegates.TryGetValue(e.GetType(), out del))
            {
                del.Invoke(e);
            }
        }
    }
}