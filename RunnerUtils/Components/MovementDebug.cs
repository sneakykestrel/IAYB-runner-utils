using HarmonyLib;
using TMPro;
using UnityEngine;

namespace RunnerUtils.Components;

//awful code that i will rewrite when i am not incredibly tired :3
public static class MovementDebug
{
    private static bool m_advanced = false;
    private static bool m_simple = false;

    private static GameObject m_obj;
    private static TextMeshProUGUI m_text;

    private static InGameLog igl = new InGameLog($"{Mod.pluginName}~Movement Info", 10);

    public static void Init() {
        igl.anchoredPos = new Vector2(-425, 535);
        igl.Setup();
        if (!m_advanced) igl.Hide();

        SetupIndicator();
    }

    private static void SetupIndicator() {
        var gps = GameManager.instance.player.GetHUD().GetGPS();
        m_obj = Object.Instantiate(new GameObject(), gps.transform.parent.gameObject.transform);
        var font = gps.transform.parent.GetComponentInChildren<TextMeshProUGUI>().font;

        m_obj.name = "RunnerUtils~HF Indicator";
        m_text = m_obj.AddComponent<TextMeshProUGUI>();
        m_text.fontSize = 35;
        m_text.font = font;
        m_text.alignment = TextAlignmentOptions.BottomLeft;

        var transform = m_obj.GetComponent<RectTransform>();
        if (!transform) transform = m_obj.AddComponent<RectTransform>();

        transform.anchoredPosition = new Vector2(-1085, -645);
    }

    public static void ToggleAdvanced() {
        m_advanced = !m_advanced;
        igl.ToggleVisibility();
    }

    public static void Toggle() {
        m_simple = !m_simple;
    }

    [HarmonyPatch(typeof(PlayerMovement), "Update")]
    public class LogMovement
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerMovement __instance, ref int ___dropOnEnemyFrames, ref int ___fallLevel, ref float ___coyoteTimer, ref float ___vMomentum) {
            m_obj.SetActive(m_simple);
            if (m_simple) m_text.text = __instance.IsHardFalling()?"<color=#FF0000>HF": "<color=#ED424240>HF";
            if (!m_advanced) return;
            igl.SetBufferLine(0, $"Hard falling: {__instance.IsHardFalling()}");
            igl.SetBufferLine(1, $"Drop frames: {___dropOnEnemyFrames}");
            igl.SetBufferLine(2, $"Fall level: {___fallLevel}");
            igl.SetBufferLine(3, $"Coyote: {___coyoteTimer:0.00}");
            igl.SetBufferLine(4, $"vMomentum: {___vMomentum:0.00}");
            igl.FlushBuffer();
        }
    }
}