using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunnerUtils.Components;

public static class LocationSave
{
    public static Vector3? savedPosition;
    public static Vector3? savedRotation;
    public static string StringLoc { get { return $"<color=red>l{savedPosition}<color=white>@<color=blue>r{savedRotation}</color>"; } }
    public static void SaveLocation() {
        savedPosition = GameManager.instance.player?.GetPosition();
        savedRotation = GameManager.instance.player?.GetLookScript().GetBaseRotation();
        if (!savedPosition.HasValue || !savedRotation.HasValue) return;
    }

    public static void ClearLocation() {
        savedPosition = null;
        savedRotation = null;
    }

    public static void RestoreLocation() {
        if (savedPosition.HasValue)
            GameManager.instance.player?.GetMovementScript().Teleport(savedPosition!.Value);
        if (savedRotation.HasValue)
            GameManager.instance.player?.GetLookScript().SetBaseRotation(savedRotation!.Value);
    }
}