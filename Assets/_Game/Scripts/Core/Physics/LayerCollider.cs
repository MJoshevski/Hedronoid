using System;
using System.Collections.Generic;
using UnityEngine;

public class LayerCollider : MonoBehaviour
{
    [SerializeField]
    LayerMask LayerMask;

    public event Action<Collision> CollisionEnter;
    public event Action<Collision> CollisionExit;

    public bool IsColliding()
    {
        return _collidingObjects.Count > 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        var go = collision.gameObject;
        if (LayerMask.Contains(go.layer))
        {
            _collidingObjects.Add(go);
            this.Raise(CollisionEnter, collision);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        var go = collision.gameObject;
        if (LayerMask.Contains(go.layer))
        {
            _collidingObjects.Remove(go);
            this.Raise(CollisionExit, collision);
        }
    }

    List<GameObject> _collidingObjects = new List<GameObject>();
}
