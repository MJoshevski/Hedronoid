using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid.Fire;
using Hedronoid;
using Hedronoid.Events;

namespace Hedronoid.AI
{

    public class NPC : HNDGameObject
    {
        public enum NPCType { Rollandian, Zombie, Firellandian, Firembie, Bunny }

        [SerializeField]
        private NPCType m_NpcType;

        [SerializeField]
        private Renderer m_HeadGraphics;
        [SerializeField]
        private Renderer m_BodyGraphics;
        [SerializeField]
        private Renderer m_BodyLowLod;
        [SerializeField]
        private Renderer m_MaleBodyLowLod;

        [Header("Rollandian Settings: ")]
        [SerializeField]
        private Material [] m_RollMaterialBody;
        [SerializeField]
        private Material[] m_RollMaterialHead;
        [SerializeField]
        private Material[] m_RollLodMaterial;
        [SerializeField]
        private Material[] m_MaleLodMaterial;
        [SerializeField]
        private float m_RCoherenceFactor = 0.6f;

        [Header("Zombie Settings: ")]
        [SerializeField]
        private Material [] m_ZombieMaterialBody;
        [SerializeField]
        private Material [] m_ZombieMaterialHead;
        [SerializeField]
        private Material[] m_ZombieLodMaterial;
        [SerializeField]
        private Material[] m_MaleZombieLodMaterial;
        [SerializeField]
        private float m_ZCoherenceFactor = 1.2f;

        [Header("Conversion Settings: ")]
        [SerializeField]
        [Tooltip("How many seconds does it take to convert the NPC from one type to another?")]
        private float m_conversionTime = 3;
        [SerializeField]
        private GameObject m_conversionEffect;

        [SerializeField]
        private ParticleSystem m_CivilianConversionParticlePrefab;
        [SerializeField]
        private ParticleSystem m_ZombieConversionParticlePrefab;
        [SerializeField]
        private ParticleSystem m_BlockheadConversionParticlePrefab;
        [SerializeField]
        private bool m_ForceFemale = false;
        private bool m_Male = false;

        private HerdMemberNavigation m_herd;
        // private RollHerdNavigation2 m_herd2;

        public NPCType NpcType
        {
            get
            {
                return m_NpcType;
            }
        }

      

        /// <summary>
        /// Thi
        /// </summary>
        public void ConvertNpc(NPCType newType)
        {
            if (m_NpcType == NPCType.Zombie && newType == NPCType.Rollandian)
            {
                // HNDEvents.Instance.Raise(new PlayAudio("Statue_Zombie2Rollandian"));

                var civPart = Instantiate(m_CivilianConversionParticlePrefab, this.transform.position, Quaternion.identity);
                Destroy(civPart.gameObject, 10f);
            }
            else if (m_NpcType == NPCType.Rollandian && newType == NPCType.Zombie)
            {
                var zombiePart = Instantiate(m_ZombieConversionParticlePrefab, this.transform.position, Quaternion.identity);
                Destroy(zombiePart.gameObject, 10f);
            }
            else if (newType == NPCType.Firellandian || newType == NPCType.Firembie)
            {
                // HNDEvents.Instance.Raise(new PlayAudio("Fire_CatchFire"));
            }

            StartCoroutine(DoConversion(newType));
        }

        IEnumerator DoConversion(NPCType newType)
        {
            // var motor = GetComponent<RollHerdMotor>();
            // if (motor) motor.enabled = false;

            // if (m_conversionEffect)
            // {
            //     HNDEvents.Instance.Raise(new InstantiateTimedPooledObject(m_conversionEffect, m_conversionTime, transform.position, transform.rotation));
            // }

            yield return new WaitForSeconds(m_conversionTime);
            ChangeNpcType(newType);
            
            // if (motor) motor.enabled = true;
        }


