﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunnerUtils.Components
{
    public class SnowmanPercent
    {
        [HarmonyPatch(typeof(Snowman), "Kill")]
        public class PatchSnowman
        {
            [HarmonyPostfix]
            public static void Postfix() {
                float time = GameManager.instance.levelController.GetCombatTimer().GetTime();
                GameManager.instance.player.GetHUD().GetNotificationPopUp().TriggerPopUp($"Snowman%: {time:0.00}", HUDNotificationPopUp.ThreatLevel.High);
            }
        }
    }
}
