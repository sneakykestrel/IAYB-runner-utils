using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunnerUtils
{
    public static class InfiniteAmmo
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

        [HarmonyPatch(typeof(PlayerArmManager))]
        public class PatchInfiniteThrows
        {
            // sorry i cant hear you complaining about mod incompatibilities from inside my cool awesome transpiler
            [HarmonyPatch(nameof(PlayerArmManager.TossWeapon))]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; ++i) {
                    if (codes[i].Calls(AccessTools.Method(typeof(PlayerArmManager), "SpawnUnarmed"))) {
                        codes.RemoveAt(i);
                        codes.InsertRange(i, new CodeInstruction[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_1),
                            CodeInstruction.Call(typeof(UnityEngine.Object), "Instantiate", new Type[] { typeof(WeaponPickup) }),
                            CodeInstruction.Call(typeof(PlayerArmManager), "EquipWeapon", new Type[] { typeof(WeaponPickup) })
                        });
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}
