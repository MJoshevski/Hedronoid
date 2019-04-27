using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDKShooter.Gravity
{
    public class GravityAttractor : MonoBehaviour
    {
        private float m_Gravity = -9.81f;

        public void Attract(Rigidbody rb)
        {
            Transform m_body = rb.transform;

            Vector3 m_YVector = new Vector3(0, transform.position.y, 0);
            Vector3 m_AttractedBodyYVector = new Vector3(0, m_body.position.y, 0);

            Vector3 gravityUp = (m_AttractedBodyYVector - m_YVector).normalized;
            Vector3 bodyUp = m_body.up;

            rb.AddForce(gravityUp * m_Gravity);

            Quaternion bodyRotation = m_body.rotation;
            Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * bodyRotation;
            bodyRotation = Quaternion.Slerp(bodyRotation, targetRotation, 50 * Time.deltaTime);
            Debug.LogFormat("Body Rotation: {0} --- Target ROtation: {1}", bodyRotation.eulerAngles.ToString(), targetRotation.eulerAngles.ToString());
        }
    }
}
