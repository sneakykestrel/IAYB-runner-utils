using HarmonyLib;
using BepInEx;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enemy;
using System.IO;
using UnityEngine.UI;
using CurvedUI;
using RunnerUtils.Components;
using BepInEx.Unity.Mono.Bootstrap;

namespace RunnerUtils
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Mod : BaseUnityPlugin
    {
        const string pluginGuid = "kestrel.iamyourbeast.runnerutils";
        const string pluginName = "Runner Utils";
        const string pluginVersion = "1.1.0";

        static InGameLog igl = new InGameLog($"{pluginName}~Ingame Log (v{pluginVersion})");

        static Dictionary<string, KeyCode> defaultBindings = new Dictionary<string, KeyCode>();
        static Dictionary<string, KeyCode> bindings = new Dictionary<string, KeyCode>();

        public static ConfigEntry<float> throwCam_rangeScalar;
        public static ConfigEntry<bool> throwCam_unlockCamera;
        public static ConfigEntry<bool> throwCam_autoSwitch;

        public static ConfigEntry<bool> saveLocation_verbose;

        static string loadBearingColonThree = ":3";

        public void Awake() {
            if (loadBearingColonThree != ":3") Application.Quit();

            defaultBindings.Add("Trigger Visibility Toggle", KeyCode.Comma);
            defaultBindings.Add("OOB Box Visibility Toggle", KeyCode.Period);
            defaultBindings.Add("Start Trigger Visibility Toggle", KeyCode.Slash);
            defaultBindings.Add("Spawner Visibility Toggle", KeyCode.M);

            defaultBindings.Add("Log Visibility Toggle", KeyCode.K);
            defaultBindings.Add("Clear Log", KeyCode.J);

            defaultBindings.Add("Toggle Infinite Ammo", KeyCode.L);

            defaultBindings.Add("Toggle Throw Cam", KeyCode.Semicolon);

            defaultBindings.Add("Save Location", KeyCode.LeftBracket);
            defaultBindings.Add("Load Location", KeyCode.RightBracket);

            throwCam_unlockCamera = Config.Bind("Throw Cam", "Unlock Camera", false, "Unlock the camera when in throw cam");
            throwCam_rangeScalar = Config.Bind("Throw Cam", "Camera Range", 0.2f, "Follow range of the throw cam");
            throwCam_autoSwitch = Config.Bind("Throw Cam", "Auto Switch", false, "Automatically switch to throw cam when a projectile is thrown");

            saveLocation_verbose = Config.Bind("Location Save", "Verbose", false, "Log the exact location and rotation when a save or load is performed");

            foreach (var binding in defaultBindings) {
                bindings.Add(binding.Key, (Config.Bind("Bindings", binding.Key, binding.Value, new ConfigDescription("", new AcceptableValueRange<KeyCode>(KeyCode.None, KeyCode.Joystick8Button19)))).Value); //surely this wont cause issues later
            }

            Logger.LogInfo("Hiiiiiiiiiiii :3");
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }

        public void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Update() {
            //mm if statements
            if (Input.GetKeyDown(bindings["Log Visibility Toggle"])) {
                igl.ToggleVisibility();
                igl.LogLine($"Toggled log visibility");
            }
            if (Input.GetKeyDown(bindings["Clear Log"])) {
                igl.Clear();
                igl.LogLine($"Cleared Log");
            }


            if (Input.GetKeyDown(bindings["Trigger Visibility Toggle"])) {
                ShowTriggers.ToggleAll();
                igl.LogLine($"Toggled all triggers' visibility");
            }
            if (Input.GetKeyDown(bindings["OOB Box Visibility Toggle"])) {
                ShowTriggers.ToggleAllOf<PlayerOutOfBoundsBox>();
                igl.LogLine($"Toggled OOB boxes' visibility");
            }
            if (Input.GetKeyDown(bindings["Start Trigger Visibility Toggle"])) {
                ShowTriggers.ToggleAllOf<PlayerTimerStartBox>();
                igl.LogLine($"Toggled start triggers' visibility");
            }
            if (Input.GetKeyDown(bindings["Spawner Visibility Toggle"])) {
                ShowTriggers.ToggleAllOf<EnemySpawner>();
                igl.LogLine($"Toggled spawners' visibility");
            }


            if (Input.GetKeyDown(bindings["Toggle Infinite Ammo"])) {
                if (!GameManager.instance.player.GetHUD()) return;
                InfiniteAmmo.Toggle();
                igl.LogLine($"Toggled infinite ammo");
            }


            if (Input.GetKeyDown(bindings["Toggle Throw Cam"])) {
                if (ThrowCam.cameraAvailable) {
                    ThrowCam.ToggleCam();
                    igl.LogLine($"Toggled throw cam");
                } else {
                    igl.LogLine($"Unable to switch to throw cam ~ no thrown weapons are in the air");
                }
            }

            if (Input.GetKeyDown(bindings["Save Location"])) {
                LocationSave.SaveLocation();
                igl.LogLine($"Saved location {(saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
            }
            if (Input.GetKeyDown(bindings["Load Location"])) {
                LocationSave.RestoreLocation();
                igl.LogLine($"Loaded previous location {(saveLocation_verbose.Value ? LocationSave.StringLoc : "")}");
            }
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ShowTriggers.RegisterAll();
        }

        [HarmonyPatch(typeof(Player))]
        public class PatchPlayer
        {
            [HarmonyPatch("Initialize")]
            [HarmonyPostfix]
            public static void PlayerInitPostfix() {
                igl.Setup();
                if (LocationSave.Location == Vector3.zero) LocationSave.SaveLocation();
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
