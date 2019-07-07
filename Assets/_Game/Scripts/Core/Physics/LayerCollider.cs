using System;
using System.Collections.Generic;
using UnityEngine;

public class LayerCollider : MonoBehaviour
{
    [SerializeField]
    LayerMask LayerMask;

    public event Action<Collider> CollisionEnter;
    public event Action<Collider> CollisionExit;

    public bool IsColliding()
    {
        return _collidingObjects.Count > 0;
    }

    void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;
        if (LayerMask.Contains(go.layer))
        {
            _collidingObjects.Add(go);
            this.Raise(CollisionEnter, other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var go = other.gameObject;
        if (LayerMask.Contains(go.layer))
        {
            _collidingObjects.Remove(go);
            this.Raise(CollisionExit, other);
        }
    }

    List<GameObject> _collidingObjects = new List<GameObject>();
}
