using UnityEngine;
using UnityEngine.UI;
using Hedronoid;

namespace Hedronoid
{
    public class DebugMenu : HNDMonoBehaviour
    {
        [Header("Refs")]
        public GameObject Panel;
        public Button DebugButton;
        public Button CloseButton;
        public Button ResetBindingsButton;
        public Button SaveBindingsButton;

        public InputField VerticalSensitivityInputField;
        public Slider VerticalSensitivitySlider;
        public InputField HorizontalSensitivityInputField;
        public Slider HorizontalSensitivitySlider;


        public Transform BindingViewsPanel;

        [Header("Prefabs")]
        public GameObject BindingView;

        protected override void Start()
        {
            Panel.SetActive(false);

            DebugButton.onClick.AddListener(() => Panel.SetActive(true));
            CloseButton.onClick.AddListener(() => Panel.SetActive(false));
            ResetBindingsButton.onClick.AddListener(() => InputManager.Instance.ResetBindings());
            SaveBindingsButton.onClick.AddListener(() => InputManager.Instance.SaveBindings());

            LoadKeyDataToUI();
            LoadSensitivities();
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

        private void LoadSensitivities()
        {
            VerticalSensitivityInputField.text = InputManager.Instance.MouseVerticalSensitivity.ToString();
            VerticalSensitivityInputField.onEndEdit.AddListener((text) =>
            {
                float sensitivity;
                if (float.TryParse(text, out sensitivity))
                {
                    SetVerticalSensitivity(sensitivity);
                }
                else
                {
                    VerticalSensitivityInputField.text = InputManager.Instance.MouseVerticalSensitivity.ToString();
                }
            });
            VerticalSensitivitySlider.value = InputManager.Instance.MouseVerticalSensitivity;
            VerticalSensitivitySlider.onValueChanged.AddListener(value =>
            {
                SetVerticalSensitivity(value);
            });

            HorizontalSensitivityInputField.text = InputManager.Instance.MouseHorizontalSensitivity.ToString();
            HorizontalSensitivityInputField.onEndEdit.AddListener((text) =>
            {
                float sensitivity;
                if (float.TryParse(text, out sensitivity))
                {
                    SetHorizontalSensitivity(sensitivity);
                }
                else
                {
                    HorizontalSensitivityInputField.text = InputManager.Instance.MouseHorizontalSensitivity.ToString();
                }
            });
            HorizontalSensitivitySlider.value = InputManager.Instance.MouseHorizontalSensitivity;
            HorizontalSensitivitySlider.onValueChanged.AddListener(value =>
            {
                SetHorizontalSensitivity(value);
            });
        }

        private void SetHorizontalSensitivity(float sensitivity)
        {
            D.UILogFormat("SetHorizontalSensitivity(float {0})", sensitivity);
            InputManager.Instance.MouseHorizontalSensitivity = sensitivity;
            HorizontalSensitivityInputField.text = sensitivity.ToString();
            HorizontalSensitivitySlider.value = sensitivity;
        }

        void SetVerticalSensitivity(float sensitivity)
        {
            D.UILogFormat("SetVerticalSensitivity(float {0})", sensitivity);
            InputManager.Instance.MouseVerticalSensitivity = sensitivity;
            VerticalSensitivityInputField.text = sensitivity.ToString();
            VerticalSensitivitySlider.value = sensitivity;
        }
    }
}