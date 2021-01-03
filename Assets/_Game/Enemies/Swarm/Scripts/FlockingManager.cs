using static Unity.Mathematics.math;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;

using System;
using Unity.Mathematics;

public class FlockingManager : MonoBehaviour
{

    static private FlockingManager _instance;
    public static FlockingManager Instance
    {
        get { return _instance; }
    }

    public struct FlockAttractor
    {
        public float3 position;
        public float radius;
        public float intensityMax;
        public float intensityMin;

    }

    public Transform respawner;

    float neighborhoodSize = 6.0f;

    //public Collider neighborhoodCollider;

    public static FlockingAgent[] agents;
    public int agentCount = 300;

    public Transform tempFollowTarget;
    //public Transform tempFollowTarget2;


    public static Collider[] overlapColliders = new Collider[100];

    static int neighBuffSize;


    public static int maxAttractors = 30;
    public NativeArray<FlockAttractor> flockAttractors;
    public int numAttractors = 1;


    public TransformAccessArray m_AgentTransformsAccessArray;
    public NativeArray<Vector3> m_agentPositionsNativeArray;
    public NativeArray<Vector3> m_agentHeadingsNativeArray;
    public NativeArray<quaternion> m_agentRotationsNativeArray;


    public NativeArray<int> m_neighborIndicesGlobalNativeArrayRead;
    public NativeArray<int> m_neighborIndicesGlobalNativeArrayWrite;

    public NativeArray<int> m_neighborIndicesGlobalNativeArrayB1;
    public NativeArray<int> m_neighborIndicesGlobalNativeArrayB2;


    public NativeArray<Vector3> m_neighborPositionsGlobalNativeArray;
    public int m_numGlobalIndices = 0;
    public int m_globalIndicesCnt = 0;

    private Transform clientJellyPopper;
    private GameObject popperHack;

    public int2[] agentNeighborArrayBounds;
    public NativeArray<int2> agentNeighborArrayBoundsNative;

    public NativeArray<Vector3> separationBehaviorResults;
    public NativeArray<Vector3> cohesionBehaviorResults;
    public NativeArray<Vector3> followBehaviorResults;
    public NativeArray<Vector3> alignmentBehaviorResults;

    public Dictionary<Collider, FlockingAgent> collidersToAgents = new Dictionary<Collider, FlockingAgent>();


    public NativeArray<RaycastCommand> m_agentRaycasts;
    public NativeArray<RaycastHit> m_agentRayhits;


    public GameObject agentPrefab;

    Transform localPlayerTransform;

    void Awake()
    {
        _instance = this;

        agents = new FlockingAgent[agentCount];

        for (int k = 0; k < agentCount; k++)
        {
            agents[k] = Instantiate(agentPrefab).GetComponent<FlockingAgent>();
        }

        if (environmentLayerMask == 0) environmentLayerMask = LayerMask.GetMask("Default");



        //hacky jelly poppings on client
         popperHack = new GameObject("ClientJellyPopper");
        popperHack.transform.parent = transform;
        Rigidbody popperBody = popperHack.AddComponent<Rigidbody>();
        popperBody.useGravity = false;
        CapsuleCollider capsule = popperHack.AddComponent<CapsuleCollider>();
        capsule.height = 2.0f;
        capsule.radius = 1.0f;
        //popperHack.layer = LayerMask.NameToLayer("ClientCollisionHack");
        clientJellyPopper = popperHack.transform;


    }

