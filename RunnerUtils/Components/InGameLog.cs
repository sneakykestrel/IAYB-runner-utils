﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace RunnerUtils
{
    public class InGameLog
    {
        //TODO: proper errors . just returning probably isnt the best idea

        private int m_bufferLen = 15;
        private bool m_shouldBeVisible = true;
        public int fadeLen = 4;
        public Vector2 anchoredPos = new Vector2(-925, 535);
        public Vector2 sizeDelta = new Vector2(500, 250);
        public TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft;
        private TextMeshProUGUI m_textComponent;
        private GameObject m_gameObj;
        string m_name;
        CircularArray<string> buffer;

        public int Length {
            get => m_bufferLen;
            set {
                m_bufferLen = value;
                buffer.MaxLength = value;
            }
        }

        public InGameLog(string name = "Unnamed Log",
                            int bufferLength = 15,
                            int fadeLength = 4) {
            m_name = name;
            m_bufferLen = bufferLength;
            fadeLen = fadeLength;
            buffer = new CircularArray<string>(m_bufferLen);
        }

        public void ToggleVisibility() {
            m_textComponent.enabled = !m_textComponent.enabled;
            m_shouldBeVisible = m_textComponent.enabled;
        }
        public void Show() {
            m_textComponent.enabled = true;
            m_shouldBeVisible = true;
        }
        public void Hide() {
            m_textComponent.enabled = false;
            m_shouldBeVisible = false;
        }

        //Logging (flushes the buffer after every call)
        public void LogLine(string message) {
            if (m_textComponent == null) return;
            buffer.Push(message + "\n");
            FlushBuffer();
        }

        public void Log(string message) {
            if (m_textComponent == null) return;
            buffer.Push(message);
            FlushBuffer();
        }

        //Pushing (does not flush buffer after every call)
        public void PushBufferLine(string message) {
            if (m_textComponent == null) return;
            buffer.Push(message + "\n");
        }

        public void PushBuffer(string message) {
            if (m_textComponent == null) return;
            buffer.Push(message);
        }

        public void Clear() {
            while (buffer.Length > 0) buffer.Pop();
            FlushBuffer();
        }

        //For like. persistent lines i guess
        public void SetBufferLine(int idx, string message) {
            if (idx >= buffer.Length) buffer.Length = idx + 1;
            buffer[idx] = message+"\n";
        }

        //times i have been completely stumped trying to debug this awful method: 2
        //i *really* need to stop using ternaries this much
        public void FlushBuffer() {
            if (m_textComponent == null) return;
            m_textComponent.text = $"{m_name}\n";
            buffer.ForEach((string val, int i) => {
                m_textComponent.text += (i<m_bufferLen-fadeLen?val:$"<alpha=#{((255*(m_bufferLen-i)/fadeLen)):X2}>"+val); //dont worry about it
            });
        }

        public void Setup() {
            var gps = GameManager.instance.player.GetHUD().GetGPS();

            //ew
            if (m_gameObj) {
                if (!m_gameObj.activeInHierarchy)
                    m_gameObj = GameObject.Instantiate(new GameObject(), gps.transform.parent.gameObject.transform);
            } else {
                m_gameObj = GameObject.Instantiate(new GameObject(), gps.transform.parent.gameObject.transform);
            }

            m_gameObj.name = $"{m_name} (Ingame Log)";

            //ew again
            var transform = m_gameObj.GetComponent<RectTransform>();
            if (!transform) transform = m_gameObj.AddComponent<RectTransform>();

            transform.anchoredPosition = anchoredPos;
            transform.sizeDelta = sizeDelta;

            var tmp = gps.transform.parent.GetComponentInChildren<TextMeshProUGUI>();

            m_textComponent = m_gameObj.GetComponent<TextMeshProUGUI>();
            if (!m_textComponent) m_textComponent = m_gameObj.AddComponent<TextMeshProUGUI>();

            m_textComponent.alignment = textAlignment;
            m_textComponent.font = tmp.font;
            m_textComponent.fontSize = 24;
            m_textComponent.enabled = m_shouldBeVisible;

            FlushBuffer();
        }

        private class CircularArray<T>
        {
            int len;
            T[] array;
            int startIndex;

            public CircularArray(int length) {
                array = new T[length];
                len = 0;
            }

            int GetIdx(int index) {
                return (startIndex + index)%array.Length;
            }

            public T this[int idx] {
                get => array[GetIdx(idx)];
                set => array[GetIdx(idx)] = value;
            }

            public void Push(T value) {
                array[GetIdx(len)] = value;
                if (len == array.Length) {
                    ++startIndex;
                    if (startIndex == array.Length) startIndex = 0;
                } else 
                    ++len;
            }

            public T Pop() {
                return array[GetIdx(--len)];
            }

            public int Length {
                get => len;
                set {
                    if (value > len) {
                        for (int i = len; i < value; ++i) {
                            array[i] = default(T);
                        }
                    }
                    len = value;
                }
            }

            public int MaxLength {
                get => array.Length;
                set {
                    if (value <= 0 || value == array.Length) return;

                    var newArr = new T[value];
                    var top = Math.Min(value, array.Length);
                    for (int i = 0; i < top; ++i) {
                        newArr[i] = array[GetIdx(i)];
                    }
                    startIndex = 0;
                    array = newArr;
                }
            }

            public void ForEach(Action<T, int> callback) {
                for (int i = 0; i < len; ++i)
                    callback(this[i], i);
            }

            public void ReverseForEach(Action<T, int> callback) {
                for (int i = len-1; i >= 0; --i)
                    callback(this[i], i);
            }
        }
    }

}
