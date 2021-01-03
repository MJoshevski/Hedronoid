using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;

using Hedronoid.Particle;
using Hedronoid.Audio;

public class FlockingAgent : MonoBehaviour
{

    public Transform m_transform;

    //public List<FlockingAgent> m_flockNeighbors;

    public FlockingAgent[] m_flockNeighbors;
    public Transform[] m_flockNeighborTransforms = new Transform[100];
    public TransformAccessArray m_TransformsAccessArray;
    public int m_numNeighbors = 0;

    public int agentIdx = 0;

    public Vector3[] m_neighborPositions = new Vector3[100];
    public int[] m_neighborIndices = new int[100];
    public NativeArray<int> m_neighborIndicesNativeArray;

    public NativeArray<Vector3> m_neighborPositionsNative;


    [HideInInspector]
    public FlockingAgent m_fallBackNeighbor;


    public FlockingBehaviorBlock m_behaviorBlock;

    private SphereCollider m_neighborhoodCollider;
    LayerMask swarmOnly;

    float m_neighborhoodRadius = 10.0f;

    public Vector3 Position = Vector3.zero;
    public Vector3 Velocity = Vector3.zero;

    public Vector3 Forward = Vector3.forward;
    public float oneOverNeighbourCount = 1.0f;

    public NativeArray<Vector3> separationVects;
    public NativeArray<Vector3> sepResults;

    public Vector3 externalForces = Vector3.zero;

    //{
    //    get { return m_transform.position;}
    //    set { m_transform.position = value; }
    //}


    //[SerializeField]
    //private ParticleList.ParticleSystems m_PopVFX = ParticleList.ParticleSystems.PRT_JELLYPOP;

    //[SerializeField]
    //private AudioList.Sounds m_PopSFX = AudioList.Sounds.GEN_JELLYFISH_ATTACK;

    private void Awake()
    {
        m_transform = this.transform;
        m_neighborhoodCollider = gameObject.AddComponent<SphereCollider>();
        m_neighborhoodCollider.radius = 0.5f;
        this.gameObject.layer = LayerMask.NameToLayer("Flockers");
        swarmOnly = LayerMask.GetMask("Flockers");

        if (m_behaviorBlock != null)
        {
            m_behaviorBlock = Object.Instantiate(m_behaviorBlock);
            m_behaviorBlock.Owner = this;
        }

        //TransformAccessArray.Allocate(100, 1, out m_TransformsAccessArray);

        m_neighborPositionsNative = new NativeArray<Vector3>(100, Allocator.Persistent);
        m_neighborIndicesNativeArray = new NativeArray<int>(100, Allocator.Persistent);

        separationVects = new NativeArray<Vector3>(100, Allocator.Persistent);
        sepResults = new NativeArray<Vector3>(1, Allocator.Persistent);

    }



    // Use this for initialization
    void Start () 
    {
        //InvokeRepeating("UpdateNeighborhood", 0.0f, 0.1f);
        isUpdateScheduled = false;
        fm = FlockingManager.Instance;
    }


    bool isUpdateScheduled = false;

    public void OnCollisionEnter(Collision collision)
    {
        
        //ParticleHelper.PlayParticleSystem(m_PopVFX, m_transform.position - m_transform.forward * 1.0f, m_transform.forward, 1.0f, false);
        //AudioHelper.PlayAudioOnPosition(m_PopSFX, m_transform.position);

        fm.ReducePlayerAttraction(collision.gameObject);
        fm.Respawn(agentIdx);

    }


    public void PrepareData()
    {

        return;
        //ExtractNeighborPositionsJob extractNeighborPositionsJob = new ExtractNeighborPositionsJob()
        //{
        //    neighborIndices = m_neighborIndicesNativeArray.Slice<int>(0, m_numNeighbors),
        //    neighborPositions = m_neighborPositionsNative.Slice<Vector3>(0, m_numNeighbors),
        //    agentPositions = FlockingManager.Instance.m_agentPositionsNativeArray
        //};

        //FlockingManager.Instance.updateAgentPositionsArrayJobHandle.Complete();
        //extractNeighborPositionsJobHandle = extractNeighborPositionsJob.Schedule<ExtractNeighborPositionsJob>(m_numNeighbors, 16, FlockingManager.Instance.updateAgentPositionsArrayJobHandle);
        //extractNeighborPositionsJobHandle = extractNeighborPositionsJob.Schedule<ExtractNeighborPositionsJob>(FlockingManager.Instance.updateAgentPositionsArrayJobHandle);

    }

    // Update is called once per frame
    public void StartComputation () 
    {
        //if (isUpdateScheduled)
        //{
        //    UpdateResults();
        //}
        //else
        //{
        //    UpdateNeighborhood();
        //}


        //externalForces = Vector3.zero;
        Forward = m_transform.forward;




        //for (int k = 0; k < m_numNeighbors; k++)
        //{
        //    m_neighborPositionsNative[k] = m_flockNeighbors[k].Position;
        //}

        //extractNeighborPositionsJobHandle.Complete();

       // m_behaviorBlock.Update();
        //isUpdateScheduled = true;



   
	}