    // Use this for initialization
    void Start()
    {
        playerAttractionMultiplier = 1.0f;

        //localPlayerTransform = Camera.main.transform; //hacky hack hack

        Shader.SetGlobalFloat("_jellyDimmer", 1.0f);

        //environmentLayerMask = LayerMask.GetMask("Default");


        flockAttractors = new NativeArray<FlockAttractor>(maxAttractors, Allocator.Persistent);


        MaterialPropertyBlock props = new MaterialPropertyBlock();
        MeshRenderer renderer;

       
        //agents = FindObjectsOfType<FlockingAgent>();
        Transform[] agentTransforms = new Transform[agents.Length];

        agentNeighborArrayBounds = new int2[agents.Length];

        for (int k = 0; k < agents.Length; k++)
        {
            FlockingAgent agent = agents[k];
            {
                agent.transform.position = transform.position + new Vector3(UnityEngine.Random.value * 30.0f, UnityEngine.Random.value * 20.0f, UnityEngine.Random.value * 30.0f);
                agent.transform.localScale = Vector3.one * (0.2f * (UnityEngine.Random.value - 0.5f) + 1.0f);
                agentTransforms[k] = agent.transform;
                agent.transform.parent = transform;
                agent.agentIdx = k;
                collidersToAgents.Add(agent.GetComponent<Collider>(), agent);

                float r = UnityEngine.Random.Range(0.0f, 1.0f);
                float g = UnityEngine.Random.Range(0.9f, 1.0f);
                float b = UnityEngine.Random.Range(0.9f, 1.0f);
                props.SetColor("_Color", new Color(r, g, b));

                renderer = agent.GetComponent<MeshRenderer>();
                renderer.SetPropertyBlock(props);
            }
        }

        neighBuffSize = 100 * agents.Length;

        m_AgentTransformsAccessArray = new TransformAccessArray(agentTransforms);

        m_agentPositionsNativeArray = new NativeArray<Vector3>(agents.Length, Allocator.Persistent);
        m_agentHeadingsNativeArray = new NativeArray<Vector3>(agents.Length, Allocator.Persistent);
        m_agentRotationsNativeArray = new NativeArray<quaternion>(agents.Length, Allocator.Persistent);

        m_neighborIndicesGlobalNativeArrayB1 = new NativeArray<int>(neighBuffSize, Allocator.Persistent);
        m_neighborIndicesGlobalNativeArrayB2 = new NativeArray<int>(neighBuffSize, Allocator.Persistent);

        m_neighborIndicesGlobalNativeArrayWrite = m_neighborIndicesGlobalNativeArrayB1;
        m_neighborIndicesGlobalNativeArrayRead = m_neighborIndicesGlobalNativeArrayB2;




        neighborhoodUpdateCounter = 0;
        m_numGlobalIndices = 0;
        m_globalIndicesCnt = 0;


        m_neighborPositionsGlobalNativeArray = new NativeArray<Vector3>(neighBuffSize, Allocator.Persistent);

        agentNeighborArrayBoundsNative = new NativeArray<int2>(agents.Length, Allocator.Persistent);

        separationBehaviorResults = new NativeArray<Vector3>(agents.Length, Allocator.Persistent);
        cohesionBehaviorResults = new NativeArray<Vector3>(agents.Length, Allocator.Persistent);
        followBehaviorResults = new NativeArray<Vector3>(agents.Length, Allocator.Persistent);
        alignmentBehaviorResults = new NativeArray<Vector3>(agents.Length, Allocator.Persistent);

        m_agentRaycasts = new NativeArray<RaycastCommand>(agents.Length, Allocator.Persistent);
        m_agentRayhits = new NativeArray<RaycastHit>(agents.Length, Allocator.Persistent);

    }

    private void OnDestroy()
    {
        flockAttractors.Dispose();

        updateAgentsJobHandle.Complete();

        m_AgentTransformsAccessArray.Dispose();
        m_agentPositionsNativeArray.Dispose();
        m_agentHeadingsNativeArray.Dispose();
        m_neighborIndicesGlobalNativeArrayB1.Dispose();
        m_neighborIndicesGlobalNativeArrayB2.Dispose();
        m_neighborPositionsGlobalNativeArray.Dispose();
        agentNeighborArrayBoundsNative.Dispose();
        separationBehaviorResults.Dispose();
        cohesionBehaviorResults.Dispose();
        followBehaviorResults.Dispose();
        alignmentBehaviorResults.Dispose();
        m_agentRaycasts.Dispose();
        m_agentRayhits.Dispose();
        m_agentRotationsNativeArray.Dispose();

    }


    public JobHandle updateAgentPositionsArrayJobHandle;
    public JobHandle extractNeighborPositionsJobHandle;
    public JobHandle computeFlockingJobHandle;
    public JobHandle updateAgentsJobHandle;
    public JobHandle buildRaycastCommandsJobHandle;
    public JobHandle sensoryRaycastsJobHandle;
    public JobHandle UpdateAgentTransformsJobHandle;

