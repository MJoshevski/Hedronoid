using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGravityService
{
    float Gravity { get; }
    GravityDirections Direction { get; }
    Vector3 GravityDirection { get; }
    Quaternion GravityRotation { get; }
    Vector3 GravityUp { get; }

    void SwitchDirection(GravityDirections direction);
}

public class GravityService : MonoSingleton<IGravityService>, IGravityService
{
    public float Gravity { get { return -9.81f; } }
    public GravityDirections Direction { get; private set; }
    public Vector3 GravityDirection { get; private set; }
    public Quaternion GravityRotation { get; private set; }
    public Vector3 GravityUp
    {
        get
        {
            return GravityDirection * -1;
        }
    }

    void Start()
    {
        GravityService.Instance.SwitchDirection(GravityDirections.DOWN);
    }

    public void SwitchDirection(GravityDirections direction)
    {
        Direction = direction;
        switch (direction)
        {
            case GravityDirections.DOWN:
                GravityDirection = Vector3.down;
                GravityRotation = Quaternion.identity;
                break;
            case GravityDirections.UP:
                GravityDirection = Vector3.up;
                GravityRotation = Quaternion.Euler(0, 0, 180);
                break;
            case GravityDirections.LEFT:
                GravityDirection = Vector3.left;
                GravityRotation = Quaternion.Euler(0, 0, -90);
                break;
            case GravityDirections.RIGHT:
                GravityDirection = Vector3.right;
                GravityRotation = Quaternion.Euler(0, 0, 90);
                break;
        }

        Debug.Log("Changed direction to " + direction);
    }
}
