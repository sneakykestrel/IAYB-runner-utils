using AimAssist;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunnerUtils
{
    public class InfiniteAmmo
    {
        public static bool enabled;

        public static void Enable() {
            enabled = true;
            GameManager.instance.player.GetHUD().GetAmmoIndicator().LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());
        }

        public static void Disable() {
            enabled = false;
            GameManager.instance.player.GetHUD().GetAmmoIndicator().LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());

        }

        public static void Toggle() {
            enabled = !enabled;
            try {
                GameManager.instance.player.GetHUD().GetAmmoIndicator().LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());
            } catch { }
        }

        private static void ColorAmmoSlots(ref List<HUDAmmoIndicatorSlot> slots, Color color) {
            for (int i = 0; i < slots.Count; ++i) {
                AccessTools.Field(typeof(HUDAmmoIndicatorSlot), "lowAmmoColor").SetValue(slots[i], color);
                slots[i].LowAmmo();
            }
        }

        [HarmonyPatch(typeof(HUDAmmoIndicator), nameof(HUDAmmoIndicator.LoadInSlots))]
        public class PatchColorAmmoSlots
        {
            [HarmonyPostfix]
            public static void Postfix(ref List<HUDAmmoIndicatorSlot> ___spawnedSlots) {
                for (int i = 0; i < ___spawnedSlots.Count; ++i) {
                    if (enabled) {
                        ColorAmmoSlots(ref ___spawnedSlots, Color.red);
                    } else {
                        ColorAmmoSlots(ref ___spawnedSlots, Color.white);
                        AccessTools.Field(typeof(HUDAmmoIndicatorSlot), "lowAmmoColor").SetValue(___spawnedSlots[i], Color.yellow);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DebugManager), nameof(DebugManager.GetInfiniteAmmo))]
        public class PatchInfiniteAmmo
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result) {
                if (enabled) {
                    __result = true;
                    return false;
                } else {
                    return true;
                }
            }
        }
    }
}
