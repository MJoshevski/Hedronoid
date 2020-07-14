using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public interface IGravityService
    {
        float GravityAmount { get; }
        Vector3 GravityDirection { get; }
        Quaternion GravityRotation { get; }
        Vector3 GravityUp { get; }
        Vector3 CurrentGravity { get; set; }
    }

    public class GravityService : MonoSingleton<IGravityService>, IGravityService
    {
        public Transform playerInputSpace;
        public Vector3 CurrentGravity { get; set; }
        public float GravityAmount { get { return 9.81f; } }
        static List<GravitySource> sources = new List<GravitySource>();

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

        public Vector3 GravityDirection { get; private set; }
        public Quaternion GravityRotation { get; private set; }
        public Vector3 GravityUp
        {
            get
            {
                return GravityDirection * -1;
            }
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
