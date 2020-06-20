using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Shooting")]
    public class Shooting : StateActions
    {
        public float fireRate_Auto = 0.2f;
        public float fireRate_Shotgun = 2f;
        public float fireRate_Rail = 5f;

        public GameObject bullets_Auto;
        public GameObject bullets_Shotgun;
        public GameObject bullets_Rail;

        public TransformVariable bulletOrigin;
        public Rigidbody rb_auto;
        public GameObject auto;
        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            PlayerActionSet playerAction = InputManager.Instance.PlayerActions;
            Ray LookRay =
                states.camera.value.ScreenPointToRay(
                    new Vector3(Screen.width / 2, Screen.height / 2, 0));

            Vector3 shootDirection = LookRay.direction;          

            if (Input.GetButton("Fire1") && 
                Time.realtimeSinceStartup - states.lastFired_Auto > fireRate_Auto)
            {
                auto = Instantiate(bullets_Auto, bulletOrigin.value.position, Quaternion.identity);
                rb_auto = auto.GetComponent<Rigidbody>();
                rb_auto.AddForce(shootDirection * 8000f);
                states.lastFired_Auto = Time.realtimeSinceStartup;
            }
            else if (Input.GetButtonDown("Fire2") &&
                Time.realtimeSinceStartup - states.lastFired_Shotgun > fireRate_Shotgun)
            {
                GameObject shot = Instantiate(bullets_Shotgun, bulletOrigin.value.position, Quaternion.identity);
                Rigidbody rb_shot = shot.GetComponent<Rigidbody>();
                rb_shot.AddForce(shootDirection * 50000f);
                states.lastFired_Shotgun = Time.realtimeSinceStartup;
            }
        }
    }
}
