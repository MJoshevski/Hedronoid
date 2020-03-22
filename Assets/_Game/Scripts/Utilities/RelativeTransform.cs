using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public class RelativeTransform : Transform
    {
        public Transform GetNonRelativeTrans { get; private set; }

        public RelativeTransform(Transform t)
        {
            GetNonRelativeTrans = t;
        }

        public float X()
        {
            return GetNonRelativeTrans.relX();
        }

        public float Y()
        {
            return GetNonRelativeTrans.relX();
        }

        public float Z()
        {
            return GetNonRelativeTrans.relX();
        }

        public Vector3 Up()
        {

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
