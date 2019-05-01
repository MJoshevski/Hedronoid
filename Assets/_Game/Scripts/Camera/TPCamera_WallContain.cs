using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class TPCamera_WallContain : ProtectCameraFromWallClip
{
    public LayerMask m_CollidesWithLayers;  //Which layers are we detecting collision for

    protected override void LateUpdate()
    {

        // initially set the target distance
        float targetDist = m_OriginalDist;

        m_Ray.origin = m_Pivot.position + m_Pivot.forward* sphereCastRadius;
        m_Ray.direction = -m_Pivot.forward;

                // initial check to see if start of spherecast intersects anything
                var cols = Physics.OverlapSphere(m_Ray.origin, sphereCastRadius);

        bool initialIntersect = false;
        bool hitSomething = false;

        // loop through all the collisions to check if something we care about
        for (int i = 0; i<cols.Length; i++)
        {
            if ((!cols[i].isTrigger) &&
                !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag)))
            {
                initialIntersect = true;
                break;
            }
        }

        // if there is a collision
        if (initialIntersect)
        {
            m_Ray.origin += m_Pivot.forward* sphereCastRadius;

            // do a raycast and gather all the intersections
            m_Hits = Physics.RaycastAll(m_Ray, m_OriginalDist - sphereCastRadius);
        }
        else
        {
            // if there was no collision do a sphere cast to see if there were any other collisions
            m_Hits = Physics.SphereCastAll(m_Ray, sphereCastRadius, m_OriginalDist + sphereCastRadius);
        }

        // sort the collisions by distance
        Array.Sort(m_Hits, m_RayHitComparer);

        // set the variable used for storing the closest to be as far as possible
        float nearest = Mathf.Infinity;

        // loop through all the collisions
        for (int i = 0; i<m_Hits.Length; i++)
        {
            int currHitLayer = 1 << m_Hits[i].collider.gameObject.layer;

            // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
            if (m_Hits[i].distance<nearest && (!m_Hits[i].collider.isTrigger) &&
                !(m_Hits[i].collider.attachedRigidbody != null &&
                    m_Hits[i].collider.attachedRigidbody.CompareTag(dontClipTag)) &&
                    (currHitLayer & m_CollidesWithLayers.value) > 0)
            {
                // change the nearest collision to latest
                nearest = m_Hits[i].distance;
                targetDist = -m_Pivot.InverseTransformPoint(m_Hits[i].point).z;
                hitSomething = true;
            }
        }

        // visualise the cam clip effect in the editor
        if (hitSomething)
        {
            Debug.DrawRay(m_Ray.origin, -m_Pivot.forward* (targetDist + sphereCastRadius), Color.red);
        }

        // hit something so move the camera to a better position
        protecting = hitSomething;
        m_CurrentDist = Mathf.SmoothDamp(m_CurrentDist, targetDist, ref m_MoveVelocity,
                                        m_CurrentDist > targetDist? clipMoveTime : returnTime);
        m_CurrentDist = Mathf.Clamp(m_CurrentDist, closestDistance, m_OriginalDist);
        m_Cam.localPosition = -Vector3.forward* m_CurrentDist;
    }
}
