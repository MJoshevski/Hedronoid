using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MDKShooter;

namespace MDKShooter
{
    public class DebugMenu : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] GameObject Panel;
        [SerializeField] Button DebugButton;
        [SerializeField] Button CloseButton;
        [SerializeField] Button ResetBindingsButton;
        [SerializeField] Button SaveBindingsButton;

        [SerializeField] Transform BindingViewsPanel;

        [Header("Prefabs")]
        [SerializeField] GameObject BindingView;

        void Start()
        {
            DebugButton.onClick.AddListener(() => Panel.SetActive(true));
            CloseButton.onClick.AddListener(() => Panel.SetActive(false));
            ResetBindingsButton.onClick.AddListener(() => InputManager.Instance.ResetBindings());
            SaveBindingsButton.onClick.AddListener(() => InputManager.Instance.SaveBindings());

            LoadKeyDataToUI();
        }

        void Update()
        {
            if (!Panel.activeSelf)
                return;

            LoadKeyDataToUI();
        }

        void LoadKeyDataToUI()
        {
            var playerActions = InputManager.Instance.PlayerActions;
            for (int i = 0; i < playerActions.Actions.Count; i++)
            {
                var action = playerActions.Actions[i];

                var keyBindingView = UnityHelper.InstantiatieViewAt<KeyBindingView>(BindingView, BindingViewsPanel, i);
                keyBindingView.ActionNameText.text = action.Name;
                keyBindingView.ChangeBindingButton.interactable = !action.IsListeningForBinding;

                for (var j = 0; j < action.Bindings.Count && j < 2; j++)
                {
                    var binding = action.Bindings[j];
                    keyBindingView.KeyText[j].text = binding.DeviceName + ": " + binding.Name;
                }

                var buttonClick = keyBindingView.ChangeBindingButton.onClick;
                buttonClick.RemoveAllListeners();
                buttonClick.AddListener(() =>
                {
                    action.ListenForBinding();
                });

            }
        }
    }
}