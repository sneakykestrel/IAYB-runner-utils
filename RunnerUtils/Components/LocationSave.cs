using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunnerUtils.Components
{
    public class LocationSave
    {
        static Vector3 savedLocation = Vector3.zero;
        static Vector3 savedRotation = Vector3.zero;

        public static Vector3 Location { get { return savedLocation; } }
        public static Vector3 Rotation { get { return savedRotation; } }
        public static string StringLoc { get { return $"<color=red>l{Location}<color=white>@<color=blue>r{Rotation}</color>"; } }

        public static void SaveLocation() {
            savedLocation = GameManager.instance.player.GetPosition();
            savedRotation = GameManager.instance.player.GetLookScript().GetBaseRotation();
        }

        public static void ClearLocation() {
            savedLocation = Vector3.zero;
            savedRotation = Vector3.zero;
        }

        public static void RestoreLocation() {
            GameManager.instance.player.GetMovementScript().Teleport(savedLocation);
            GameManager.instance.player.GetLookScript().SetBaseRotation(savedRotation);
        }
    }
}
