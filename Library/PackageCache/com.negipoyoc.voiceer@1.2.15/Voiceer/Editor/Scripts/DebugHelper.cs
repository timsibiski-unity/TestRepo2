using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Voiceer
{
    public static class DebugHelper
    {
        [MenuItem("Window/Voiceer/Debug/Enable Debug Log", priority = 200)]
        static void EnableDebugLog()
        {
            SessionState.SetBool("EditorHook.logDebug", true);
            SessionState.SetBool("SoundPlayer.logDebug", true);
            //EditorHook.logDebug = true;
            //SoundPlayer.logDebug = true;
        }

        [MenuItem("Window/Voiceer/Debug/Disable Debug Log", priority = 201)]
        static void DisableDebugLog()
        {
            SessionState.SetBool("EditorHook.logDebug", false);
            SessionState.SetBool("SoundPlayer.logDebug", false);
            //EditorHook.logDebug = false;
            //SoundPlayer.logDebug = false;
        }
    }
}
