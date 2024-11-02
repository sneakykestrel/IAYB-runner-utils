﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RunnerUtils.Components
{
    public class FairPlay
    {
        static InGameLog igl = new InGameLog($" ", 10);
        public static bool triggersModified = false;
        public static bool infiniteAmmo = false;
        public static bool locationSaved = false;
        public static bool timePaused = false;
        public static bool autoJump = false;

        public static void Init() {
            igl.anchoredPos = new Vector2(800, 567);
            igl.textAlignment = TMPro.TextAlignmentOptions.TopRight;
            igl.Setup();
        }

        public static void Update() {
            igl.Clear();
            if (triggersModified) igl.LogLine("Triggers");
            if (infiniteAmmo) igl.LogLine("Ammo");
            if (locationSaved) igl.LogLine("Location Saved");
            if (timePaused) igl.LogLine("Time Paused");
            if (autoJump) igl.LogLine("Auto Jump");
            igl.FlushBuffer();
        }
    }
}