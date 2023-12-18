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

                new Harmony(PluginInfo.PLUGIN_GUID).PatchAll(typeof(JesterPatch));
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            }
            else
            {
                Logger.LogError($"Could not load audio file");
            }
        }
    }

    [HarmonyPatch(typeof(JesterAI))]
    internal class JesterPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void AudioPatch(JesterAI __instance)
        {
            __instance.popGoesTheWeaselTheme = Plugin.Audio;
        }

        [HarmonyPatch("SetJesterInitialValues")]
        [HarmonyPostfix]
        public static void ForceTime(JesterAI __instance)
        {
            if (Plugin.ConstTimer)
            {
                __instance.popUpTimer = 41.5f;
            }
        }
    }
}