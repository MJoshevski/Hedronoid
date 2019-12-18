using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SO;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/Mono Actions/Input Manager")]
    public class InputManagerSP : Action
    {
        public FloatVariable horizontal;
        public FloatVariable vertical;
        public BoolVariable jump;

        public StateManagerVariable playerStates;
        public ActionBatch inputUpdateBatch;

        public PlayerActionSet PlayerActions { get; private set; }
        public float MouseHorizontalSensitivity { get; set; }
        public float MouseVerticalSensitivity { get; set; }

        public override void Execute_Start()
        {
        }

        public override void Execute()
        {
            inputUpdateBatch.Execute();

            if(playerStates.value != null)
            {
                playerStates.value.movementVariables.Horizontal = horizontal.value;
                playerStates.value.movementVariables.Vertical = vertical.value;

                float moveAmount =
                    Mathf.Clamp01(Mathf.Abs(horizontal.value) + Mathf.Abs(vertical.value));
                playerStates.value.movementVariables.MoveAmount = moveAmount;
                playerStates.value.isJumping = jump.value;

            }
        }

        public void SaveBindings()
        {
            var saveData = PlayerActions.Save();
            PlayerPrefs.SetString(KEY_BINDINGS, saveData);
            PlayerPrefs.SetFloat(KEY_MOUSE_HORIZONTAL_SENSITIVITY, MouseHorizontalSensitivity);
            PlayerPrefs.SetFloat(KEY_MOUSE_VERTICAL_SENSITIVITY, MouseVerticalSensitivity);
            PlayerPrefs.Save();
            Debug.Log("Bindings saved...");
        }

        public void ResetBindings()
        {
            PlayerActions = PlayerActionSet.CreateWithDefaultBindings();
            Debug.Log("Bindings reset...");

        }

        void LoadBindings()
        {
            if (PlayerPrefs.HasKey(KEY_BINDINGS))
            {
                var saveData = PlayerPrefs.GetString(KEY_BINDINGS);
                PlayerActions.Load(saveData);
                Debug.Log("Bindings loaded...");
            }
        }

        void LoadSensitivities()
        {
            MouseHorizontalSensitivity = PlayerPrefs.GetFloat(KEY_MOUSE_HORIZONTAL_SENSITIVITY, 50f);
            MouseVerticalSensitivity = PlayerPrefs.GetFloat(KEY_MOUSE_VERTICAL_SENSITIVITY, 50f);
        }

        const string KEY_BINDINGS = "Bindings";
        const string KEY_MOUSE_HORIZONTAL_SENSITIVITY = "MouseHorizontalSensitivity";
        const string KEY_MOUSE_VERTICAL_SENSITIVITY = "MouseVerticalSensitivity";
    }

}