    public JobHandle extractNeighborPositionsJobHandle;


    public void UpdateResults()
    {
        //extractNeighborPositionsJobHandle.Complete();
        //m_behaviorBlock.GetResults();

        //if (m_behaviorBlock.steering.sqrMagnitude < 0.000001f) m_behaviorBlock.steering = Forward;

        ////Debug.DrawRay(Position, 5.0f * m_behaviorBlock.steering, Color.blue);
        ////Debug.DrawRay(Position, 5.0f * transform.forward, Color.red);


        ////Quaternion uprighter = Quaternion.LookRotation(Vector3.ProjectOnPlane(Forward, Vector3.up), Vector3.up);

        ////m_transform.rotation = Quaternion.Slerp(m_transform.rotation, uprighter, 0.001f); ;

        //Quaternion steerRotation = Quaternion.LookRotation(m_behaviorBlock.steering, m_transform.up);

        //Quaternion newSteeringRot = Quaternion.RotateTowards(m_transform.rotation, steerRotation, 3.0f);

        //m_transform.rotation = Quaternion.Slerp(m_transform.rotation, newSteeringRot, 0.5f);





        ////Position += m_behaviorBlock.steering * 0.05f;
        ////Position += Vector3.ClampMagnitude(m_behaviorBlock.displacement, 0.05f);
        Position += m_transform.forward * 0.03f;


        m_transform.position = Position;

      
    }

    List<FlockingAgent> tempNeighbors = new List<FlockingAgent>();

    private void OnDestroy()
    {
        m_neighborPositionsNative.Dispose();
        separationVects.Dispose();
        sepResults.Dispose();
        m_neighborIndicesNativeArray.Dispose();
    }

    public int startInGlobalIdx = 0;

    FlockingManager fm;


    public void UpdateNeighborhood()
    {
        //tempNeighbors.Clear();



        //int numOverlaps = Physics.OverlapSphereNonAlloc(fm.m_agentPositionsNativeArray[agentIdx], m_neighborhoodRadius, FlockingManager.overlapColliders, swarmOnly);
        int numOverlaps = Physics.OverlapSphereNonAlloc(m_transform.position, m_neighborhoodRadius, FlockingManager.overlapColliders, swarmOnly);


        m_numNeighbors = 0;
        Collider neighCol = (Collider)(m_neighborhoodCollider);

        startInGlobalIdx = fm.m_globalIndicesCnt;
        if (numOverlaps > 0)
        {
            for (int k = 0; k < numOverlaps; k++)
            {
                Collider c = FlockingManager.overlapColliders[k];
                if (c != neighCol)
                {
                    m_fallBackNeighbor = fm.collidersToAgents[c]; //c.gameObject.GetComponent<FlockingAgent>();
                    //tempNeighbors.Add(newNeighbor);
                    //fm.m_neighborIndices[fm.m_numGlobalIndices] = newNeighbor.agentIdx;
                    fm.m_neighborIndicesGlobalNativeArrayWrite[fm.m_globalIndicesCnt] = m_fallBackNeighbor.agentIdx;

                    fm.m_globalIndicesCnt++;
                    m_numNeighbors++;
                }
            }
            //m_flockNeighbors = tempNeighbors.ToArray();
            //m_neighborIndicesNativeArray.CopyFrom(m_neighborIndices);

        }


           // if (m_numNeighbors > 0) m_fallBackNeighbor = m_flockNeighbors[0];
            if (m_numNeighbors > 0) oneOverNeighbourCount = 1.0f / (float)m_numNeighbors;

            if (m_numNeighbors > 0) return;
            // m_numNeighbors = m_flockNeighbors.Length;


            if (m_fallBackNeighbor == null) m_fallBackNeighbor = FlockingManager.FindFlockBuddy();

            fm.m_neighborIndicesGlobalNativeArrayWrite[fm.m_globalIndicesCnt] = m_fallBackNeighbor.agentIdx;
            fm.m_globalIndicesCnt++;
            m_numNeighbors=1;

            //if (m_TransformsAccessArray.length > 0) m_TransformsAccessArray.RemoveAtSwapBack(0);

            //for (int k = 0; k < m_numNeighbors; k++)
            //{
            //    //m_TransformsAccessArray.Add(m_flockNeighbors[k].m_transform);
            //    m_flockNeighborTransforms[k] = m_flockNeighbors[k].m_transform;
            //}

            //if (m_TransformsAccessArray.isCreated) m_TransformsAccessArray.Dispose();
            //m_TransformsAccessArray = new TransformAccessArray(m_flockNeighborTransforms, 1);

            //Debug.Log(this.name + " : " + m_TransformsAccessArray.length);





    }





}
