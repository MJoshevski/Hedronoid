using UnityEngine;
using System.Collections.Generic;
using System;
using Hedronoid;
using Hedronoid.Events;

namespace Hedronoid
{
    /// <summary>
    /// Base class for everything in the game. 
    /// HNDGameObject inherits from this and most objects should inherit from HNDGameObject. 
    /// However, managers and controllers might inherit from this class.
    /// 
    /// It cleans up its Event listeners and instanced Materials by itself.
    /// </summary>
    public class HNDMonoBehaviour : MonoBehaviour
    {
        //Cache of Event listeners, which will be Released when this object dies. 
        protected Dictionary<Type, List<Delegate>> cachedEventListeners = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Used by Events system when adding new Listener on this HNDMonoBehaviour
        /// </summary>
        public void CacheEventListener(Type t, Delegate del)
        {
            if (!cachedEventListeners.ContainsKey(t))
                cachedEventListeners[t] = new List<Delegate>();
            cachedEventListeners[t].Add(del);
        }

        protected virtual void Awake()
        { }

        protected virtual void Start()
        { }

        protected virtual void OnEnable()
        { }

        protected virtual void OnDisable()
        { }

        protected virtual void OnDestroy()
        {
            RemoveAllListeners();
            CleanupInstancedMaterials();
        }

        private void CleanupInstancedMaterials()
        {
#if !UNITY_EDITOR
		Renderer r = GetComponent<Renderer>();
		if (r != null)
		{
			if (r.sharedMaterials != null && r.sharedMaterials.Length > 0)
			{
				for (int i = 0; i < r.sharedMaterials.Length; i++)
				{
					if (r.sharedMaterials[i] == null)
						continue;
					if (r.sharedMaterials[i].name.EndsWith("(Instance)"))
					{
						r.sharedMaterials[i].mainTexture = null;
						Destroy(r.sharedMaterials[i]);
					}
				}
			}
		}
		
		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		if (mf != null)
		{
			if (mf.sharedMesh != null && mf.sharedMesh.name.EndsWith("(Instance)"))
			{
				mf.sharedMesh = null;
				Destroy(mf.sharedMesh);
			}
		}
#endif
        }

        /// <summary>
        /// Called when the object dies (or is returned to the pool by ObjectPool package).
        /// Removes all the Events package listeners.
        /// </summary>
        internal void RemoveAllListeners()
        {
            foreach (Type t in cachedEventListeners.Keys)
            {
                foreach (Delegate del in cachedEventListeners[t])
                {
                    HNDEvents.Instance.RemoveListener(t, del);
                }
                cachedEventListeners[t].Clear();
            }
            cachedEventListeners.Clear();
        }
        /// <summary>
        /// Called by the object pool
        /// Removes all the Events package listeners.
        /// </summary>
        public void ClearListeners()
        {
            RemoveAllListeners();
        }

        public void ForceOnDestroyCall()
        {
            OnDestroy();
        }

        public static void CleanUpHNDMonoBehaviours(GameObject go)
        {
            // Since OnDestroy only gets called when destroying objects that has previously been active, we have to call the ForceOnDestroy method, to be sure we unload everything
            HNDMonoBehaviour[] embs = go.GetComponentsInChildren<HNDMonoBehaviour>(true);
            if (embs != null && embs.Length > 0)
            {
                for (int i = 0; i < embs.Length; i++)
                {
                    if (embs[i] != null)
                        embs[i].ForceOnDestroyCall();
                }
            }
        }
    }
}