using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAutosyncTransforms : MonoBehaviour
{
    bool prev;
    // Start is called before the first frame update
    void Start()
    {
        prev = Physics.autoSyncTransforms;
        Physics.autoSyncTransforms = false;
    }

    // Update is called once per frame
    void OnDestroy()
    {
         Physics.autoSyncTransforms = prev;
    }
}
