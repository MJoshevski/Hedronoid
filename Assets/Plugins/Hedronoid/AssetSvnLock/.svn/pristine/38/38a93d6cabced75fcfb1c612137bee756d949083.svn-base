using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;

public class NotifyForLockedFiles : UnityEditor.AssetModificationProcessor
{
	public static string[] OnWillSaveAssets(string[] paths)
	{
		List<string> pathsToSave = new List<string>();

		for (int i = 0; i < paths.Length; ++i)
		{
			FileInfo info = new FileInfo(paths[i]);
			if (info.IsReadOnly && info.Extension == ".unity")
			{
				if (UnityEditor.EditorUtility.DisplayDialog("Can not save.", 
					"Sorry, but " + paths[i] + " is Read-Only! \n Need lock this file on SVN.", "Get Lock", "Cancel"))
				{
					try
					{
						var processInfo = new ProcessStartInfo("c:/Program Files/TortoiseSVN/bin/TortoiseProc.exe", "/command:lock /path:\"" + paths[i] +"\"");

						processInfo.CreateNoWindow = true;
						processInfo.UseShellExecute = false;
						processInfo.RedirectStandardOutput = true;
						processInfo.RedirectStandardError = true;
						
						var process = Process.Start(processInfo);

						// string outputCommand = process.StandardOutput.ReadToEnd();
						// UnityEngine.Debug.Log("Output command: " + outputCommand);

						process.WaitForExit();

						// Save file if get lock on svn was successful
						// if (process.ExitCode == 0)
						// {
						// 	pathsToSave.Add(paths[i]);
						// }

						process.Close();

					}
					catch (Exception e)
					{
						UnityEngine.Debug.LogError(e);
					}
				}
			}
			else
			{
				pathsToSave.Add(paths[i]);
			}
				
		}

		return pathsToSave.ToArray();
	}
}
