using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public static class GravityService
    {
        public static Vector3 CurrentGravity { get; set; }
        static List<GravitySource> sources = new List<GravitySource>();

        public static List<GravitySource> GetActiveGravitySources()
        {
            List<GravitySource> activeGravities = new List<GravitySource>();

            foreach (GravitySource gs in sources)
                if (gs.IsPlayerInGravity)
                    activeGravities.Add(gs);

            return activeGravities;
        }

        public static Vector3 GetGravity(Vector3 position)
        {
            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            return g;
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
        {
            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            upAxis = -g.normalized;
            return g;
        }

        public static Vector3 GetUpAxis(Vector3 position)
        {
            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            return -g.normalized;
        }

        public static Vector3 GetForwardAxis(Vector3 position)
        {
            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            return new Vector3(0, 0, g.normalized.z);
        }

        public static Vector3 GetRightAxis(Vector3 position)
        {
            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            return new Vector3(g.normalized.x, 0, 0);
        }

        public static void Register(GravitySource source)
        {
            Debug.Assert(
            !sources.Contains(source),
            "Duplicate registration of gravity source!", source);
            sources.Add(source);
        }

        public static void Unregister(GravitySource source)
        {
            Debug.Assert(
            sources.Contains(source),
            "Unregistration of unknown gravity source!", source);

            sources.Remove(source);
        }    
    }
}
