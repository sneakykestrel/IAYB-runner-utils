using HarmonyLib;
using BepInEx;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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

namespace RunnerUtils.Components
{
    public class ThrowCam
    {
        private static Camera m_cam;
        private static Camera m_oldCam;
        private static GameObject m_obj;
        private static Vector3 m_motion;

        public static bool cameraAvailable;

        public static void UpdatePos(Transform anchor) {
            m_obj.transform.position = (anchor.position-(m_motion*Mod.throwCam_rangeScalar.Value));
            if (Mod.throwCam_unlockCamera.Value) {
                m_obj.transform.rotation = GameManager.instance.cameraManager.GetArmCamera().transform.rotation;
            } else {
                m_obj.transform.LookAt(anchor);
            }
        }

        public static void Reset() {
            cameraAvailable = false;
            GameManager.instance.player.GetHUD().GetReticle().gameObject.SetActive(true);
            m_cam.enabled = false;
            m_oldCam.enabled = true;
        }

        public static void ToggleCam() {
            m_oldCam.enabled = !m_oldCam.enabled;
            m_cam.enabled = !m_cam.enabled;
            GameObject reticle = GameManager.instance.player.GetHUD().GetReticle().gameObject;
            reticle.SetActive(!reticle.activeInHierarchy);
        }

        [HarmonyPatch(typeof(PlayerWeaponToss))]
        public static class InitCam
        {
            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            public static void StartPostfix(ref Transform ___tiltAnchor) {
                m_oldCam = GameManager.instance.cameraManager.GetManagersCamera();
                m_obj = new GameObject();

                m_cam = m_obj.AddComponent<Camera>();
                m_cam.GetUniversalAdditionalCameraData().cameraStack.Add(m_oldCam.GetUniversalAdditionalCameraData().cameraStack[1]);
                cameraAvailable = true;
                m_cam.enabled = false;
                if (Mod.throwCam_autoSwitch.Value) {
                    ToggleCam();
                }
            }

            [HarmonyPatch("Update")]
            [HarmonyPostfix]
            public static void UpdatePostfix(ref bool ___hitSurface, ref Transform ___tiltAnchor) {
                if (___hitSurface) {
                    return;
                }
                UpdatePos(___tiltAnchor);
            }

            [HarmonyPatch("OnCollisionEnter")]
            [HarmonyPostfix]
            public static void CollisionEnterPostfix() {
                Reset();
            }

            [HarmonyPatch("FixedUpdate")]
            [HarmonyPostfix]
            public static void FixedUpdatePostfix(float ___speed, float ___gravity, Transform ___tiltAnchor) {
                Vector3 a = ___tiltAnchor.parent.transform.forward * ___speed;
                a += Vector3.down * ___gravity;
                m_motion = a;
            }
        }

    }
}
