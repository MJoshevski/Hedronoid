using UnityEngine;
using UnityEngine.UI;

namespace Hedronoid
{
    public class DebugMenu : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] GameObject Panel;
        [SerializeField] Button DebugButton;
        [SerializeField] Button CloseButton;
        [SerializeField] Button ResetBindingsButton;
        [SerializeField] Button SaveBindingsButton;

        [SerializeField] InputField VerticalSensitivityInputField;
        [SerializeField] Slider VerticalSensitivitySlider;
        [SerializeField] InputField HorizontalSensitivityInputField;
        [SerializeField] Slider HorizontalSensitivitySlider;


        [SerializeField] Transform BindingViewsPanel;

        [Header("Prefabs")]
        [SerializeField] GameObject BindingView;

        void Start()
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
            Debug.LogFormat("SetHorizontalSensitivity(float {0})", sensitivity);
            InputManager.Instance.MouseHorizontalSensitivity = sensitivity;
            HorizontalSensitivityInputField.text = sensitivity.ToString();
            HorizontalSensitivitySlider.value = sensitivity;
        }

        void SetVerticalSensitivity(float sensitivity)
        {
            Debug.LogFormat("SetVerticalSensitivity(float {0})", sensitivity);
            InputManager.Instance.MouseVerticalSensitivity = sensitivity;
            VerticalSensitivityInputField.text = sensitivity.ToString();
            VerticalSensitivitySlider.value = sensitivity;
        }
    }
}