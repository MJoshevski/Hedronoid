using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Collections;
using Hedronoid.Objects;
using Hedronoid;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{

    public class HerdManager : HNDGameObject
    {
        private static List<HerdManager> m_Flocks = null;
        [SerializeField]
        private bool m_SubscribeToGameObject = true;
        [SerializeField]
        private string m_FlockName = "";
        [SerializeField]
        private string m_FlockType = "Rollandians";
        [SerializeField]
        private List<string> m_FlocksTypeToInteractWith = new List<string>();
        [SerializeField]
        private bool m_SetInInspector = false;
        [Header("Inspector set up")]
        [SerializeField]
        private List<GameObject> m_FlockMembers = new List<GameObject>();
        [Header("Spawn set up")]
        [SerializeField]
        private GameObject m_FlockMemberPrefab;
        [SerializeField]
        private Vector3 m_FlockSpawnAreaSize = new Vector3(5f, 2f, 5f);
        [Header("Goal")]
        [SerializeField]
        private bool m_UpdateGoalFromCenter = true;
        [SerializeField]
        private Vector3 m_GoalPos = Vector3.zero;
        [SerializeField]
        private float m_GoalDirectionDistance = 5f;
        [SerializeField]
        private bool m_UpdateGoalWithEffects = false;

        private List<FlockAttract> m_Attract = new List<FlockAttract>();
        private float m_AttractIntensity = 0f;
        private List<FlockRepel> m_Repel = new List<FlockRepel>();
        private float m_RepelIntensity = 0f;
        [SerializeField]
        private List<BaseInteractable> m_Interact = new List<BaseInteractable>();


        public Vector3 GoalPos
        {
            get { return m_GoalPos; }
            set { m_GoalPos = value; }
        }

        public List<GameObject> FlockMembers
        {
            get { return m_FlockMembers; }
            set { m_FlockMembers = value; }
        }

        public string FlockName
        {
            get { return m_FlockName; }
            set { m_FlockName = value; }
        }

        public static List<HerdManager> Flocks
        {
            get
            {
                if (m_Flocks == null)
                {
                    Debug.Log("Setting up flocks");
                    m_Flocks = new List<HerdManager>();
                    HerdManager[] managers = (HerdManager[])GameObject.FindObjectsOfType(typeof(HerdManager));
                    for (int i = 0; i < managers.Length; i++)
                        m_Flocks.Add(managers[i]);
                }
                return m_Flocks;
            }

            set { m_Flocks = value; }
        }

        public string FlockType
        {
            get { return m_FlockType; }
            set { m_FlockType = value; }
        }

        public List<string> FlocksTypeToInteractWith
        {
            get { return m_FlocksTypeToInteractWith; }
            set { m_FlocksTypeToInteractWith = value; }
        }

        public List<FlockAttract> Attract
        {
            get { return m_Attract; }
            set { m_Attract = value; }
        }

        public List<FlockRepel> Repel
        {
            get { return m_Repel; }
            set { m_Repel = value; }
        }

        public float AttractIntensity
        {
            get { return m_AttractIntensity; }
            set { m_AttractIntensity = value; }
        }

        public float RepelIntensity {
            get { return m_RepelIntensity; }
            set { m_RepelIntensity = value; }
        }

        public bool UpdateGoalFromCenter {
            get { return m_UpdateGoalFromCenter; }
            set { m_UpdateGoalFromCenter = value; }
        }

        public Vector3 FlockSpawnAreaSize {
            get { return m_FlockSpawnAreaSize; }
            set { m_FlockSpawnAreaSize = value; }
        }

        public float GoalDirectionDistance {
            get { return m_GoalDirectionDistance; }
            set { m_GoalDirectionDistance = value; }
        }

        public List<BaseInteractable> Interact {
            get { return m_Interact; }
            set { m_Interact = value; }
        }
        
    }
}