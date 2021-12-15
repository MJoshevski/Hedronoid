using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class GunBot : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    private SteeringBasics steering;
    private void Awake()
    {
        if (!steering) TryGetComponent(out steering);
    }
    void Start()
    {
        steering.Arrive(target.transform.position);
    }

    private void FixedUpdate()
    {
        Vector3 accel = steering.Arrive(target.transform.position);

        steering.Steer(accel);
        steering.LookWhereYoureGoing();
    }

}