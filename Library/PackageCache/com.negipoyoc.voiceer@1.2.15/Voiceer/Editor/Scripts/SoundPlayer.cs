using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Voiceer
{
    public static class SoundPlayer
    {
        static bool LogDebug => SessionState.GetBool("SoundPlayer.logDebug", false);

        public static VoicePreset CurrentVoicePreset => VoiceerEditorUtility.GetStorageSelector()?.CurrentVoicePreset;
        public static bool VoiceSelectorExists => VoiceerEditorUtility.GetStorageSelector() != null;

        public static void PlaySound(Hook hook)
        {
            //VoicePresetがあるか
            if (CurrentVoicePreset == null)
            {
                if (LogDebug)
                    Debug.Log("Current Voice Preset was null");

                return;
            }

            //Clipが存在するか
            var clip = CurrentVoicePreset.GetRandomClip(hook);
            if (clip == null)
            {
                if (LogDebug)
                    Debug.Log("Couldn't find a voice clip for " + hook);

                return;
            }

            if (LogDebug)
                Debug.Log("Attempting to play a sound for " + hook + "\nClip name: " + clip.name + "\n");

            //ボリューム調整が有効か
            if(VoicePresetSelectorEditor.isVolumeControlEnabled)
            {
                PlaySoundClipExperimental(clip, VoicePresetSelectorEditor.volume);
            }
            else
            {
                PlaySoundClip(clip);
            }
        }

        private static void PlaySoundClip(AudioClip clip)
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;

            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            var method = audioUtilClass.GetMethod//
            (
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
#if UNITY_2019_2_OR_NEWER
                new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
#else
                new Type[] { typeof(AudioClip) },
#endif
                null
            );

            if (method == null)
            {
                Debug.LogError("Method is null!");
            }

#if UNITY_2019_2_OR_NEWER
            //Debug.Log(clip);
            //Debug.Log(method);
            method.Invoke(null, new object[] { clip, 0, false });
#else
            method.Invoke(null, new object[] {clip});
#endif
        }

        private static float Decibel2Liner(float db)
        {
            return Mathf.Pow(10f, db/20f);
        }

        /// <summary>
        /// An alternate PlaySoundClip method that allows the user to change the clip's volume.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="db"></param>
        private static void PlaySoundClipExperimental(AudioClip clip, float db)
        {
            var voiceerAudio = new GameObject("VoiceerAudio");
            voiceerAudio.hideFlags = HideFlags.HideAndDontSave;
            var audioSource = voiceerAudio.AddComponent<AudioSource>();
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            var volume = Decibel2Liner(db);
            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] = samples[i] * volume;
            }

            var adjustedClip = AudioClip.Create("VolumeAdjusted_" + clip.name, clip.samples, clip.channels, clip.frequency, false);
            adjustedClip.SetData(samples, 0);
            audioSource.clip = adjustedClip;
            audioSource.Play();

            //this callback will destroy the new AudioSource once it's done playing
            EditorApplication.CallbackFunction removeOnPlayed = null;
            removeOnPlayed = () =>
            {
                if (!audioSource.isPlaying)
                {
                    if (EditorHook.playModeState != PlayModeStateChange.ExitingEditMode &&
                        EditorHook.playModeState != PlayModeStateChange.ExitingPlayMode)
                    {
                        if (LogDebug)
                            Debug.Log("Removing audio source because it's no longer playing\nPlay mode state: " + EditorHook.playModeState + "\n");

                        EditorApplication.update -= removeOnPlayed;
                        UnityEngine.Object.DestroyImmediate(voiceerAudio);
                    }
                }
            };

            //assign the callback to editor update
            EditorApplication.update += removeOnPlayed;
        }
    }
}