using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class ChangeMaterialAction : HNDAction
    {
        [Header("'Change Material' Specific Settings")]
        [SerializeField]
        private GameObject m_TargetObject;
        [SerializeField]
        private int m_MaterialIdOnTarget = 0;
        [SerializeField]
        private Material m_ChangeToMaterial;

        private Material m_OriginalMaterial;

        protected override void Awake()
        {
            base.Awake();

            m_OriginalMaterial = m_TargetObject.GetComponent<Renderer>().material;
        }

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            Renderer r = m_TargetObject.GetComponent<Renderer>();
            if (r != null)
            {
                if (m_MaterialIdOnTarget == 0 && r.materials.Length == 1)
                    r.material = m_ChangeToMaterial;
                else
                {
                    Material[] mats = r.materials;
                    mats[m_MaterialIdOnTarget] = m_ChangeToMaterial;
                    r.materials = mats;
                }
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);

            Renderer r = m_TargetObject.GetComponent<Renderer>();
            if (r != null)
            {
                if (m_MaterialIdOnTarget == 0 && r.materials.Length == 1)
                    m_TargetObject.GetComponent<Renderer>().material = m_OriginalMaterial;
                else
                {
                    Material[] mats = r.materials;
                    mats[m_MaterialIdOnTarget] = m_OriginalMaterial;
                    r.materials = mats;
                }
            }
        }
    }
}