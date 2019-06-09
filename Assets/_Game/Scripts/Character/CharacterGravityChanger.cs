using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGravityChanger : MonoBehaviour
{
    [Tooltip("If checked, gravity switching will be handled by key presses. If uncheked, switching will be triggered on wall collision.")]
    [SerializeField]
    private bool ChangeOnKeyPress = true;

    //[SerializeField]
    //private LayerMask GravityWallsLayers;

    [Tooltip("How much time (seconds) should we wait before we can switch again?")]
    [SerializeField]
    private float GravitySwitchCooldown = 1.5f;

    private string MostRecentCollision;
    private bool CanSwitchAgain = true;

    void Update()
    {
        if (!ChangeOnKeyPress) return;

        IGravityService gravityService = GravityService.Instance;
        if (Input.GetKeyDown(KeyCode.Alpha1))
            gravityService.SwitchDirection(GravityDirections.DOWN);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            gravityService.SwitchDirection(GravityDirections.UP);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            gravityService.SwitchDirection(GravityDirections.LEFT);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            gravityService.SwitchDirection(GravityDirections.RIGHT);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            gravityService.SwitchDirection(GravityDirections.FRONT);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            gravityService.SwitchDirection(GravityDirections.BACK);
    }

    private void FixedUpdate()
    {
        if (ChangeOnKeyPress) return;

        IGravityService gravityService = GravityService.Instance;
        RaycastHit m_Hit;

        bool m_HitDetect = Physics.SphereCast(
            transform.localPosition,
            0.00000001f,
            transform.forward,
            out m_Hit);

        if (m_HitDetect)
        {
            MostRecentCollision = m_Hit.collider.tag;

            //Debug.Log("Hit object with tag: " + MostRecentCollision);

            if (MostRecentCollision != gravityService.Direction.ToString() && CanSwitchAgain)
            {
                if (MostRecentCollision == "DOWN")
                    gravityService.SwitchDirection(GravityDirections.DOWN);
                else if (MostRecentCollision == "UP")
                    gravityService.SwitchDirection(GravityDirections.UP);
                else if (MostRecentCollision == "LEFT")
                    gravityService.SwitchDirection(GravityDirections.LEFT);
                else if (MostRecentCollision == "RIGHT")
                    gravityService.SwitchDirection(GravityDirections.RIGHT);
                else if (MostRecentCollision == "FRONT")
                    gravityService.SwitchDirection(GravityDirections.FRONT);
                else if (MostRecentCollision == "BACK")
                    gravityService.SwitchDirection(GravityDirections.BACK);
                else return;

                SwitchingBlockCooldown();
            }
        }
    }

    private IEnumerator SwitchingBlockCooldown()
    {
        CanSwitchAgain = false;
        yield return new WaitForSeconds(GravitySwitchCooldown);
        CanSwitchAgain = false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.localPosition, 1f);
    }
}
