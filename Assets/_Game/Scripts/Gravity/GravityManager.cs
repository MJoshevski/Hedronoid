using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace Hedronoid.Gravity
{
    public class GravityManager : HNDMonoBehaviour
    {
        public List<GravitySource> gravitySourcesInScene = new List<GravitySource>();
        public List<string> gravityParentNamesInScene;

        public void ScanForGravitySources()
        {
            gravitySourcesInScene = FindObjectsOfType<GravitySource>().ToList();
            gravityParentNamesInScene = new List<string>(gravitySourcesInScene.Count);
            gravityParentNamesInScene.Clear();

            foreach (GravitySource gs in gravitySourcesInScene)
                gravityParentNamesInScene.Add(gs.transform.parent.name);
        }
    }
}
