using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace WhistleJester
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static AudioClip Audio;
        public static bool ConstTimer = true;

        private void Awake()
        {
            ConstTimer = Config.Bind("General", "ConstTimer", true, "Sets the Jester's popUpTimer field to line up with the song").Value;

            string location = Info.Location.TrimEnd($"{PluginInfo.PLUGIN_GUID}.dll".ToCharArray());
            UnityWebRequest audioLoader = UnityWebRequestMultimedia.GetAudioClip($"File://{location}WhatHaveIDone.mp3", AudioType.MPEG);

            audioLoader.SendWebRequest();
            while (!audioLoader.isDone) { }

            if (audioLoader.result == UnityWebRequest.Result.Success)
            {
                Audio = DownloadHandlerAudioClip.GetContent(audioLoader);

                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll(typeof(JesterAudioPatch));
                if (ConstTimer)
                {
                    harmony.PatchAll(typeof(JesterAIPatch));
                }
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            }
            else
            {
                Logger.LogError($"Could not load audio file");
            }
        }
    }

    [HarmonyPatch(typeof(JesterAI))]
    internal class JesterAudioPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void AudioPatch(JesterAI __instance)
        {
            __instance.popGoesTheWeaselTheme = Plugin.Audio;
        }
    }

    [HarmonyPatch(typeof(JesterAI))]
    internal class JesterAIPatch
    {
        [HarmonyPatch("SetJesterInitialValues")]
        [HarmonyPostfix]
        public static void ForceTime(JesterAI __instance)
        {
            __instance.popUpTimer = 41.5f;
        }
    }
}
