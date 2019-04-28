using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDKShooter
{
    public interface IInputManager
    {
        PlayerActionSet PlayerActions { get; }
        void SaveBindings();
        void ResetBindings();
    }

    public class InputManager : MonoSingleton<IInputManager>, IInputManager
    {
        public PlayerActionSet PlayerActions { get; private set; }

        protected override void Awake()
        {
            PlayerActions = PlayerActionSet.CreateWithDefaultBindings();
            LoadBindings();

            base.Awake();
        }

        public void SaveBindings()
        {
            var saveData = PlayerActions.Save();
            PlayerPrefs.SetString(KEY_BINDINGS, saveData);
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

        const string KEY_BINDINGS = "Bindings";
    }
}