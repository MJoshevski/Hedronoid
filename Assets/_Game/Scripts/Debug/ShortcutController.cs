using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using System;

namespace Hedronoid
{
    public class ShortcutController : HNDMonoSingleton<ShortcutController>
    {
        protected ShortcutController() { } // guarantee this will be always a singleton only - can't use the constructor!
        public KeyCode m_restartLevelKey = KeyCode.R;     // Restarts current level

        public enum EShortcuts
        {
            RESET_SCENE,
            FAST_RESET,
        }

        private Dictionary<int, bool> m_ShortcutPressedDict = new Dictionary<int, bool>();
        private int m_ControllerType = 0;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);

            m_ShortcutPressedDict = new Dictionary<int, bool>();
            int amountOfShortCuts = Enum.GetNames(typeof(EShortcuts)).Length;
            for (int i = 0; i < amountOfShortCuts; i++)
            {
                m_ShortcutPressedDict.Add((int)i, false);
            }

            string[] joystickNames = Input.GetJoystickNames();
            for (int i = 0; i < joystickNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(joystickNames[i]))
                {
                    if (joystickNames[i].Contains("XBOX"))
                    {
                        m_ControllerType = 1;
                        break;
                    }
                }
            }
            // If there are joysticks connected, but it is not Xbox controllers, we assume it is a Playstation controller
            if (m_ControllerType == 0 && joystickNames.Length > 0)
            {
                m_ControllerType = 2;
            }
        }

        void Update()
        {
            // Enums used as keys in dict generate garbage!
            // R1 + X / RB + A
            m_ShortcutPressedDict[(int)EShortcuts.RESET_SCENE] = Input.GetKeyDown(m_restartLevelKey);
            /*|| (m_ControllerType == 1 && !Input.GetKey(ControllerMap.XB_BUTTON_LB) && Input.GetKey(ControllerMap.XB_BUTTON_RB) && Input.GetKeyDown(ControllerMap.XB_BUTTON_A))
            || (m_ControllerType == 2 && !Input.GetKey(ControllerMap.PS_BUTTON_L1) && Input.GetKey(ControllerMap.PS_BUTTON_R1) && Input.GetKeyDown(ControllerMap.PS_BUTTON_X));*/

            // R1 + X / RB + A
            //m_ShortcutPressedDict[(int)EShortcuts.FAST_RESET] = Input.GetKeyDown(KeyCode.F)
            // || (m_ControllerType == 1 && !Input.GetKey(ControllerMap.XB_BUTTON_LB) && Input.GetKey(ControllerMap.XB_BUTTON_RB) && Input.GetKeyDown(ControllerMap.XB_BUTTON_B))
            // || (m_ControllerType == 2 && !Input.GetKey(ControllerMap.PS_BUTTON_L1) && Input.GetKey(ControllerMap.PS_BUTTON_R1) && Input.GetKeyDown(ControllerMap.PS_BUTTON_CIRCLE));
        }

        public bool IsShortcutPressed(EShortcuts shortcut)
        {
            return m_ShortcutPressedDict[(int)shortcut];
        }
    }
}