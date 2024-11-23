using Enemy;
using Equipment;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace RunnerUtils
{
    public static class ShowTriggers
    {
        static Texture2D tex = new Texture2D(1, 1);
        static Material mat = new Material(Shader.Find("Sprites/Default"));
        static Color spawnerColor = Color.magenta;
        static Color placedEquipmentColor = new Color(1, 0, 0.5f, 0.5f);
        static Dictionary<Type, Color> triggerColors = new Dictionary<Type, Color>();
        static Dictionary<string, Color> extraColors = new Dictionary<string, Color>();
        static List<GameObject> registry = new List<GameObject>();

        static ShowTriggers() {
            mat.SetTexture("_MainTex", tex);

            triggerColors.Add(typeof(PlayerOutOfBoundsBox), new Color(1, 0.5f, 0, 0.5f));
            triggerColors.Add(typeof(PlayerTimerStartBox), new Color(0.5f, 0, 1, 0.5f));
            triggerColors.Add(typeof(KillPlayerEventTrigger), new Color(1, 0, 0, 0.5f));
            extraColors.Add("Spawn", new Color(1, 1, 0, 0.5f));
            extraColors.Add("SpawnEnemy", new Color(1, 1, 0, 0.5f));
        }

        public static void ShowAllOf<T>() where T : MonoBehaviour {
            for (int i = registry.Count - 1; i >= 0; --i) {
                if (!registry[i]) {
                    registry.RemoveAt(i);
                    continue;
                }
                if (registry[i].TryGetComponent<T>(out _)) registry[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }

        public static void HideAllOf<T>() where T : MonoBehaviour {
            for (int i = registry.Count - 1; i >= 0; --i) {
                if (!registry[i]) {
                    registry.RemoveAt(i);
                    continue;
                }
                if (registry[i].TryGetComponent<T>(out _)) registry[i].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        public static void ToggleAllOf<T>() where T : MonoBehaviour {
            for (int i = registry.Count - 1; i >= 0; --i) {
                if (!registry[i]) {
                    registry.RemoveAt(i);
                    continue;
                }
                if (registry[i].TryGetComponent<T>(out _)) {
                    var renderer = registry[i].GetComponent<MeshRenderer>();
                    renderer.enabled = !renderer.enabled;
                }
            }
        }

        public static void ShowAll() {
            for (int i = registry.Count - 1; i >= 0; --i) {
                if (!registry[i]) {
                    registry.RemoveAt(i);
                    continue;
                }
                foreach (Transform child in registry[i].transform)
                    if (child.gameObject.name == "SpawnerTrace") child.gameObject.SetActive(true);
                registry[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }

        public static void HideAll(){
            for (int i = registry.Count - 1; i >= 0; --i) {
                if (!registry[i]) {
                    registry.RemoveAt(i);
                    continue;
                }
                foreach (Transform child in registry[i].transform)
                    if (child.gameObject.name == "SpawnerTrace") child.gameObject.SetActive(false);
                registry[i].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        public static void ToggleAll() {
            for (int i = registry.Count - 1; i >= 0; --i) {
                if (!registry[i]) {
                    registry.RemoveAt(i);
                    continue;
                }
                var renderer = registry[i].GetComponent<MeshRenderer>();
                foreach (Transform child in registry[i].transform)
                    if (child.gameObject.name == "SpawnerTrace") child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
                renderer.enabled = !renderer.enabled;
            }
        }
        public static void RegisterAllOf<T>() where T : MonoBehaviour {
            registry.Clear();
            foreach (var obj in GameObject.FindObjectsOfType<EventTriggerBoxPlayer>(true)) {
                if (obj.gameObject.GetComponent<T>())
                    RegisterObj(obj);
            }
        }

        //horrible nested loops . yay
        public static void RegisterAllExcluding(Type[] Ts) {
            registry.Clear();
            foreach (var obj in GameObject.FindObjectsOfType<EventTriggerBoxPlayer>(true)) {
                bool yeah = true;
                foreach (Type t in Ts) {
                    if (obj.gameObject.GetComponent(t)) {
                        yeah = false;
                        break;
                    }
                }
                if (yeah) RegisterObj(obj);
            }
        }

        public static void RegisterAll() {
            registry.Clear();
            foreach (var obj in GameObject.FindObjectsOfType<EventTriggerBoxPlayer>(true)) {
                RegisterObj(obj);
            }
        }

        private static void MakeVisible(GameObject obj, Color boxColor) {
            BoxCollider collider;
            if (!obj.TryGetComponent<BoxCollider>(out collider)) return;

            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            MeshFilter filter;
            if (!obj.TryGetComponent<MeshFilter>(out filter)) {
                filter = obj.AddComponent<MeshFilter>();
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh cubeMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.Destroy(cube);
            Mesh scaled = UnityEngine.Object.Instantiate(cubeMesh);

            Vector3[] verts = scaled.vertices;
            for (int i = 0; i < verts.Length; ++i) {
                verts[i] = Vector3.Scale(verts[i], collider.size) + collider.center;
            }
            scaled.vertices = verts;
            scaled.RecalculateBounds();
            filter.mesh = scaled;

            renderer.material = mat;

            renderer.enabled = false; //disable by default
            renderer.material.color = boxColor;
        }

        private static void RegisterObj(EventTriggerBoxPlayer obj){
            

            Color col = new Color(0, 1, 0, 0.25f);

            UnityEvent onTrigger = AccessTools.Field(typeof(EventTriggerBoxPlayer), "onTrigger").GetValue(obj) as UnityEvent;
            int eventCount = onTrigger.GetPersistentEventCount();

            for (int i = 0; i < eventCount; ++i) {

                //add spawner trace objects
                string methodName = onTrigger.GetPersistentMethodName(i);


                if (methodName == "Spawn" || methodName == "SpawnEnemy") {
                    var target = (onTrigger.GetPersistentTarget(i));
                    if (!target) continue;

                    GameObject targetObj;
                    try {
                        targetObj = ((MonoBehaviour)target).gameObject;
                    } catch {
                        //this exists because ONE trigger on permafrost is wired up wrong
                        continue;
                    }

                    var spawners = new List<EnemySpawner>(targetObj.GetComponentsInChildren<EnemySpawner>(true));

                    foreach (var spawner in spawners) {
                        var child = new GameObject("SpawnerTrace");
                        child.transform.SetParent(obj.gameObject.transform);

                        var line = child.AddComponent<LineRenderer>();
                        line.material = mat;
                        line.startColor = extraColors[methodName];
                        line.endColor = spawnerColor;
                        line.positionCount = 2;
                        line.SetPosition(0, obj.gameObject.transform.position);
                        line.SetPosition(1, spawner.gameObject.transform.position);
                        child.SetActive(false);
                    }

                }

                if (extraColors.ContainsKey(onTrigger.GetPersistentMethodName(i)))
                    col = extraColors[onTrigger.GetPersistentMethodName(i)];
            }

            foreach (var kv in triggerColors) {
                if (obj.gameObject.GetComponent(kv.Key)) col = kv.Value;
            }

            MakeVisible(obj.gameObject, col);
            registry.Add(obj.gameObject);
        }

        [HarmonyPatch(typeof(EnemySpawner))]
        public class ShowSpawners
        {
            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            public static void StartPostfix(EnemySpawner __instance) {
                if (__instance.gameObject.TryGetComponent<MeshRenderer>(out MeshRenderer renderer)) {
                    renderer.material.SetColor("_BaseColor", spawnerColor);
                }
                __instance.gameObject.SetActive(true);
                registry.Add(__instance.gameObject);
            }

            [HarmonyPatch("SpawnEnemy")]
            [HarmonyPostfix]
            public static void SpawnPostfix(EnemySpawner __instance, ref bool __state) {
                __instance.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(PlacedEquipment), nameof(PlacedEquipment.Initialize))]
        public class ShowEquipment
        {
            [HarmonyPostfix]
            public static void PlacedPostfix(ref PlacedEquipment __instance) {
                MakeVisible(__instance.gameObject, placedEquipmentColor);
                registry.Add(__instance.gameObject);
            }
        }
    }
}
