using AimAssist;
using HarmonyLib;
using RunnerUtils.Components;
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
        private static bool m_enabled;

        public static bool Enabled { get { return m_enabled; } }

        public static void Enable() {
            m_enabled = true;
            GameManager.instance.player.GetHUD().GetAmmoIndicator().LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());
        }

        public static void Disable() {
            m_enabled = false;
            GameManager.instance.player.GetHUD().GetAmmoIndicator().LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());

        }

        public static void Toggle() {
            m_enabled = !m_enabled;
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
                    if (m_enabled) {
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
                if (m_enabled) {
                    __result = true;
                    return false;
                } else {
                    return true;
                }
            }
        }
    }
}
