using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCollisionNormal : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        _collision = collision;
    }

    void OnCollisionStay(Collision collision)
    {
        _collision = collision;
    }

    void OnCollisionEnd(Collision collision)
    {
        _collision = null;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (_collision == null)
            return;

        for (int i = 0; i < _collision.contacts.Length; i++)
        {
            var contact = _collision.contacts[i];

            Gizmos.DrawRay(contact.point, contact.normal);
            Gizmos.DrawWireSphere(contact.point, .1f);
        }
    }

    Collision _collision;
}
