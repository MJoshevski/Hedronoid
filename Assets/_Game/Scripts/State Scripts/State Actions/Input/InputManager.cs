using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public interface IInputManager
    {
        PlayerActionSet PlayerActions { get; }
        float MouseHorizontalSensitivity { get; set; }
        float MouseVerticalSensitivity { get; set; }

        void SaveBindings();
        void ResetBindings();
    }

    public class InputManager : MonoSingleton<IInputManager>, IInputManager
    {
        public PlayerActionSet PlayerActions { get; private set; }
        public float MouseHorizontalSensitivity { get; set; }
        public float MouseVerticalSensitivity { get; set; }

        protected override void Awake()
        {
            PlayerActions = PlayerActionSet.CreateWithDefaultBindings();
            LoadBindings();
            LoadSensitivities();

            base.Awake();
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