    bool jobscheduled = false;

    static float tempSeparationDistance = 4.0f;
    static float tempFollowDistance = 60.0f;

    static float m_cohesionWeight = 1.0f;
    static float m_separationWeight = 1.0f;
    static float m_alignmentWeight = 1.0f;
    static float m_followWeight = 2.0f;

    int neighUpdatesPerFrame = 15;

    public LayerMask environmentLayerMask; 


    static Vector3 m_driftVector = new Vector3(0.0f, 0.0f, 0.0f);

    public void Respawn(int agentID)
    {
        agents[agentID].m_transform.position = transform.position;
    }

    public float playerAttractionMultiplier = 1.0f;
    public void ReducePlayerAttraction(GameObject other)
    {
        if (other != popperHack) return;
        playerAttractionMultiplier = -2.0f;
        Invoke("RestorePlayerAttraction", 3.0f);
    }

    void RestorePlayerAttraction()
    {
        playerAttractionMultiplier = 1.0f;
    }


    // Update is called once per frame
    void Update()
    {
        float spinTimer = Time.deltaTime * 0.5f;
        //m_driftVector = new Vector3(0.5f * cos(spinTimer), 0.2f, 0.2f * sin(spinTimer)); 

       // sensoryRaycastsJobHandle.Complete();
        updateAgentsJobHandle.Complete();
        UpdateAgentTransformsJobHandle.Complete();



        CheckSwapNeighborhoodIndexBuffers();


        //clientJellyPopper.position = Camera.main.transform.position;

        numAttractors = 2;
        flockAttractors[0] = new FlockAttractor()
        {
            position = tempFollowTarget.position,
            radius = 60,
            intensityMax = 2.0f,
            intensityMin = 2.0f
        };


       
        flockAttractors[1] = new FlockAttractor()
        {
            //position = Camera.main.transform.position,
            radius = 15.0f,
            intensityMax = 3.0f * playerAttractionMultiplier,
            intensityMin = 3.0f * playerAttractionMultiplier
        };
               //flockAttractors[2] = new FlockAttractor()
        //{
        //    position = tempFollowTarget2.position,
        //    radius = 6.0f,
        //    intensityMax = -100.0f,
        //    intensityMin = -100.0f

        //};


        UpdateAgentPositionsArrayJob updateAgentPositionsArrayJob = new UpdateAgentPositionsArrayJob()
        {
            agentPositions = m_agentPositionsNativeArray,
            agentHeadings = m_agentHeadingsNativeArray
        };

        updateAgentPositionsArrayJobHandle = updateAgentPositionsArrayJob.Schedule(m_AgentTransformsAccessArray, UpdateAgentTransformsJobHandle);

        BuildSensorRaycastCommandsJob buildSensorRaycastCommandsJob = new BuildSensorRaycastCommandsJob()
        {
            agentPositions = m_agentPositionsNativeArray,
            agentHeadings = m_agentHeadingsNativeArray,
            maxDist = 3.0f,
            envLayerMask = environmentLayerMask,
            sensorRaycasts = m_agentRaycasts
        };

        buildRaycastCommandsJobHandle = buildSensorRaycastCommandsJob.Schedule(agents.Length, 64, updateAgentPositionsArrayJobHandle);
        sensoryRaycastsJobHandle = RaycastCommand.ScheduleBatch(m_agentRaycasts, m_agentRayhits, 32, buildRaycastCommandsJobHandle);




        NativeSlice<Vector3> neighborPositionsSlice = m_neighborPositionsGlobalNativeArray.Slice<Vector3>(0, m_numGlobalIndices);
        NativeSlice<int> neighborIndicesSlice = m_neighborIndicesGlobalNativeArrayRead.Slice<int>(0, m_numGlobalIndices);


        NativeSlice<FlockAttractor> attractorSlice = flockAttractors.Slice<FlockAttractor>(0, numAttractors);

        ComputeFlockingJob computeFlockingJob = new ComputeFlockingJob()
        {
            neighborIndices = neighborIndicesSlice,
            agentBoundsIdx = agentNeighborArrayBoundsNative,
            agentPositions = m_agentPositionsNativeArray,
            agentHeadings = m_agentHeadingsNativeArray,
            oneOverSeparationDistance = 1.0f / tempSeparationDistance,
            separationDistSquared = tempSeparationDistance * tempSeparationDistance,
            separationResults = separationBehaviorResults,
            cohesionResults = cohesionBehaviorResults,
            targetEngagementDistance = tempFollowDistance,
            //followTarget = tempFollowTarget.position +Vector3.up * 2.0f,
            attractors = attractorSlice,
            followResults = followBehaviorResults,
            alignmentResults = alignmentBehaviorResults


        };

        computeFlockingJobHandle = computeFlockingJob.Schedule(agents.Length, 64, updateAgentPositionsArrayJobHandle);


        UpdateAgentsJob updateAgentsJob = new UpdateAgentsJob()
        {
            separationResults = separationBehaviorResults,
            cohesionResults = cohesionBehaviorResults,
            followResults = followBehaviorResults,
            alignmentResults = alignmentBehaviorResults,

            agentHeadings = m_agentHeadingsNativeArray,
            driftVector = m_driftVector,

            cohesionWeight = m_cohesionWeight,
            separationWeight = m_separationWeight,
            followWeight = m_followWeight,
            alignmentWeight = m_alignmentWeight,

            timeNorm = (Time.deltaTime > 0.016f ? 0.016f : Time.deltaTime) / 0.02f,

            agentRotations = m_agentRotationsNativeArray,
            agentPositions = m_agentPositionsNativeArray,

            rayHits = m_agentRayhits
      
        };

        JobHandle combinedAgentUpdateJobHandle = JobHandle.CombineDependencies(computeFlockingJobHandle, sensoryRaycastsJobHandle);

        updateAgentsJobHandle = updateAgentsJob.Schedule(agents.Length, 64,  combinedAgentUpdateJobHandle);

        

        JobHandle.ScheduleBatchedJobs();
        UpdateAgentNeighbors();


        UpdateAgentTransformsJob updateAgentTransformsJob = new UpdateAgentTransformsJob()
        {
            agentPositions = m_agentPositionsNativeArray,
            agentRotations = m_agentRotationsNativeArray
        };
        UpdateAgentTransformsJobHandle = updateAgentTransformsJob.Schedule(m_AgentTransformsAccessArray, updateAgentsJobHandle);





        


        //extractNeighborPositionsJobHandle.Complete();
        //updateAgentsJobHandle.Complete();

        //foreach (FlockingAgent agent in agents)
        //{
        //    agent.PrepareData();
        //}



        //updateAgentPositionsArrayJobHandle.Complete();
        //JobHandle.ScheduleBatchedJobs();

        //foreach (FlockingAgent agent in agents)
        //{
        //    agent.StartComputation();
        //}


    }


