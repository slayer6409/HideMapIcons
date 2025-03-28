﻿using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HideMapIcons
{
    [BepInPlugin("Slayer.HideMapIcons", "Hide Map Icons", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource? Log;
        public static ConfigFile? BepInExConfig = null!;
        public static ConfigEntry<string>? RemovedMapKeywords;
        public static ConfigEntry<string>? BlacklistMapKeywords;


        void Awake()
        {
            Log = Logger;
            Log.LogInfo("Hide Upgrade Map Icons Started");
            BepInExConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "HideMapIcons.cfg"), true);
            RemovedMapKeywords = BepInExConfig.Bind("General",
                                                    "BlockedMapKeywords",
                                                    "UPGRADE",
                                                    "Comma-separated list of keywords to block map icons");
            BlacklistMapKeywords = BepInExConfig.Bind("General",
                                                    "BlacklistMapKeywords",
                                                    "",
                                                    "Comma-separated list of keywords to blacklist map icons from removing, so if you put Stamina here but upgrade there, it will hide all upgrades except ones that contain \"stamina\"");
            Harmony harmony = new Harmony("Slayer.HideMapIcons");
            harmony.PatchAll();
           
        }
       
    }
    [HarmonyPatch(typeof(Map), nameof(Map.AddCustom))]
    public class Patch_Map_AddCustom
    {
        static bool Prefix(MapCustom mapCustom, Sprite sprite, Color color)
        {
            string name = mapCustom.name.ToUpper();
            var blocked = Plugin.RemovedMapKeywords!.Value
                .Split(',')
                .Select(k => k.Trim().ToUpper())
                .Where(k => !string.IsNullOrEmpty(k));

            var blacklist = Plugin.BlacklistMapKeywords!.Value
                .Split(',')
                .Select(k => k.Trim().ToUpper())
                .Where(k => !string.IsNullOrEmpty(k));

            var attributes = mapCustom.gameObject.GetComponent<ItemAttributes>();
            if (attributes != null) 
            {
                var field = attributes.GetType().GetField("itemName", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var value = field.GetValue(attributes) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        name = value.ToUpper();
                    }
                }
            }


            if (blocked.Any(k => name.Contains(k)) && !blacklist.Any(k => name.Contains(k)))
            {
                return false;
            }
            return true; 
        }
    }
}