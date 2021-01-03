using System.Collections;
using System.Collections.Generic;
using Hedronoid;
using UnityEngine;

namespace Hedronoid.AI
{
	public class HNDAI 
	{
        static HNDAISettings s_Settings;

		public static HNDAISettings Settings
        {
            get
            {
                Init();
                return s_Settings;
            }
        }

        static bool s_IsInitialized = false;
		
		// Init
        static void Init()
        {
            if (s_IsInitialized || !Application.isPlaying) return;

            s_Settings = HNDScriptableObjectExtensions.LoadOrCreateScriptableObjectInResources<HNDAISettings>(HNDAISettings.AssetName);
            s_IsInitialized = true;
        }
	}
}
