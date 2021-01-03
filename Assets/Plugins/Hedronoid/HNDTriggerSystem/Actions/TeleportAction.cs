using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class TeleportAction : HNDAction
    {

        [Header("'Teleport' Specific Settings")]
        [SerializeField]
        private bool m_TeleportCollidingObject;
        [SerializeField]
        private Transform m_TeleportObject;
        [SerializeField]
        private Transform m_TeleportTarget;
        [SerializeField]
        private ParticleSystem m_ParticleOnTeleport;

        private Vector3 m_EnterPosition;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            if (m_TeleportCollidingObject)
            {

                if (m_ParticleOnTeleport != null)
                {
                    m_ParticleOnTeleport.transform.position = triggeringObject.transform.position;
                    // If you don't have the GameObjectUtilities implemented, then just use the normal Play-method of the particle system.
                    // This might produce weird results, so it's generally adviceable to import the Hedronoid Utilities package and use
                    // ResetAndPlayParticleSystem instead.
                    //GameObjectUtilities.ResetAndPlayParticleSystem(particleOnTeleport);
                    m_ParticleOnTeleport.Play();
                }

                //Rigidbody rb = other.rigidbody;
                m_EnterPosition = m_TeleportTarget.position;
                triggeringObject.transform.BroadcastMessage("PrepareTeleport", SendMessageOptions.DontRequireReceiver);
                /*if (rb != null)
                    rb.MovePosition(teleportTarget.position);
                else
                    other.transform.position = teleportTarget.position;*/
                triggeringObject.transform.position = m_TeleportTarget.position;
                triggeringObject.transform.BroadcastMessage("WasTeleported", SendMessageOptions.DontRequireReceiver);

            }
            else if (m_TeleportObject != null)
            {
                if (m_ParticleOnTeleport != null)
                {
                    m_ParticleOnTeleport.transform.position = m_TeleportObject.transform.position;
                    // If you don't have the GameObjectUtilities implemented, then just use the normal Play-method of the particle system.
                    // This might produce weird results, so it's generally adviceable to import the Hedronoid Utilities package and use
                    // ResetAndPlayParticleSystem instead.
                    //GameObjectUtilities.ResetAndPlayParticleSystem(particleOnTeleport);
                    m_ParticleOnTeleport.Play();
                }

                m_EnterPosition = m_TeleportTarget.position;
                m_TeleportObject.BroadcastMessage("PrepareTeleport", SendMessageOptions.DontRequireReceiver);
                m_TeleportObject.position = m_TeleportTarget.position;
                m_TeleportObject.BroadcastMessage("WasTeleported", SendMessageOptions.DontRequireReceiver);
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);

            if (m_TeleportCollidingObject)
            {
                if (m_ParticleOnTeleport != null)
                {
                    m_ParticleOnTeleport.transform.position = triggeringObject.transform.position;
                    // If you don't have the GameObjectUtilities implemented, then just use the normal Play-method of the particle system.
                    // This might produce weird results, so it's generally adviceable to import the Hedronoid Utilities package and use
                    // ResetAndPlayParticleSystem instead.
                    //GameObjectUtilities.ResetAndPlayParticleSystem(particleOnTeleport);
                    m_ParticleOnTeleport.Play();
                }

                //Rigidbody rb = other.rigidbody;

                triggeringObject.transform.BroadcastMessage("PrepareTeleport", SendMessageOptions.DontRequireReceiver);
                /*if (rb != null)
                    rb.MovePosition(enterPosition);
                else
                    other.transform.position = enterPosition;*/
                triggeringObject.transform.position = m_EnterPosition;
                triggeringObject.transform.BroadcastMessage("WasTeleported", SendMessageOptions.DontRequireReceiver);
            }
            else if (m_TeleportObject != null)
            {
                if (m_ParticleOnTeleport != null)
                {
                    m_ParticleOnTeleport.transform.position = m_TeleportObject.transform.position;
                    // If you don't have the GameObjectUtilities implemented, then just use the normal Play-method of the particle system.
                    // This might produce weird results, so it's generally adviceable to import the Hedronoid Utilities package and use
                    // ResetAndPlayParticleSystem instead.
                    //GameObjectUtilities.ResetAndPlayParticleSystem(particleOnTeleport);
                    m_ParticleOnTeleport.Play();
                }

                m_TeleportObject.BroadcastMessage("PrepareTeleport", SendMessageOptions.DontRequireReceiver);
                m_TeleportObject.position = m_EnterPosition;
                m_TeleportObject.BroadcastMessage("WasTeleported", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}