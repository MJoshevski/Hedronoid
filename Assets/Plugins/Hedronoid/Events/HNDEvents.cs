using UnityEngine;
using System.Collections.Generic;
using System;

namespace Hedronoid.Events
{
    /// <summary>
    /// Event system.
    /// Subscribe to events, raise events...
    /// </summary>
    public class HNDEvents
    {
        static HNDEvents instance = null;
        public static HNDEvents Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HNDEvents();
                }
                return instance;
            }
        }

        public delegate void HNDEventDelegate<T>(T e) where T : HNDBaseEvent;
        private delegate void HNDEventDelegate(HNDBaseEvent e);

        private Dictionary<System.Type, HNDEventDelegate> delegates = new Dictionary<System.Type, HNDEventDelegate>();
        private Dictionary<System.Delegate, HNDEventDelegate> delegateLookup = new Dictionary<System.Delegate, HNDEventDelegate>();

        /// <summary>
        /// Adds listener for a specific event.
        /// Don't forget to call RemoveListener when you no logner care about the event, like in OnDestroy().
        /// Otherwise listener will live on even after gameobject is destroyed.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="del">Delegate method</param>
        public void AddListener<T>(HNDEventDelegate<T> del) where T : HNDBaseEvent
        {
            // Early-out if we've already registered this delegate
            if (delegateLookup.ContainsKey(del))
                return;

            //cache the delegate in its Target, so that it can be removed when the Target dies
            if (del.Target is HNDMonoBehaviour)
                ((HNDMonoBehaviour)del.Target).CacheEventListener(typeof(T), del);

            // Create a new non-generic delegate which calls our generic one.
            // This is the delegate we actually invoke.
            HNDEventDelegate internalDelegate = (e) => del((T)e);
            delegateLookup[del] = internalDelegate;

            HNDEventDelegate tempDel;
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
        public void RemoveListener<T>(HNDEventDelegate<T> del) where T : HNDBaseEvent
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
            HNDEventDelegate internalDelegate;
            if (delegateLookup.TryGetValue(del, out internalDelegate))
            {
                HNDEventDelegate tempDel;
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
        public void Raise(HNDBaseEvent e)
        {
            HNDEventDelegate del;
            if (delegates.TryGetValue(e.GetType(), out del))
            {
                del.Invoke(e);
            }
        }
    }
}