using System.Collections.Generic;
using UnityEngine;

public class LayerCollider : MonoBehaviour
{
    [SerializeField]
    LayerMask LayerMask;

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
        }
    }

    void OnTriggerExit(Collider other)
    {
        var go = other.gameObject;
        if (LayerMask.Contains(go.layer))
        {
            _collidingObjects.Remove(go);
        }
    }

    List<GameObject> _collidingObjects = new List<GameObject>();
}