    public static FlockingAgent FindFlockBuddy()
    {
        return agents[(int)(UnityEngine.Random.value * (agents.Length - 1))];
    }

    //float NeighborhoodUpdateTimer = 0.0f;
    int neighborhoodUpdateCounter = 0;
  

    private void UpdateAgentNeighborhood(int k)
    {
        FlockingAgent agent = agents[k];
        agent.UpdateNeighborhood();
        agentNeighborArrayBounds[k] = new int2(agent.startInGlobalIdx, agent.m_numNeighbors);

    }


    private void CheckSwapNeighborhoodIndexBuffers()
    {
        if (neighborhoodUpdateCounter >= agents.Length)
        {
            neighborhoodUpdateCounter = 0;

            if (m_neighborIndicesGlobalNativeArrayRead.Equals(m_neighborIndicesGlobalNativeArrayB1))
            {
                m_neighborIndicesGlobalNativeArrayWrite = m_neighborIndicesGlobalNativeArrayB1;
                m_neighborIndicesGlobalNativeArrayRead = m_neighborIndicesGlobalNativeArrayB2;
            }
            else
            {
                m_neighborIndicesGlobalNativeArrayWrite = m_neighborIndicesGlobalNativeArrayB2;
                m_neighborIndicesGlobalNativeArrayRead = m_neighborIndicesGlobalNativeArrayB1;
            }
            agentNeighborArrayBoundsNative.CopyFrom(agentNeighborArrayBounds);


            m_numGlobalIndices = m_globalIndicesCnt;
            m_globalIndicesCnt = 0;

        }
    }
    private void UpdateAgentNeighbors()
    {
        int c = neighUpdatesPerFrame;
        while (c > 0 && neighborhoodUpdateCounter < agents.Length)
        {
            UpdateAgentNeighborhood(neighborhoodUpdateCounter);
            c--;
            neighborhoodUpdateCounter++;
        }

        
    }

