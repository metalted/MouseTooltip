using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MouseTooltip
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class BaseZeepKistPlugin : BaseUnityPlugin
    {
        public const string pluginGuid = "com.metalted.zeepkist.mousetooltip";
        public const string pluginName = "Mouse Tooltip";
        public const string pluginVersion = "1.0";

        public ConfigEntry<string> tooltipLanguage;

        public void Awake()
        {
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {pluginName} is loaded!");

            tooltipLanguage = Config.Bind("General", "Language", "default", "Language for the tooltip. If not available will show the -default- set.");

            TooltipHandler.Init((string) tooltipLanguage.BoxedValue);
        }

        public void OnGUI()
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.box);
            labelStyle.wordWrap = true;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = Mathf.FloorToInt(Screen.height / 40);
            labelStyle.normal.textColor = Color.white;

            if (TooltipHandler.visible)
            {
                if (!string.IsNullOrEmpty(TooltipHandler.currentText))
                {
                    GUIContent labelContent = new GUIContent(TooltipHandler.currentText);
                    Vector2 labelSize = labelStyle.CalcSize(labelContent);
                    int padding = Mathf.CeilToInt(Screen.width / 200f);
                    Vector2 newSize = new Vector2(labelSize.x + padding, labelSize.y + padding);
                    Rect boxRect = new Rect(0, 0, 0, 0);
                    boxRect.width = newSize.x;
                    boxRect.height = newSize.y;


                    Vector2 normalPosition = new Vector2(Input.mousePosition.x + 50, Screen.height - Input.mousePosition.y - 50);
                    if (normalPosition.x + boxRect.width > Screen.width)
                    {
                        boxRect.position = new Vector2(Input.mousePosition.x - 50 - boxRect.width, Screen.height - Input.mousePosition.y - 50);
                        GUI.Box(boxRect, labelContent, labelStyle);
                    }
                    else
                    {
                        boxRect.position = normalPosition;
                        GUI.Box(boxRect, labelContent, labelStyle);
                    }
                }
            }
        }
    }

    public static class TooltipHandler
    {
        public static string currentText = "";
        public static bool visible = false;
        public static bool enabled = false;
        public static Dictionary<string, Dictionary<string, string>> data;
        public static Dictionary<string, string> lang;

        public static void Init(string language)
        {
            string pluginsFolder = AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins";
            string languageFileName = "MouseTooltipLang.json";
            string[] files = Directory.GetFiles(pluginsFolder, languageFileName, SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                string jsonData = File.ReadAllText(files[0]);
                data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonData);

                if (data.ContainsKey(language))
                {
                    lang = data[language];
                    enabled = true;
                    Debug.Log("Mouse Tooltip Language Loaded: " + language + ".");
                }
                else if (data.ContainsKey("default"))
                {
                    lang = data["default"];
                    enabled = true;

                    Debug.Log("Mouse Tooltip Language Loaded: default.");
                }
            }
            else
            {
                Debug.LogError("Tooltips not loaded because MouseTooltipLang.json wasn't found!");
            }
        }

        public static string GetTranslation(string inputString)
        {
            if (lang.ContainsKey(inputString))
            {
                return lang[inputString];
            }
            else
            {
                return "";
            }
        }

        public static void OnButtonEnter(LEV_CustomButton button)
        {
            if (!enabled) { return; }

            Image img = button.GetComponent<Image>();            

            if (img != null)
            {
                string translation = GetTranslation(img.activeSprite.name);

                if (translation != "")
                {
                    currentText = translation;
                    visible = true;
                }
            }
        }

        public static void OnButtonExit()
        {
            if (!enabled) { return; }

            visible = false;
        }
    }

    [HarmonyPatch(typeof(LEV_CustomButton), "OnPointerEnter")]
    public class ButtonEnter
    {
        public static void Postfix(LEV_CustomButton __instance)
        {
            TooltipHandler.OnButtonEnter(__instance);
        }
    }

    [HarmonyPatch(typeof(LEV_CustomButton), "OnPointerExit")]
    public class ButtonExit
    {
        public static void Postfix(LEV_CustomButton __instance)
        {
            TooltipHandler.OnButtonExit();
        }
    }
}