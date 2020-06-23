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
        public float shootForce_Auto = 8000f;
        public float shootForce_Shotgun = 5000f;
        public float shootForce_Rail = 100000f;

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

            RaycastHit hit;
            Physics.Raycast(LookRay, out hit, 10000f);

            Gizmos.Line(bulletOrigin.value.position, hit.point, Color.yellow);
            Vector3 shootDirection = hit.point - bulletOrigin.value.position;

            if (Input.GetButton("Fire1") && 
                Time.realtimeSinceStartup - states.lastFired_Auto > fireRate_Auto)
            {
                auto = Instantiate(bullets_Auto, bulletOrigin.value.position, Quaternion.identity);
                rb_auto = auto.GetComponent<Rigidbody>();
                rb_auto.AddForce(shootDirection.normalized * shootForce_Auto);
                states.lastFired_Auto = Time.realtimeSinceStartup;
            }
            else if (Input.GetButtonDown("Fire2") &&
                Time.realtimeSinceStartup - states.lastFired_Shotgun > fireRate_Shotgun)
            {
                GameObject shot = Instantiate(bullets_Shotgun, bulletOrigin.value.position, Quaternion.identity);
                Rigidbody rb_shot = shot.GetComponent<Rigidbody>();
                rb_shot.AddForce(shootDirection.normalized * shootForce_Shotgun);
                states.lastFired_Shotgun = Time.realtimeSinceStartup;
            }
        }
    }
}
