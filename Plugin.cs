using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections;

namespace Cardio
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class BaseZeepKistPlugin : BaseUnityPlugin
    {
        public const string pluginGuid = "com.metalted.zeepkist.basezeepkistplugin";
        public const string pluginName = "BaseZeepkistPlugin";
        public const string pluginVersion = "1.0";

        public void Awake()
        {
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {pluginName} is loaded!");
        }
    }
}