using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunnerUtils.Components
{
    public class PauseTime
    {
        private static TimeScale m_timeScale;
        private static bool m_enabled = false;

        public static bool Enabled { get { return m_enabled; } }

        public static void Toggle() {
            m_timeScale.SetScale(Mathf.Abs(m_timeScale.GetScale()-1f));
            m_enabled = !m_enabled;
        }

        public static void Reset() {
            m_timeScale = GameManager.instance.timeManager.CreateTimeScale(1f);
            m_enabled = false;
        }
    }
}
