using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    public class ActionHook : MonoBehaviour
    {
		public Action[] fixedUpdateActions;
		public Action[] updateActions;

        private void Start()
        {
            if (fixedUpdateActions == null && updateActions == null)
                return;

            if (fixedUpdateActions != null)
            {
                for (int i = 0; i < fixedUpdateActions.Length; i++)
                {
                    fixedUpdateActions[i].Execute_Start();
                }
            }

            if (updateActions != null)
            {
                for (int i = 0; i < updateActions.Length; i++)
                {
                    updateActions[i].Execute_Start();
                }
            }
        }

        void FixedUpdate()
		{
			if (fixedUpdateActions == null)
				return;

			for (int i = 0; i < fixedUpdateActions.Length; i++)
			{
				fixedUpdateActions[i].Execute();
			}
		}

		void Update()
        {
			if (updateActions == null)
				return;

            for (int i = 0; i < updateActions.Length; i++)
            {
                updateActions[i].Execute();
            }
        }
    }
}
