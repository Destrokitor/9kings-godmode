using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NineKingsGodMode
{
    // Balão com o efeito do item, no estilo dos popups do jogo (caixa escura, borda branca).
    // Aparece quando o mouse fica parado sobre o item por um instante.
    public class Tooltip : MonoBehaviour
    {
        const float Delay = 0.45f;
        const float Width = 720f;

        static Tooltip s_Instance;

        RectTransform m_Canvas, m_Box;
        TextMeshProUGUI m_Title, m_Subtitle, m_Body;
        RectTransform m_Target;
        float m_ShowAt;

        public static void Create(RectTransform canvasRoot)
        {
            var go = new GameObject("Tooltip", typeof(RectTransform));
            go.transform.SetParent(canvasRoot, false);
            s_Instance = go.AddComponent<Tooltip>();
            s_Instance.Build(canvasRoot);
        }

        void Build(RectTransform canvasRoot)
        {
            m_Canvas = canvasRoot;
            m_Box = Ui.Box(transform, "Box", 0.96f);
            m_Box.pivot = new Vector2(0, 1);
            m_Box.anchorMin = m_Box.anchorMax = new Vector2(0.5f, 0.5f);
            m_Box.sizeDelta = new Vector2(Width, 100);
            Ui.VLayout(m_Box, 6, TextAnchor.UpperLeft, 26);
            foreach (var g in m_Box.GetComponentsInChildren<Graphic>()) g.raycastTarget = false;

            m_Title = Line(50, Ui.Mint, bold: true);
            m_Subtitle = Line(30, Ui.Dim, bold: false);
            m_Body = Line(38, Color.white, bold: false);
            m_Box.gameObject.SetActive(false);
        }

        TextMeshProUGUI Line(float size, Color color, bool bold)
        {
            var t = Ui.Text(m_Box, "", size, color, bold, TextAlignmentOptions.TopLeft);
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            t.richText = true;
            Ui.Size(t, Width - 52);
            return t;
        }

        public static void Request(RectTransform target, string title, string subtitle, string body)
        {
            if (s_Instance == null || string.IsNullOrWhiteSpace(body) && string.IsNullOrWhiteSpace(title)) return;
            var t = s_Instance;
            t.m_Target = target;
            t.m_ShowAt = Time.unscaledTime + Delay;
            t.m_Title.text = title ?? "";
            t.m_Subtitle.text = subtitle ?? "";
            t.m_Subtitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(subtitle));
            t.m_Body.text = string.IsNullOrWhiteSpace(body) ? "Sem descrição." : body;
            t.m_Box.gameObject.SetActive(false);
        }

        public static void Cancel(RectTransform target)
        {
            if (s_Instance == null || s_Instance.m_Target != target) return;
            s_Instance.m_Target = null;
            s_Instance.m_Box.gameObject.SetActive(false);
        }

        void Update()
        {
            if (m_Target == null || !m_Target.gameObject.activeInHierarchy)
            {
                if (m_Box.gameObject.activeSelf) m_Box.gameObject.SetActive(false);
                m_Target = null;
                return;
            }
            if (!m_Box.gameObject.activeSelf && Time.unscaledTime >= m_ShowAt)
            {
                m_Box.gameObject.SetActive(true);
                FitHeight();
                Place();
            }
        }

        // Altura = conteúdo (o sprite de fundo informa um tamanho mínimo grande demais para ContentSizeFitter).
        void FitHeight()
        {
            float h = 26 * 2;
            int lines = 0;
            foreach (var t in new[] { m_Title, m_Subtitle, m_Body })
            {
                if (!t.gameObject.activeSelf) continue;
                h += t.GetPreferredValues(t.text, Width - 52, 0).y;
                lines++;
            }
            h += 6 * Mathf.Max(0, lines - 1);
            m_Box.sizeDelta = new Vector2(Width, h);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Box);
        }

        // Ao lado do item (direita; esquerda se não couber), sem sair da tela.
        void Place()
        {
            var corners = new Vector3[4];
            m_Target.GetWorldCorners(corners);
            Vector2 min = m_Canvas.InverseTransformPoint(corners[0]);
            Vector2 max = m_Canvas.InverseTransformPoint(corners[2]);
            var half = m_Canvas.rect.size / 2f;
            float h = m_Box.rect.height;
            const float gap = 18f;

            float x = max.x + gap;
            if (x + Width > half.x - 10) x = min.x - gap - Width;
            float y = Mathf.Clamp(max.y, -half.y + h + 10, half.y - 10);
            m_Box.anchoredPosition = new Vector2(x, y);
            m_Box.SetAsLastSibling();
        }
    }

    // Colocado nos itens do menu: pede o balão ao passar o mouse.
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Func<string> Title, Subtitle, Body;

        public void OnPointerEnter(PointerEventData e)
        {
            string title = "", sub = "", body = "";
            try { title = Title?.Invoke(); sub = Subtitle?.Invoke(); body = Body?.Invoke(); }
            catch (Exception ex) { Debug.LogWarning("[GodMode] descrição: " + ex.Message); }
            Tooltip.Request((RectTransform)transform, title, sub, body);
        }

        public void OnPointerExit(PointerEventData e) => Tooltip.Cancel((RectTransform)transform);

        void OnDisable() => Tooltip.Cancel((RectTransform)transform);
    }
}
