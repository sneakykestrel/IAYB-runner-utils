using HarmonyLib;

namespace RunnerUtils.Components;

public class AutoJump : ComponentBase<AutoJump>
{
    public override string Identifier => "Auto Jump";
    public override bool ShowOnFairPlay => true;
    
    [HarmonyPatch(typeof(PlayerMovement), "UpdateJumpCheck")]
    public static class PatchAutoJump
    {
        [HarmonyPrefix]
        public static bool Prefix(ref PlayerMovement __instance) {
            if (!Instance.enabled) return true;
            var perchDetachment = __instance.BlockPerchDetachment();
            if (__instance.CanJump() && !perchDetachment && GameManager.instance.inputManager.jump.Held()) {
                __instance.Jump();
            }
            return false;
        }
    }
}