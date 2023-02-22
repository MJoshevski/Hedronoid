using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraShake
{
    public enum CameraShakeType
    {
        None = 0,
        BounceShake = 1 ,
        BounceShake2 = 2,
        KickShake = 3,
        PerlinShake = 4
    }
    [CreateAssetMenu(menuName = "Hedronoid/Camera Shake Preset")]
    public class CameraShakeCollection : ScriptableObject
    {
        public CameraShakeType DefaultCamerShake = CameraShakeType.None;

        [SerializeField]
        BounceShake.Params BounceShakeParams;
        [SerializeField]
        BounceShake.Params BounceShakeParams2;
        [SerializeField]
        List<KickShake.Params> KickShakeParamsList = new List<KickShake.Params>();
        [SerializeField]
        PerlinShake.Params PerlinShakeParams;

        public void PlayCameraShake(CameraShakeType type)
        {
            ICameraShake cameraShakeInstance = null;

            switch (type)
            {
                case CameraShakeType.BounceShake:
                    cameraShakeInstance = new BounceShake(BounceShakeParams);
                    break;
                case CameraShakeType.BounceShake2:
                    cameraShakeInstance = new BounceShake(BounceShakeParams2);
                    break;
                case CameraShakeType.KickShake:
                    int idx = Random.Range(0, KickShakeParamsList.Count - 1);
                    cameraShakeInstance = new KickShake(KickShakeParamsList[idx], KickShakeParamsList[idx].KickShakeDisplacementType);
                    break;
                case CameraShakeType.PerlinShake:
                    cameraShakeInstance = new PerlinShake(PerlinShakeParams);
                    break;
            }

            if (cameraShakeInstance != null)
                CameraShaker.Shake(cameraShakeInstance);
        }

        public void PlayDefaultCameraShake()
        {
            PlayCameraShake(DefaultCamerShake);
        }
    }
}

