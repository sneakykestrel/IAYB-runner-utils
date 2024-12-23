﻿using BepInEx;
using BepInEx.Configuration;
using Enemy;
using HarmonyLib;
using RunnerUtils.Components;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunnerUtils
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Mod : BaseUnityPlugin
    {
        public const string pluginGuid = "kestrel.iamyourbeast.runnerutils";
        public const string pluginName = "Runner Utils";
        public const string pluginVersion = "2.3.2";

        static InGameLog igl = new InGameLog($"{pluginName}~Ingame Log (v{pluginVersion})");
        static bool shouldResetScale;

        static Dictionary<string, KeyCode> defaultBindings = new Dictionary<string, KeyCode>();
        static Dictionary<string, ConfigEntry<KeyCode>> bindings = new Dictionary<string, ConfigEntry<KeyCode>>();

        public static ConfigEntry<float> throwCam_rangeScalar;
        public static ConfigEntry<bool> throwCam_unlockCamera;
        public static ConfigEntry<bool> throwCam_autoSwitch;
        public static ConfigEntry<bool> walkabilityOverlay;

        public static ConfigEntry<bool> saveLocation_verbose;

        public static ConfigEntry<bool> snowmanPercent;

        static string loadBearingColonThree = ":3";

        public void Awake() {
            this.gameObject.hideFlags = HideFlags.HideAndDontSave; //fuck you unity
            if (loadBearingColonThree != ":3") Application.Quit();

            defaultBindings.Add("Trigger Visibility Toggle", KeyCode.Comma);
            defaultBindings.Add("Force Trigger Visibility On", KeyCode.O);
            defaultBindings.Add("Force Trigger Visibility Off", KeyCode.I);
            defaultBindings.Add("OOB Box Visibility Toggle", KeyCode.Period);
            defaultBindings.Add("Start Trigger Visibility Toggle", KeyCode.Slash);
            defaultBindings.Add("Spawner Visibility Toggle", KeyCode.M);
            defaultBindings.Add("Log Visibility Toggle", KeyCode.K);
            defaultBindings.Add("Clear Log", KeyCode.J);
            defaultBindings.Add("Toggle Infinite Ammo", KeyCode.L);
            defaultBindings.Add("Toggle Throw Cam", KeyCode.Semicolon);
            defaultBindings.Add("Save Location", KeyCode.LeftBracket);
            defaultBindings.Add("Load Location", KeyCode.RightBracket);
            defaultBindings.Add("Toggle timestop", KeyCode.RightShift);
            defaultBindings.Add("Toggle auto jump", KeyCode.P);
            defaultBindings.Add("Toggle advanced movement info", KeyCode.None);
            defaultBindings.Add("Toggle hard fall overlay", KeyCode.U);

            throwCam_unlockCamera = Config.Bind("Throw Cam", "Unlock Camera", false, "Unlock the camera when in throw cam");
            throwCam_rangeScalar = Config.Bind("Throw Cam", "Camera Range", 0.2f, new ConfigDescription("Follow range of the throw cam", new AcceptableValueRange<float>(0.01f, 3.0f)));
            throwCam_autoSwitch = Config.Bind("Throw Cam", "Auto Switch", false, "Automatically switch to throw cam when a projectile is thrown");
            walkabilityOverlay = Config.Bind("Walkability Overlay", "Enable", false, "Makes all walkable surfaces appear snowy, and all unwalkable surfaces appear black");

            saveLocation_verbose = Config.Bind("Location Save", "Verbose", false, "Log the exact location and rotation when a save or load is performed");

            snowmanPercent = Config.Bind("Snowman%", "Enable", true, "Displays your time upon destroying a snowman, to time the (silly) category snowman%");

            foreach (var binding in defaultBindings) {
                bindings.Add(binding.Key, Config.Bind("Bindings", binding.Key, binding.Value, new ConfigDescription("", new AcceptableValueRange<KeyCode>(KeyCode.None, KeyCode.Joystick8Button19)))); //surely this wont cause issues later
            }

            new Harmony(pluginGuid).PatchAll();
            Logger.LogInfo("Hiiiiiiiiiiii :3");
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update() {
            //mm if statements
            if (Input.GetKeyDown(bindings["Log Visibility Toggle"].Value)) {
                igl.ToggleVisibility();
                igl.LogLine($"Toggled log visibility");
            }
            if (Input.GetKeyDown(bindings["Clear Log"].Value)) {
                igl.Clear();
                igl.LogLine($"Cleared Log");
            }


            if (Input.GetKeyDown(bindings["Trigger Visibility Toggle"].Value)) {
                ShowTriggers.ToggleAll();
                igl.LogLine($"Toggled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
            if (Input.GetKeyDown(bindings["Force Trigger Visibility On"].Value)) {
                ShowTriggers.ShowAll();
                igl.LogLine($"Enabled all triggers' visibility");
                FairPlay.triggersModified = true;
            }
            if (Input.GetKeyDown(bindings["Force Trigger Visibility Off"].Value)) {
                ShowTriggers.HideAll();
                igl.LogLine($"Disabled all triggers' visibility");
                FairPlay.triggersModified = false;
            }
            if (Input.GetKeyDown(bindings["OOB Box Visibility Toggle"].Value)) {
                ShowTriggers.ToggleAllOf<PlayerOutOfBoundsBox>();
                igl.LogLine($"Toggled OOB boxes' visibility");
                FairPlay.triggersModified = true;
            }
            if (Input.GetKeyDown(bindings["Start Trigger Visibility Toggle"].Value)) {
                ShowTriggers.ToggleAllOf<PlayerTimerStartBox>();
                igl.LogLine($"Toggled start triggers' visibility");
                FairPlay.triggersModified = true;
            }
            if (Input.GetKeyDown(bindings["Spawner Visibility Toggle"].Value)) {
                ShowTriggers.ToggleAllOf<EnemySpawner>();
                igl.LogLine($"Toggled spawners' visibility");
                FairPlay.triggersModified = true;
            }


            if (Input.GetKeyDown(bindings["Toggle Infinite Ammo"].Value)) {
                if (!GameManager.instance.player.GetHUD()) return;
                InfiniteAmmo.Toggle();
                igl.LogLine($"Toggled infinite ammo");
                FairPlay.infiniteAmmo = InfiniteAmmo.Enabled;
            }


            if (Input.GetKeyDown(bindings["Toggle Throw Cam"].Value)) {
                if (ThrowCam.cameraAvailable) {
                    ThrowCam.ToggleCam();
                    igl.LogLine($"Toggled throw cam");
                } else {
                    igl.LogLine($"Unable to switch to throw cam ~ no thrown weapons are in the air");
                }
            }

            if (Input.GetKeyDown(bindings["Save Location"].Value)) {
                LocationSave.SaveLocation();
                igl.LogLine($"Saved location {(saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
                FairPlay.locationSaved = true;
            }
            if (Input.GetKeyDown(bindings["Load Location"].Value)) {
                if (LocationSave.Location != Vector3.zero) {
                    LocationSave.RestoreLocation();
                    igl.LogLine($"Loaded previous location {(saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
                } else {
                    igl.LogLine("No location saved!");
                }
            }
            if (Input.GetKeyDown(bindings["Load Location"].Value) && Input.GetKeyDown(bindings["Save Location"].Value)) {
                LocationSave.ClearLocation();
                igl.LogLine($"Cleared saved location");
                FairPlay.locationSaved = false;
            }

            if (GameManager.instance.timeManager != null && shouldResetScale) {
                PauseTime.Reset();
                shouldResetScale = false;
            }
            if (Input.GetKeyDown(bindings["Toggle timestop"].Value)) {
                PauseTime.Toggle();
                igl.LogLine($"Toggled timestop");
                FairPlay.timePaused = PauseTime.Enabled;
            }


            if (Input.GetKeyDown(bindings["Toggle auto jump"].Value)) {
                AutoJump.Toggle();
                igl.LogLine($"Toggled auto jump");
                FairPlay.autoJump = AutoJump.Enabled;
            }

            if (Input.GetKeyDown(bindings["Toggle hard fall overlay"].Value)) {
                MovementDebug.Toggle();
                igl.LogLine($"Toggled hf overlay");
                FairPlay.hfOverlay = !FairPlay.hfOverlay;
            }
            if (Input.GetKeyDown(bindings["Toggle advanced movement info"].Value)) {
                MovementDebug.ToggleAdvanced();
                igl.LogLine($"Toggled movement info");
            }
            
            FairPlay.Update();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (mode == LoadSceneMode.Additive) ShowTriggers.ExtendRegistry();
            shouldResetScale = true;
        }

        [HarmonyPatch(typeof(Player))]
        public class PatchPlayer
        {
            [HarmonyPatch("Initialize")]
            [HarmonyPostfix]
            public static void PlayerInitPostfix() {
                ShowTriggers.RegisterAll();
                igl.Setup();
                ThrowCam.Reset();
                FairPlay.Init();
                MovementDebug.Init();
            }
        }

        [HarmonyPatch(typeof(PlayerMovement))]
        public class PatchPlayerMovement
        {
            [HarmonyPatch("Initialize")]
            [HarmonyPostfix]
            public static void MovementInitPostfix(CharacterController ___controller) {
                if (walkabilityOverlay.Value) {
                    foreach (Terrain t in FindObjectsByType<Terrain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                        TerrainData td = t.terrainData;
                        WalkabilityOverlay.MakeWalkabilityTex(ref td, ___controller.slopeLimit);
                        t.terrainData = td;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HUDLevelTimer), "Update")]
        public class The
        {
            [HarmonyPostfix]
            public static void Postfix(ref TMP_Text ___gradeText) {
                ___gradeText.text = $" {___gradeText.text}";
            }
        }
    }
}
