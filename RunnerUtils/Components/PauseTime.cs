using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunnerUtils.Components;

public static class PauseTime
{
    private static TimeScale m_timeScale;

    public static bool Enabled { get; private set; } = false;

    public static void Toggle() {
        m_timeScale.SetScale(Mathf.Abs(m_timeScale.GetScale()-1f));
        Enabled = !Enabled;
    }

    public static void Reset() {
        m_timeScale = GameManager.instance.timeManager.CreateTimeScale(1f);
        Enabled = false;
    }
}