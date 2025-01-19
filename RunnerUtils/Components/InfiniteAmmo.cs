using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RunnerUtils.Components;

public static class InfiniteAmmo
{
    private static bool m_enabled;

    public static bool Enabled {
        get { return m_enabled; }
    }

    public static void Enable() {
        m_enabled = true;
        GameManager.instance.player.GetHUD().GetAmmoIndicator()
            .LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());
    }

    public static void Disable() {
        m_enabled = false;
        GameManager.instance.player.GetHUD().GetAmmoIndicator()
            .LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());

    }

    public static void Toggle() {
        m_enabled = !m_enabled;
        try {
            GameManager.instance.player.GetHUD().GetAmmoIndicator()
                .LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());
        }
        catch {
            // ignored
        }
    }

    private static void ColorAmmoSlots(List<HUDAmmoIndicatorSlot> slots, Color color) {
        foreach (var t in slots) {
            t.lowAmmoColor = color;
            t.LowAmmo();
        }
    }

    [HarmonyPatch(typeof(HUDAmmoIndicator), nameof(HUDAmmoIndicator.LoadInSlots))]
    public class PatchColorAmmoSlots
    {
        [HarmonyPostfix]
        public static void Postfix(ref List<HUDAmmoIndicatorSlot> ___spawnedSlots) {
            for (int i = 0; i < ___spawnedSlots.Count; ++i) {
                if (m_enabled) {
                    ColorAmmoSlots(___spawnedSlots, Color.red);
                }
                else {
                    ColorAmmoSlots(___spawnedSlots, Color.white);
                    ___spawnedSlots[i].lowAmmoColor = Color.yellow;
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
            }
            else {
                return true;
            }
        }
    }
}