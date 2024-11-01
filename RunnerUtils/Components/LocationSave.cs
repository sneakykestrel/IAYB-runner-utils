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
        static Vector3 savedLocation;
        static Vector3 savedRotation;

        public static Vector3 Location { get { return savedLocation; } }
        public static Vector3 Rotation { get { return savedRotation; } }
        public static string StringLoc { get { return $"l{Location}@r{Rotation}"; } }

        public static void SaveLocation() {
            savedLocation = GameManager.instance.player.GetPosition();
            savedRotation = GameManager.instance.player.GetLookScript().GetBaseRotation();
        }

        public static void RestoreLocation() {
            GameManager.instance.player.GetMovementScript().Teleport(savedLocation);
            GameManager.instance.player.GetLookScript().SetBaseRotation(savedRotation);
        }
    }
}