        public void ChangeNpcType(NPCType newType)
        {

            switch (newType)
            {
                case NPCType.Rollandian:
                if(m_herd)
                    m_herd.ChangeType(newType, m_RCoherenceFactor);
                    if (m_BodyGraphics)
                    {
                        m_BodyGraphics.materials = m_RollMaterialBody;
                    }
                    if (m_HeadGraphics)
                    {
                        m_HeadGraphics.materials = m_RollMaterialHead;
                    }
                    if (m_BodyLowLod)
                    {
                        m_BodyLowLod.materials = m_RollLodMaterial;
                        if (m_MaleBodyLowLod)
                            m_MaleBodyLowLod.materials = m_MaleLodMaterial;
                    }
                    break;

                case NPCType.Zombie:
                    if (m_herd)
                    {
                        m_herd.ChangeType(newType, m_ZCoherenceFactor);
                    }

                    if (m_BodyGraphics)
                    {
                        m_BodyGraphics.materials = m_ZombieMaterialBody;
                    }
                    if (m_HeadGraphics)
                    {
                        m_HeadGraphics.materials = m_ZombieMaterialHead;
                    }
                    if (m_BodyLowLod)
                    {
                        m_BodyLowLod.materials = m_ZombieLodMaterial;
                        if(m_MaleBodyLowLod)
                            m_MaleBodyLowLod.materials = m_MaleZombieLodMaterial;
                    }

                    // AutoSwordSlash ass = GetComponent<AutoSwordSlash>();
                    // if (ass)
                    //     ass.StopSlash();

                    break;

                case NPCType.Firellandian:
                
                    if (m_herd)
                    {
                        m_herd.ChangeType(newType, m_RCoherenceFactor);
                    }

                    if (m_BodyGraphics)
                    {
                        m_BodyGraphics.materials = m_RollMaterialBody;
                    }
                    if (m_HeadGraphics)
                    {
                        m_HeadGraphics.materials = m_RollMaterialHead;
                    }
                    break;

                case NPCType.Firembie:
                    if (m_herd)
                        m_herd.ChangeType(NPCType.Zombie, m_ZCoherenceFactor); // for now firembies and zombies behave the same

                    if (m_BodyGraphics)
                    {
                        m_BodyGraphics.materials = m_ZombieMaterialBody;
                    }
                    if (m_HeadGraphics)
                    {
                        m_HeadGraphics.materials = m_ZombieMaterialHead;
                    }
                    
                    // AutoSwordSlash ass = GetComponent<AutoSwordSlash>();
                    // if (ass)
                    //     ass.StopSlash();

                    break;
            }
            m_NpcType = newType;
        }

        protected override void Start()
        {
            base.Start();
            m_Male = !m_ForceFemale && m_MaleBodyLowLod && Random.Range(0f, 1f) > 0.5f;
            if (!m_herd)
                m_herd = GetComponent<HerdMemberNavigation>();
            
            // if (!m_herd2)
            //     m_herd2 = GetComponent<RollHerdNavigation2>();

            if (m_Male)
            {
                if (m_MaleBodyLowLod)
                    m_MaleBodyLowLod.gameObject.SetActive(true);
                if (m_BodyLowLod)
                    m_BodyLowLod.gameObject.SetActive(false);
            } else
            {
                if (m_MaleBodyLowLod)
                    m_MaleBodyLowLod.gameObject.SetActive(false);
                if (m_BodyLowLod)
                    m_BodyLowLod.gameObject.SetActive(true);

            }
            ChangeNpcType(m_NpcType);
        }

        protected override void Awake()
        {
            base.Awake();
            HNDEvents.Instance.AddListener<OnCatchFire>(OnCatchFire);
            HNDEvents.Instance.AddListener<OnPutOutFire>(OnPutOutFire);
        }

        void OnCatchFire (OnCatchFire e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;

            switch (m_NpcType)
            {
                case NPCType.Rollandian:
                    ChangeNpcType(NPCType.Firellandian);
                    break;
                case NPCType.Zombie:
                    ChangeNpcType(NPCType.Firembie);
                    break;
            }
        }

        void OnPutOutFire(OnPutOutFire e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;

            switch (m_NpcType)
            {
                case NPCType.Rollandian:
                    ChangeNpcType(NPCType.Firellandian);
                    break;
                case NPCType.Zombie:
                    ChangeNpcType(NPCType.Firembie);
                    break;
                case NPCType.Firellandian:
                    ChangeNpcType(NPCType.Rollandian);
                    break;
                case NPCType.Firembie:
                    ChangeNpcType(NPCType.Zombie);
                    break;
            }

        }

    }

}
