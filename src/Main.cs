using System;
using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using UnityModManagerNet;

namespace Aristocrat
{
    public static class Main
    {
        public static UnityModManager.ModEntry ModEntry;
        public static Harmony HarmonyInstance;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            modEntry.OnGUI = OnGUI;

            try
            {
                HarmonyInstance.PatchAll();
                Log("Aristocrat loaded.");
            }
            catch (Exception ex)
            {
                Log("Harmony patch failed: " + ex);
                return false;
            }

            return true;
        }

        public static void Log(string message)
        {
            ModEntry?.Logger.Log(message);
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            UnityEngine.GUILayout.Label("Adds the Aristocrat inquisitor archetype, based on Faith Hunter.");
            UnityEngine.GUILayout.Label("Judgments are limited to Purity, Protection, and Smiting. Teamwork feat selections at 15th and 18th level are removed.");
            UnityEngine.GUILayout.Label("Martial Weapon Proficiency, Weapon Focus at 1st level, Greater Weapon Focus at 10th level, and Right to Rule from 4th level.");
            UnityEngine.GUILayout.Label("Create a new Inquisitor and choose Aristocrat.");
        }
    }

    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
    static class BlueprintsReadyPatch
    {
        static void Postfix()
        {
            try
            {
                Content.Create();
            }
            catch (Exception ex)
            {
                Main.Log("Aristocrat setup failed: " + ex);
            }
        }
    }
}
