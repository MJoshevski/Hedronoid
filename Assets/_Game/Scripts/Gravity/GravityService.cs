using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public enum GravityOrigin
    {
        Global = 0,
        Local = 1
    }

    public interface IGravityService
    {
        float GravityAmount { get; }
        GravityDirections Direction { get; }
        GravityOrigin Origin { get; }
        Vector3 GravityDirection { get; }
        Quaternion GravityRotation { get; }
        Vector3 GravityUp { get; }
        Vector3 GravityRight { get; }
        Vector3 GravityForward { get; }

        void SwitchDirection(GravityDirections direction);
    }

    public class GravityService : MonoSingleton<IGravityService>, IGravityService
    {
        public Transform playerInputSpace;
        public float GravityAmount { get { return 9.81f; } }
        public static Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }
        public GravityDirections Direction { get; private set; }
        public GravityOrigin Origin { get; set; }
        public Vector3 GravityDirection { get; private set; }
        public Quaternion GravityRotation { get; private set; }
        public Vector3 GravityUp
        {
            get
            {
                return GravityDirection * -1;
            }
        }
        public Vector3 GravityRight { get; private set; }
        public Vector3 GravityForward { get; private set; }

        void Start()
        {
            Direction = GravityDirections.DOWN;
            SwitchDirection(GravityDirections.DOWN);
            Origin = GravityOrigin.Global;
        }

        void Update()
        {
            if (playerInputSpace)
            {
                GravityRight = VectorExtensions.ProjectDirectionOnPlane(playerInputSpace.right, GravityUp);
                GravityForward = VectorExtensions.ProjectDirectionOnPlane(playerInputSpace.forward, GravityUp);
            }
            else
            {
                GravityRight = VectorExtensions.ProjectDirectionOnPlane(Vector3.right, GravityUp);
                GravityForward = VectorExtensions.ProjectDirectionOnPlane(Vector3.forward, GravityUp);
            }
        }

        public void SwitchDirection(GravityDirections direction)
        {
            Debug.LogFormat("[Gravity] Switched from {0} to {1}", this.Direction, direction);
            switch (direction)
            {
                case GravityDirections.DOWN:
                    GravityDirection = Vector3.down;
                    break;
                case GravityDirections.UP:
                    GravityDirection = Vector3.up;
                    break;
                case GravityDirections.LEFT:
                    GravityDirection = Vector3.left;
                    break;
                case GravityDirections.RIGHT:
                    GravityDirection = Vector3.right;
                    break;
                case GravityDirections.FRONT:
                    GravityDirection = Vector3.forward;
                    break;
                case GravityDirections.BACK:
                    GravityDirection = Vector3.back;
                    break;
            }
            switch (Direction)
            {
                case GravityDirections.UP:
                    {
                        switch (direction)
                        {
                            case GravityDirections.DOWN:
                                GravityRotation = Quaternion.Euler(0, -90, 0);
                                break;
                            case GravityDirections.LEFT:
                                GravityRotation = Quaternion.Euler(90, 0, -90);
                                break;
                            case GravityDirections.RIGHT:
                                GravityRotation = Quaternion.Euler(90, 0, 90);
                                break;
                            case GravityDirections.FRONT:
                                GravityDirection = Vector3.forward;
                                GravityRotation = Quaternion.Euler(-90, 0, 0);
                                break;
                            case GravityDirections.BACK:
                                GravityDirection = Vector3.back;
                                GravityRotation = Quaternion.Euler(90, 0, 0);
                                break;
                        }
                    }
                    break;
                case GravityDirections.DOWN:
                    {
                        switch (direction)
                        {
                            case GravityDirections.UP:
                                GravityRotation = Quaternion.Euler(0.0f, -270.0f, 180.0f);
                                break;
                            case GravityDirections.LEFT:
                                GravityRotation = Quaternion.Euler(-90, 0, -90);
                                break;
                            case GravityDirections.RIGHT:
                                GravityRotation = Quaternion.Euler(-90, 0, 90);
                                break;
                            case GravityDirections.FRONT:
                                GravityDirection = Vector3.forward;
                                GravityRotation = Quaternion.Euler(-90, 0, 0);
                                break;
                            case GravityDirections.BACK:
                                GravityDirection = Vector3.back;
                                GravityRotation = Quaternion.Euler(90, 0, 0);
                                break;
                        }
                    }
                    break;
                case GravityDirections.LEFT:
                    {
                        switch (direction)
                        {
                            case GravityDirections.UP:
                                GravityRotation = Quaternion.Euler(0.0f, -270.0f, 180.0f);
                                break;
                            case GravityDirections.DOWN:
                                GravityRotation = Quaternion.Euler(180.0f, -90.0f, -180.0f);
                                break;
                            case GravityDirections.RIGHT:
                                GravityRotation = Quaternion.Euler(0, 0, 90);
                                break;
                            case GravityDirections.FRONT:
                                GravityDirection = Vector3.forward;
                                GravityRotation = Quaternion.Euler(-90, 0, 0);
                                break;
                            case GravityDirections.BACK:
                                GravityDirection = Vector3.back;
                                GravityRotation = Quaternion.Euler(90, 0, 0);
                                break;
                        }
                    }
                    break;
                case GravityDirections.RIGHT:
                    {
                        switch (direction)
                        {
                            case GravityDirections.UP:
                                GravityRotation = Quaternion.Euler(0, 180, 180);
                                break;
                            case GravityDirections.DOWN:
                                GravityRotation = Quaternion.Euler(0, -90, 0);
                                break;
                            case GravityDirections.LEFT:
                                GravityRotation = Quaternion.Euler(0, 180, 90);
                                break;
                            case GravityDirections.FRONT:
                                GravityDirection = Vector3.forward;
                                GravityRotation = Quaternion.Euler(-90, 0, 0);
                                break;
                            case GravityDirections.BACK:
                                GravityDirection = Vector3.back;
                                GravityRotation = Quaternion.Euler(90, 0, 0);
                                break;
                        }
                    }
                    break;
                case GravityDirections.FRONT:
                    {
                        switch (direction)
                        {
                            case GravityDirections.UP:
                                GravityRotation = Quaternion.Euler(0, 180, 180);
                                break;
                            case GravityDirections.DOWN:
                                GravityRotation = Quaternion.Euler(0, -90, 0);
                                break;
                            case GravityDirections.LEFT:
                                GravityRotation = Quaternion.Euler(0, 180, 90);
                                break;
                            case GravityDirections.RIGHT:
                                GravityDirection = Vector3.forward;
                                GravityRotation = Quaternion.Euler(-90, 0, 0);
                                break;
                            case GravityDirections.BACK:
                                GravityDirection = Vector3.back;
                                GravityRotation = Quaternion.Euler(90, 0, 0);
                                break;
                        }
                    }
                    break;
                case GravityDirections.BACK:
                    {
                        switch (direction)
                        {
                            case GravityDirections.UP:
                                GravityRotation = Quaternion.Euler(0, 180, 180);
                                break;
                            case GravityDirections.DOWN:
                                GravityRotation = Quaternion.Euler(0, -90, 0);
                                break;
                            case GravityDirections.LEFT:
                                GravityRotation = Quaternion.Euler(0, 180, 90);
                                break;
                            case GravityDirections.RIGHT:
                                GravityDirection = Vector3.forward;
                                GravityRotation = Quaternion.Euler(-90, 0, 0);
                                break;
                            case GravityDirections.FRONT:
                                GravityDirection = Vector3.back;
                                GravityRotation = Quaternion.Euler(90, 0, 0);
                                break;
                        }
                    }
                    break;
            }
            Direction = direction;
        }
    }
}
