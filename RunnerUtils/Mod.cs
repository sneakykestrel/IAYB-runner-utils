using HarmonyLib;
using BepInEx;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using Enemy;
using RunnerUtils.Components;

namespace RunnerUtils
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Mod : BaseUnityPlugin
    {
        const string pluginGuid = "kestrel.iamyourbeast.runnerutils";
        const string pluginName = "Runner Utils";
        const string pluginVersion = "1.2.7";

        static InGameLog igl = new InGameLog($"{pluginName}~Ingame Log (v{pluginVersion})");
        static bool shouldResetScale;

        static Dictionary<string, KeyCode> defaultBindings = new Dictionary<string, KeyCode>();
        static Dictionary<string, ConfigEntry<KeyCode>> bindings = new Dictionary<string, ConfigEntry<KeyCode>>();

        public static ConfigEntry<float> throwCam_rangeScalar;
        public static ConfigEntry<bool> throwCam_unlockCamera;
        public static ConfigEntry<bool> throwCam_autoSwitch;

        public static ConfigEntry<bool> saveLocation_verbose;

        static string lastSceneName = "";
        static string loadBearingColonThree = ":3";

        public void Awake() {
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

            throwCam_unlockCamera = Config.Bind("Throw Cam", "Unlock Camera", false, "Unlock the camera when in throw cam");
            throwCam_rangeScalar = Config.Bind("Throw Cam", "Camera Range", 0.2f, new ConfigDescription("Follow range of the throw cam", new AcceptableValueRange<float>(0.01f, 3.0f)));
            throwCam_autoSwitch = Config.Bind("Throw Cam", "Auto Switch", false, "Automatically switch to throw cam when a projectile is thrown");

            saveLocation_verbose = Config.Bind("Location Save", "Verbose", false, "Log the exact location and rotation when a save or load is performed");

            foreach (var binding in defaultBindings) {
                bindings.Add(binding.Key, Config.Bind("Bindings", binding.Key, binding.Value, new ConfigDescription("", new AcceptableValueRange<KeyCode>(KeyCode.None, KeyCode.Joystick8Button19)))); //surely this wont cause issues later
            }

            Logger.LogInfo("Hiiiiiiiiiiii :3");
            new Harmony(pluginGuid).PatchAll();
        }

        public void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Update() {
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

            FairPlay.Update();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ShowTriggers.RegisterAll();
            lastSceneName = scene.name;
            shouldResetScale = true;
        }

        [HarmonyPatch(typeof(Player))]
        public class PatchPlayer
        {
            [HarmonyPatch("Initialize")]
            [HarmonyPostfix]
            public static void PlayerInitPostfix() {
                igl.Setup();
                FairPlay.Init();
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
