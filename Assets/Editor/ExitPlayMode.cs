/*
	Description:当检测到代码更改时,自动停止播放,防止Unity卡死
*/
using UnityEngine;
using UnityEditor;

namespace UnityFramework
{
    [InitializeOnLoad] // Make static initialiser be called as soon as the scripts are initialised in the editor (rather than just in play mode).
    public class ExitPlayModeOnScriptCompile
    {
        private static ExitPlayModeOnScriptCompile _instance = null;

        // Static initialiser called by Unity Editor whenever scripts are loaded (editor or play mode)
        static ExitPlayModeOnScriptCompile()
        {
            Unused(_instance);
            _instance = new ExitPlayModeOnScriptCompile();
        }

        private ExitPlayModeOnScriptCompile()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        ~ExitPlayModeOnScriptCompile()
        {
            EditorApplication.update -= OnEditorUpdate;
            // Silence the unused variable warning with an if.
            _instance = null;
        }

        // Called each time the editor updates.
        private static void OnEditorUpdate()
        {
            if (EditorApplication.isPlaying && EditorApplication.isCompiling)
            {
                Debug.Log("Exiting play mode due to script compilation.");
                EditorApplication.isPlaying = false;
            }
        }

        // Used to silence the 'is assigned by its value is never used' warning for _instance.
        private static void Unused<T>(T unusedVariable)
        {

        }
    }
}
