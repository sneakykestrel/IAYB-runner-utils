using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunnerUtils.Components
{
    public class AutoJump
    {
        private static bool m_enabled = false;

        public static bool Enabled { get { return m_enabled; } }

        public static void Toggle() {
            m_enabled = !m_enabled;
        }

        [HarmonyPatch(typeof(PlayerMovement), "UpdateJumpCheck")]
        public static class PatchAutoJump
        {
            [HarmonyPrefix]
            public static bool Prefix(ref PlayerMovement __instance) {
                if (!m_enabled) return true;
                bool perchDetachment = (bool)AccessTools.Method(typeof(PlayerMovement), "BlockPerchDetachment").Invoke(__instance, null);
                if (__instance.CanJump() && !perchDetachment && GameManager.instance.inputManager.jump.Held()) {
                    AccessTools.Method(typeof(PlayerMovement), "Jump").Invoke(__instance, null);
                }
                return false;
            }
        }
    }
}
