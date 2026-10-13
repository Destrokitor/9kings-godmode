using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NineKingsGodMode
{
    // Objeto persistente do mod: troca de cenas, botão DEUS e console do jogo.
    public class GodModeHost : MonoBehaviour
    {
        GameObject m_Hud;

        void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            if (scene.name == "KingSelection" && !KingSelectionToggle.Reloading) GodMode.OnLeftKingSelection();
        }

        void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            Debug.Log("[GodMode] cena " + from.name + " -> " + to.name);
            if (to.name == "MainMenu") GodMode.OnLeftGameplay();
        }

        void Update()
        {
            bool inGodRun = GodMode.Active && Wave.OnGameplay;
            if (Wave.Options != null) Wave.Options.EnableGameConsole = GodMode.Active;

            if (inGodRun && m_Hud == null) m_Hud = CreateHud();
            if (m_Hud != null) m_Hud.SetActive(inGodRun && !GodMenu.IsOpen);
        }

        // Botão "DEUS" no canto superior esquerdo, abaixo do ouro.
        static GameObject CreateHud()
        {
            var go = new GameObject("GodModeHud");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 29000;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2292.74f, 1440f);
            scaler.matchWidthOrHeight = 1f;
            go.AddComponent<GraphicRaycaster>();

            var btn = Ui.Button(go.transform, "DEUS", GodMenu.Open, 210, 76, 44, Ui.Mint, "GodModeButton");
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(40, -130);
            rt.sizeDelta = new Vector2(210, 76);
            return go;
        }
    }
}
