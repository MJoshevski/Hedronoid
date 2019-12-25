using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Inputs/Axis")]
    public class InputAxis : Action
    {
        InControl.PlayerAction _playerAction;
        public string targetString;
        public float value;

        public FloatVariable floatVariable;

        public override void Execute_Start()
        {
           // _playerAction = InputManager.Instance
           //.PlayerActions
           //.Actions
           //.FirstOrDefault(a => a.Name == CharacterDashSettings.ActionName.Trim());
           // if (_playerAction == null)
           // {
           //     Debug.LogError("Could not find player action with name " + CharacterDashSettings.ActionName);
           // }
        }

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
