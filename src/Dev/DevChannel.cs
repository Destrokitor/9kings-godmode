// Canal de teste: só compilado com `dotnet build -p:Dev=1`. Lê comandos de $GODMODE_DEV_DIR/9k_cmd.txt.
using System;
using System.IO;
using System.Linq;
using Core;
using Difficulty;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NineKingsGodMode.Dev
{
    public class DevChannel : MonoBehaviour
    {
        static string Dir => Environment.GetEnvironmentVariable("GODMODE_DEV_DIR");

        public static void Attach(GameObject host)
        {
            if (!string.IsNullOrEmpty(Dir)) host.AddComponent<DevChannel>();
        }

        void Update()
        {
            string cmdFile = Path.Combine(Dir, "9k_cmd.txt");
            if (!File.Exists(cmdFile)) return;
            string cmd = File.ReadAllText(cmdFile).Trim();
            File.Delete(cmdFile);
            string result;
            try { result = Run(cmd); } catch (Exception e) { result = "ERR " + e; }
            File.WriteAllText(Path.Combine(Dir, "9k_out.txt"), result ?? "null");
        }

        static string Run(string cmd)
        {
            var p = cmd.Split(' ');
            string arg = cmd.Length > p[0].Length ? cmd.Substring(p[0].Length + 1) : "";
            switch (p[0])
            {
                case "scene": return SceneManager.GetActiveScene().name + " god=" + GodMode.Active;
                case "godstart":
                    GodMode.SetPending(true);
                    Wave.StartNewGame(p[1], DifficultySO.Instance.GetSelectedDifficulty());
                    return "started";
                case "open": GodMenu.Open(); return "open=" + GodMenu.IsOpen;
                case "press":
                    foreach (var b in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                    {
                        var t = b.GetComponentInChildren<TMP_Text>();
                        if (t != null && t.text == arg && b.gameObject.activeInHierarchy) { b.onClick.Invoke(); return "pressed"; }
                    }
                    return "no button";
                case "hover":
                    foreach (var tr in UnityEngine.Object.FindObjectsByType<TooltipTrigger>(FindObjectsSortMode.None))
                        if (tr.Title?.Invoke() == arg) { tr.OnPointerEnter(null); return "hover " + arg; }
                    return "no trigger";
                case "tip":
                {
                    var box = GameObject.Find("GodModeMenu/Tooltip/Box");
                    if (box == null) return "no box";
                    return string.Join("\n", box.GetComponentsInChildren<RectTransform>(true).Select(r =>
                        r.name + " active=" + r.gameObject.activeSelf + " size=" + r.rect.size + " pos=" + r.anchoredPosition +
                        (r.GetComponent<TMP_Text>() is TMP_Text tt ? " text=" + tt.text.Substring(0, Math.Min(60, tt.text.Length)) + " col=" + tt.color + " pref=" + tt.preferredHeight : "")));
                }
                case "triggers":
                    return string.Join(", ", UnityEngine.Object.FindObjectsByType<TooltipTrigger>(FindObjectsSortMode.None).Take(40).Select(t => t.Title?.Invoke()));
            }
            return "unknown";
        }
    }
}
