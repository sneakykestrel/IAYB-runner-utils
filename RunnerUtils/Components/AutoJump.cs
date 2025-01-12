using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunnerUtils.Components;

public static class AutoJump
{
    public static bool Enabled { get; private set; } = false;

    public static void Toggle() => Enabled = !Enabled;

    [HarmonyPatch(typeof(PlayerMovement), "UpdateJumpCheck")]
    public static class PatchAutoJump
    {
        [HarmonyPrefix]
        public static bool Prefix(ref PlayerMovement __instance) {
            if (!Enabled) return true;
            var perchDetachment = (bool)AccessTools.Method(typeof(PlayerMovement), "BlockPerchDetachment").Invoke(__instance, null);
            if (__instance.CanJump() && !perchDetachment && GameManager.instance.inputManager.jump.Held()) {
                AccessTools.Method(typeof(PlayerMovement), "Jump").Invoke(__instance, null);
            }
            return false;
        }
    }
}