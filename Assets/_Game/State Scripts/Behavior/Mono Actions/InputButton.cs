using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using System.Linq;
using InControl;

namespace Hedronoid
{
	[CreateAssetMenu(menuName = "Inputs/Button")]
	public class InputButton : Action
	{
		public string targetInput;
		public bool isPressed;
		public KeyState keyState;
		public bool updateBoolVar = true;
        public BoolVariable targetBoolVariable;
        private PlayerAction _playerAction;

        public override void Execute_Start()
        {
            _playerAction = PlayerStateManager.Instance
                .PlayerActions
                .Actions
                .FirstOrDefault(a => a.Name == targetInput.Trim());

            if (_playerAction == null)
            {
                Debug.LogError("Could not find player action with name " + targetInput);
            }
        }

        public override void Execute()
		{
            switch (keyState)
			{
				case KeyState.onDown:
                    isPressed = _playerAction.WasPressed;
					break;
				case KeyState.onHold:
					isPressed = _playerAction.IsPressed;
                    break;
				case KeyState.onUp:
					isPressed = _playerAction.WasReleased;
                    break;
				default:
					break;
			}

			if (updateBoolVar)
			{
                if (targetBoolVariable != null)
                {
                    targetBoolVariable.value = isPressed;
                }
            }
		}

		public enum KeyState
		{
			onDown,onHold,onUp
		}
	}
}
