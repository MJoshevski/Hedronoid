using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGravityChanger : MonoBehaviour
{
    void Update()
    {
        var gravityService = GravityService.Instance;
        if (Input.GetKeyDown(KeyCode.Alpha1))
            gravityService.SwitchDirection(GravityDirections.DOWN);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            gravityService.SwitchDirection(GravityDirections.UP);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            gravityService.SwitchDirection(GravityDirections.LEFT);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            gravityService.SwitchDirection(GravityDirections.RIGHT);
    }
}
