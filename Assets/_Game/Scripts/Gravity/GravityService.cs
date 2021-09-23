using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hedronoid
{
    public static class GravityService
    {
        public static Vector3 CurrentGravity { get; set; }
        static List<GravitySource> sources = new List<GravitySource>();
        static float lastTimestampPrioritization = 0f;
        public static List<GravitySource> GetActiveGravitySources()
        {
            List<GravitySource> activeGravities = new List<GravitySource>();

            foreach (GravitySource gs in sources)
                if (gs.IsPlayerInGravity)
                    activeGravities.Add(gs);

            return activeGravities;
        }

        public static int GetMaxPriorityWeight()
        {
            List<GravitySource> activeGravities = GetActiveGravitySources();

            int maxWeight = 0;
            foreach (GravitySource gs in activeGravities)
                if (gs.CurrentPriorityWeight > maxWeight)
                    maxWeight = gs.CurrentPriorityWeight;

            return maxWeight;
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

        public static void PrioritizeActiveOverlappedGravities(Vector3 position, Vector3 moveDir)
        {
            Dictionary<Vector3, List<GravitySource>> filteredSrcs = FilterEmbeddedGravities();

            List<GravitySource> activeGravities = new List<GravitySource>();

            if (filteredSrcs.Count == 1 && filteredSrcs.First().Key != Vector3.zero)
            {
                activeGravities = filteredSrcs.First().Value;
            }
            else if (filteredSrcs.ContainsKey(Vector3.zero))
            {
                activeGravities = filteredSrcs[Vector3.zero];
            }
            else
            {
                Vector3 key = filteredSrcs.First().Key;
                float distance = Vector3.Distance(key, position);
                Vector3 candidate = key;

                foreach (Vector3 srcPos in filteredSrcs.Keys)
                {
                    if (srcPos == key) break;

                    float curr = Vector3.Distance(srcPos, position);
                    if (distance > curr)
                    {
                        distance = curr;
                        candidate = srcPos;
                    }
                }

                if (candidate != Vector3.zero)
                    activeGravities = filteredSrcs[candidate];
            }

            List<Vector3> activeGrvPositions = new List<Vector3>();
            Dictionary<GravitySource, float> srcAngleDictionary = new Dictionary<GravitySource, float>();
            Dictionary<GravitySource, float> srcDistanceDictionary = new Dictionary<GravitySource, float>();

            if (activeGravities.Count > 1)
            {
                foreach (GravitySource gs in activeGravities)
                {
                    Vector3 playerToGravityDir = (gs.transform.position - position).normalized;
                    float angle = Vector3.Angle(playerToGravityDir, moveDir);
                    float distance = Vector3.Distance(gs.transform.position, position);

                    activeGrvPositions.Add(gs.transform.position);
                    if (!srcAngleDictionary.ContainsKey(gs))
                        srcAngleDictionary.Add(gs, angle);
                    if (!srcDistanceDictionary.ContainsKey(gs))
                        srcDistanceDictionary.Add(gs, distance);
                }

                List<GravitySource> srcs = srcAngleDictionary.Keys.ToList();
                List<float> angleVals = srcAngleDictionary.Values.ToList();
                List<float> distanceVals = srcDistanceDictionary.Values.ToList();

                GravitySource l = srcs[0];

                if (activeGrvPositions.Any(o => o != activeGrvPositions[0]))
                {
                    // This prevents the update priority to execute on each frame. 
                    // WHY: Jittering between gravities bug. When we are between overlapping 
                    // gravities the movement direction rotates to accomodate for the new gravity. 
                    // By doing so it made a small angle and overrode the priority system by reverting 
                    // the gravity back to the previous one.
                    // By constraining the execution step (with a sweetspot of 0.2s), we avoid this issue (so far).

                    if (Time.realtimeSinceStartup - lastTimestampPrioritization <= 0.2f) return;
                    else lastTimestampPrioritization = Time.realtimeSinceStartup;
                    //
                    if (moveDir == null || moveDir == Vector3.zero) return;

                    if (angleVals.Any(o => o != angleVals[0]))
                    {
                        GravitySource r;
                        for (int i = 1; i < srcs.Count; ++i)
                        {
                            r = srcs[i];
                            if (srcAngleDictionary[l] > srcAngleDictionary[r])
                                l = r;
                        }
                    }
                    else if (distanceVals.Any(o => o != distanceVals[0]))
                    {
                        GravitySource r;
                        for (int i = 1; i < srcs.Count; ++i)
                        {
                            r = srcs[i];
                            if (srcDistanceDictionary[l] > srcDistanceDictionary[r])
                                l = r;
                        }
                    }
                }
                else
                {
                    l = PrioritizeEmbeddedGravities(position, activeGravities);
                }

                //Debug.LogErrorFormat("UNO:  SRC {0} has angle with player {1}. And this is src: {2}",
                //    srcs[0].name, srcAngleDictionary[srcs[0]], name);
                //Debug.LogErrorFormat("DUE:  SRC {0} has angle with player {1}. And this is src: {2}",
                //    srcs[1].name, srcAngleDictionary[srcs[1]], name);

                //Debug.LogErrorFormat("MIN>>>>>SRC {0} has angle with player {1}. And this is src: {2}",
                //    l.name, srcAngleDictionary[l], name);

                l.CurrentPriorityWeight = activeGravities.Count + 1;

                foreach (GravitySource gs in activeGravities)
                    if (gs.CurrentPriorityWeight > 1)
                        gs.CurrentPriorityWeight--;
            }
            else if (activeGravities.Count == 1)
            {
                if (GetMaxPriorityWeight() > 1)
                    activeGravities[0].CurrentPriorityWeight = GetMaxPriorityWeight();
                else activeGravities[0].CurrentPriorityWeight = GetMaxPriorityWeight() + 1;

                foreach (GravitySource gs in activeGravities[0].OverlappingSources)
                    gs.CurrentPriorityWeight = 1;
            }
        }

        public static GravitySource PrioritizeEmbeddedGravities(Vector3 position, List<GravitySource> sources)
        {
            List<float> colliderSizes = new List<float>();

            foreach (GravitySource src in sources)
            {
                if (src.TriggerCol && src.TriggerCol.isTrigger)
                {
                    //Debug.LogErrorFormat("SRC NAME: {0}, BOUNDS: {1}", src.name, src.TriggerCol.bounds.size.sqrMagnitude);
                    colliderSizes.Add(src.TriggerCol.bounds.size.sqrMagnitude);
                }
                else D.GravError("One of the embedded gravities doesn't have a trigger collider attached.");
            }

            float smallest = colliderSizes[0];

            for (int i = 1; i < colliderSizes.Count; ++i)
                if (smallest > colliderSizes[i])
                {
                    smallest = colliderSizes[i];
                }

            return sources[colliderSizes.FindIndex(o => o == smallest)];
        }
        public static Dictionary<Vector3, List<GravitySource>> FilterEmbeddedGravities()
        {
            List<GravitySource> activeGravities = GetActiveGravitySources();
            List<GravitySource> zeroList = new List<GravitySource>();
            List<GravitySource> nonZeroList = new List<GravitySource>();
            Dictionary<Vector3, List<GravitySource>> dict = new Dictionary<Vector3, List<GravitySource>>();

            bool notEqual = false;

            if (activeGravities.Count == 0) return dict;

            if (activeGravities.Count > 1)
            {
                for (int i = 0; i < activeGravities.Count; i++)
                {
                    notEqual = false;
                    for (int j = 0; j < activeGravities.Count; j++)
                    {
                        if (activeGravities[i] == activeGravities[j]) continue;

                        if (activeGravities[i].transform.position ==
                            activeGravities[j].transform.position)
                        {
                            notEqual = false;
                            nonZeroList.Add(activeGravities[j]);
                        }
                        else
                        {
                            notEqual = true;
                        }
                    }

                    if (notEqual && !zeroList.Contains(activeGravities[i]) &&
                        !nonZeroList.Contains(activeGravities[i]))
                    {
                        zeroList.Add(activeGravities[i]);
                        continue;
                    }
                    else if (!notEqual && !nonZeroList.Contains(activeGravities[i]) &&
                        !zeroList.Contains(activeGravities[i]))
                    {
                        nonZeroList.Add(activeGravities[i]);
                    }

                    if (!dict.ContainsKey(activeGravities[i].transform.position))
                        dict.Add(activeGravities[i].transform.position, nonZeroList);
                }
            }
            else zeroList.Add(activeGravities[0]);


            if (zeroList.Count > 0)
            {
                dict.Add(Vector3.zero, zeroList);
            }
            return dict;
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
