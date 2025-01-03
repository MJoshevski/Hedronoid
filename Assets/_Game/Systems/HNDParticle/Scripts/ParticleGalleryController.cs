﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hedronoid.Particle
{
    public class ParticleGalleryController : MonoBehaviour
    {

        private List<HNDParticleSystem> m_PRTList = new List<HNDParticleSystem>();

        private Vector3 m_IntervalOffset = Vector3.right * 2.5f;
        private Vector3 m_CameraOffset = new Vector3(0, 3f, -8f);

#if UNITY_EDITOR
        public void Init(ParticleManagerData particleManagerData, HNDParticleSystemData currentPRTData)
        {
            //Find existing PRTs - TODO
            var existingPRTs = FindObjectsOfType<HNDParticleSystem>();
            var spawnPos = Vector3.zero;
            float furthestDistToOrigo = 0;
            foreach (var prt in existingPRTs)
            {
                float distToOrigo = Vector3.Distance(prt.transform.position, Vector3.zero);
                if (distToOrigo > furthestDistToOrigo)
                {
                    furthestDistToOrigo = distToOrigo;
                    spawnPos = prt.transform.position + m_IntervalOffset;
                }
                m_PRTList.Add(prt);
            }

            //Instantiate missing PRTs
            foreach (var prtData in particleManagerData.ParticleSystems)
            {
                if (m_PRTList.Find(x => x.name == prtData.Name)) continue;

                var prt = (HNDParticleSystem) PrefabUtility.InstantiatePrefab(prtData.ParticleSystemPrefab);
                prt.transform.position = spawnPos;
                spawnPos += m_IntervalOffset;

                m_PRTList.Add(prt);
            }

            if (currentPRTData != null)
            {
                var focusPRT = m_PRTList.Find(p => p.name == currentPRTData.Name);
                
                // if (focusPRT == null) return;
                // var prt = Instantiate(currentPRTData.ParticleSystemPrefab);
                var sceneView = SceneView.currentDrawingSceneView;
                var lastSceneView = SceneView.lastActiveSceneView;

                // Debug.Log("sceneView: " + sceneView + ", lastSceneView: " + lastSceneView);
                // if (sceneView) sceneView.pivot = prt.transform.position;
                if (lastSceneView)
                {
                    lastSceneView.pivot = focusPRT.transform.position;
                    lastSceneView.LookAt(focusPRT.transform.position);
                }
                var sceneCamera = Camera.main;
                if (sceneCamera)
                {
                    sceneCamera.transform.position = focusPRT.transform.position + m_CameraOffset;
                    sceneCamera.transform.LookAt(focusPRT.transform);
                }
            }

            EditorApplication.isPlaying = true;

            foreach (var prt in m_PRTList)
            {
                var topPartSys = prt.GetComponentInChildren<ParticleSystem>();
                if (topPartSys)
                {
                    var main = topPartSys.main;
                    main.loop = true;
                    main.duration *= 2f; //Making duration longer, so it's more obvious that particleSys is not actually looping
                }
            }
        }
#endif
    }

}
