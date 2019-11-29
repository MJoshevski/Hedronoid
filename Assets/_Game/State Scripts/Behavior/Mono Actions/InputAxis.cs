using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    [CreateAssetMenu(menuName = "Inputs/Axis")]
    public class InputAxis : Action
    {
        public string targetString;
        public float value;

        public SO.FloatVariable floatVariable;

        public override void Execute()
        {
            value = Input.GetAxis(targetString);

            if(floatVariable != null)
            {
                floatVariable.value = value;
            }
        }
    }
}