    [BurstCompile]
    struct UpdateAgentPositionsArrayJob : IJobParallelForTransform
    {
        [WriteOnly]
        public NativeArray<Vector3> agentPositions;
        [WriteOnly]
        public NativeArray<Vector3> agentHeadings;

        public void Execute(int index, TransformAccess transform)
        {
            agentPositions[index] = transform.position;
            agentHeadings[index] = transform.rotation * Vector3.forward;
        }
    }

    [BurstCompile]
    struct UpdateAgentTransformsJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<Vector3> agentPositions;
        [ReadOnly]
        public NativeArray<quaternion> agentRotations;

        public void Execute(int index, TransformAccess transform)
        {
             transform.position = agentPositions[index];
             transform.rotation = agentRotations[index];
        }
    }


    [BurstCompile]
    struct UpdateAgentsJob : IJobParallelFor
    {
     

        [ReadOnly]
        public float cohesionWeight;
        [ReadOnly]
        public float separationWeight;
        [ReadOnly]
        public float alignmentWeight;
        [ReadOnly]
        public float followWeight;
        [ReadOnly]
        public Vector3 driftVector;

        [ReadOnly]
        public float timeNorm;

        [ReadOnly]
        public NativeArray<Vector3> separationResults;
        [ReadOnly]
        public NativeArray<Vector3> cohesionResults;
        [ReadOnly]
        public NativeArray<Vector3> followResults;
        [ReadOnly]
        public NativeArray<Vector3> alignmentResults;
        [ReadOnly]
        public NativeArray<Vector3> agentHeadings;
        [ReadOnly]
        public NativeArray<RaycastHit> rayHits;
        
        
        public NativeArray<quaternion> agentRotations;
        public NativeArray<Vector3> agentPositions;


        public void Execute(int index)
        {
            float3 heading = agentHeadings[index];


            float3 flockingVector = (separationResults[index] * separationWeight +
                cohesionResults[index] * cohesionWeight +
                followResults[index] * followWeight +
                alignmentResults[index] * alignmentWeight + driftVector).normalized;

            float rotationSpeed = 6.0f;
            float movementSpeed = 0.08f;
            float rotationFilter = 0.6f;
            //float speedColMultiplier = 1.0f;
           // bool hasCollided = false;
            Vector3 collResponse = Vector3.zero;


            if (rayHits[index].point != Vector3.zero)
            {
                flockingVector = (float3)rayHits[index].normal;
                
                //movementSpeed = 0.04f;
                
                //speedColMultiplier = 1.0f - 0.5f * (dot(rayHits[index].normal, heading) + 1.0f);
                rotationSpeed = 12.0f;
                rotationFilter = 0.5f;
                collResponse = rayHits[index].normal * 0.01f;
            }


            if (length(flockingVector) < 0.00001f)
             flockingVector = heading;

            //float3 flatHeading = heading; flatHeading.y = 0.0f;

            //quaternion uprighter = Quaternion.LookRotation(flatHeading, Vector3.up);


            //agentRotations[index] = slerp(agentRotations[index], uprighter, 0.001f);


            Quaternion steerRotation = Quaternion.LookRotation(flockingVector, mul(agentRotations[index], float3(0,1,0)));

            Quaternion newSteeringRot = Quaternion.RotateTowards(agentRotations[index], steerRotation, rotationSpeed * timeNorm);

            //todo: maybe double buffer rotations and positions and also interleave them
            agentRotations[index] = slerp(agentRotations[index], newSteeringRot, rotationFilter * timeNorm);

            Vector3 rotatedHeading = newSteeringRot * Vector3.forward;

            agentPositions[index] += rotatedHeading * movementSpeed * timeNorm + collResponse;

        }
    }

    [BurstCompile]
    public struct ExtractNeighborPositionsJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeSlice<int> neighborIndices;
        [ReadOnly]
        public NativeArray<Vector3> agentPositions;
        [WriteOnly]
        public NativeSlice<Vector3> neighborPositions;

        public void Execute(int idx)
        {
            neighborPositions[idx] = agentPositions[neighborIndices[idx]];
        }

    }

    [BurstCompile]
    public struct BuildSensorRaycastCommandsJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> agentPositions;
        [ReadOnly]
        public NativeArray<Vector3> agentHeadings;
        [ReadOnly]
        public float maxDist;
        [ReadOnly]
        public int envLayerMask;

        [WriteOnly]
        public NativeArray<RaycastCommand> sensorRaycasts;


        public void Execute(int idx)
        {
            RaycastCommand rc = new RaycastCommand();
            rc.distance = maxDist;
            rc.direction = agentHeadings[idx];
            rc.from = agentPositions[idx];
            rc.layerMask = envLayerMask;
            rc.maxHits = 1;
           
            sensorRaycasts[idx] = rc;
            


        }

    }

        [BurstCompile]
    public struct ComputeFlockingJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeSlice<int> neighborIndices;
        [ReadOnly]
        public NativeArray<int2> agentBoundsIdx;
        [ReadOnly]
        public NativeArray<Vector3> agentPositions;
        [ReadOnly]
        public NativeArray<Vector3> agentHeadings;
        [ReadOnly]
        public float separationDistSquared;
        [ReadOnly]
        public float oneOverSeparationDistance;
        [ReadOnly]
        public float3 followTarget;
        [ReadOnly]
        public float targetEngagementDistance;
        [ReadOnly]
        public NativeSlice<FlockAttractor> attractors;



        [WriteOnly]
        public NativeArray<Vector3> separationResults;
        [WriteOnly]
        public NativeArray<Vector3> cohesionResults;
        [WriteOnly]
        public NativeArray<Vector3> followResults;
        [WriteOnly]
        public NativeArray<Vector3> alignmentResults;


        public void Execute(int index)
        {

            int2 thisAgent = agentBoundsIdx[index];

            if (thisAgent.y < 1)
            {
                separationResults[index] = new Vector3(0,0,0);
                cohesionResults[index] = new Vector3(0, 0, 0);
                return;
            }

            int end = thisAgent.x + thisAgent.y;
            float3 agentPos = agentPositions[index];
            float3 res = float3(0);
            float3 centroid = float3(0);
            float3 alignmentHeading = float3(0);
            for (int k = thisAgent.x; k < end; k++)
            {
                //float3 neighPos = neighborPositions[k];
                int neighIdx = neighborIndices[k];
                float3 neighPos = agentPositions[neighIdx];
                float3 neighHeading = agentHeadings[neighIdx];
                alignmentHeading = alignmentHeading + neighHeading;

                centroid = centroid + neighPos;
                float3 dist = agentPos - neighPos;
                float distSq = lengthsq(dist);
                if (distSq < separationDistSquared)
                {
                    float magn = Mathf.Sqrt(distSq);
                    float sep = 1.0f - (magn * oneOverSeparationDistance);

                    res = res + (sep * dist / magn);
                }
            }

            centroid = centroid / (float)thisAgent.y;

            cohesionResults[index] = normalize(centroid - agentPos);
            separationResults[index] = res;

            alignmentResults[index] = normalize(alignmentHeading);// + 10.0f * float3(0,1,0) * (Mathf.PerlinNoise(0.3f * agentPos.x, 0.3f * agentPos.z) - 0.5f);

            float3 attractionResults = float3(0);

            //optimize this using lengthsq, sqrt and use computed length for normalization
            for (int k = 0; k < attractors.Length; k++)
            {
                float3 vectToTarget = attractors[k].position - agentPos;
                float targetMagn = length(vectToTarget);

                attractionResults += targetMagn > attractors[k].radius ?
                    new float3(0, 0, 0) :
                    normalize(vectToTarget) * lerp(attractors[k].intensityMin, attractors[k].intensityMax, 1.0f - targetMagn/ attractors[k].radius);
            }


            followResults[index] = attractionResults;



        }
    }



}
