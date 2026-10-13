using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UserInterface;

namespace NineKingsGodMode
{
    // Peças de interface montadas com as mesmas fontes, sprites e cores do 9 Kings.
    public static class Ui
    {
        public static readonly Color Mint = new Color(0.458f, 0.962f, 0.718f, 1f);
        public static readonly Color ButtonTint = new Color(1f, 0.953f, 0.824f, 1f);
        public static readonly Color Red = new Color(1f, 0.33f, 0.33f, 1f);
        public static readonly Color Gold = new Color(1f, 0.78f, 0.25f, 1f);
        public static readonly Color Dim = new Color(1f, 1f, 1f, 0.55f);

        static TMP_FontAsset s_Bold, s_Regular;
        static readonly Dictionary<string, Sprite> s_Sprites = new Dictionary<string, Sprite>();

        public static TMP_FontAsset BoldFont => s_Bold ? s_Bold : (s_Bold = FindFont("TauSans Outlined Bold"));
        public static TMP_FontAsset RegularFont => s_Regular ? s_Regular : (s_Regular = FindFont("TauSans"));

        static TMP_FontAsset FindFont(string name)
        {
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            return fonts.FirstOrDefault(f => f.name == name) ?? fonts.FirstOrDefault();
        }

        public static Sprite Sprite(string name)
        {
            if (s_Sprites.TryGetValue(name, out var s) && s) return s;
            s = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name == name);
            s_Sprites[name] = s;
            return s;
        }

        // Objetos -----------------------------------------------------------------

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static void Stretch(RectTransform rt, float pad = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        public static LayoutElement Size(Component c, float w = -1, float h = -1, float flexW = -1)
        {
            var le = c.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
            if (w >= 0) { le.preferredWidth = w; le.minWidth = w; }
            if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            return le;
        }

        public static Image Image(Transform parent, string sprite, Color color, string name = "Image")
        {
            var img = Rect(name, parent).gameObject.AddComponent<Image>();
            img.sprite = Sprite(sprite);
            img.type = img.sprite && img.sprite.border != Vector4.zero ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            img.color = color;
            if (PpuMultiplier.TryGetValue(sprite, out var ppu)) img.pixelsPerUnitMultiplier = ppu;
            return img;
        }

        // Mesmos valores usados nas telas do jogo (bordas finas como no original).
        static readonly Dictionary<string, float> PpuMultiplier = new Dictionary<string, float>
        {
            { "BorderedBox_2", 61.2f }, { "RoundedRectangle", 46.09f }, { "BorderedBox", 18.5f },
        };

        public static TextMeshProUGUI Text(Transform parent, string text, float size, Color color, bool bold = false,
            TextAlignmentOptions align = TextAlignmentOptions.Center, string name = "Txt")
        {
            var t = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            t.font = bold ? BoldFont : RegularFont;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }

        // Caixa escura com contorno branco (mesma dos popups do jogo).
        public static RectTransform Box(Transform parent, string name = "Box", float alpha = 0.94f)
        {
            var fill = Image(parent, "RoundedRectangle", new Color(0, 0, 0, alpha), name);
            var outline = Image(fill.transform, "BorderedBox", new Color(1, 1, 1, 0.35f), "Outline");
            outline.raycastTarget = false;
            outline.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Stretch(outline.rectTransform);
            return fill.rectTransform;
        }

        public static HorizontalLayoutGroup HLayout(Component c, float spacing = 8, TextAnchor align = TextAnchor.MiddleCenter, int pad = 0)
        {
            var l = c.gameObject.AddComponent<HorizontalLayoutGroup>();
            l.spacing = spacing;
            l.childAlignment = align;
            l.childControlWidth = l.childControlHeight = true;
            l.childForceExpandWidth = l.childForceExpandHeight = false;
            l.padding = new RectOffset(pad, pad, pad, pad);
            return l;
        }

        public static VerticalLayoutGroup VLayout(Component c, float spacing = 8, TextAnchor align = TextAnchor.UpperCenter, int pad = 0)
        {
            var l = c.gameObject.AddComponent<VerticalLayoutGroup>();
            l.spacing = spacing;
            l.childAlignment = align;
            l.childControlWidth = l.childControlHeight = true;
            l.childForceExpandWidth = l.childForceExpandHeight = false;
            l.padding = new RectOffset(pad, pad, pad, pad);
            return l;
        }

        public static readonly Color TextIdle = new Color(0.72f, 0.76f, 0.74f, 1f);

        // Botão "caixa" (fundo escuro, contorno branco fino) - usado em +/-, cartas e terrenos.
        public static Button Button(Transform parent, string label, Action onClick, float w = 300, float h = 70,
            float fontSize = 34, Color? textColor = null, string name = "Btn")
        {
            var fill = Image(parent, "RoundedRectangle", new Color(0, 0, 0, 0.82f), name);
            var outline = Image(fill.transform, "BorderedBox", new Color(1, 1, 1, 0.55f), "Outline");
            outline.raycastTarget = false;
            outline.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Stretch(outline.rectTransform);
            var txt = Text(fill.transform, label, fontSize, textColor ?? Color.white, bold: true);
            Stretch(txt.rectTransform, 8);
            var btn = MakeButton(fill, txt, onClick);
            var fx = btn.gameObject.AddComponent<HoverFx>();
            fx.Outline = outline;
            fx.Label = txt;
            fx.HoverScale = 1.04f;
            Size(fill, w, h);
            return btn;
        }

        // Botão só de texto, como o menu de pausa do jogo (cinza -> branco no hover).
        public static Button TextButton(Transform parent, string label, Action onClick, float w = 300, float h = 70,
            float fontSize = 44, string name = "TextBtn")
        {
            var hit = Image(parent, "RoundedRectangle", new Color(0, 0, 0, 0f), name);
            var txt = Text(hit.transform, label, fontSize, TextIdle, bold: true);
            Stretch(txt.rectTransform, 4);
            var btn = MakeButton(hit, txt, onClick);
            var fx = btn.gameObject.AddComponent<HoverFx>();
            fx.Label = txt;
            fx.TintLabel = true;
            fx.HoverScale = 1.08f;
            Size(hit, w, h);
            return btn;
        }

        static Button MakeButton(Image target, TextMeshProUGUI txt, Action onClick)
        {
            var btn = target.gameObject.AddComponent<GButton>();
            btn.targetGraphic = target;
            btn.transition = Selectable.Transition.None;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
            btn.onClick.AddListener(() =>
            {
                try { onClick?.Invoke(); }
                catch (Exception e) { Debug.LogError("[GodMode] ação falhou: " + e); }
            });
            return btn;
        }

        public static void SetLabel(Button b, string label) => b.GetComponentInChildren<TextMeshProUGUI>().text = label;
    }

    // Efeito de hover dos botões (cor do texto/contorno e leve aumento).
    public class HoverFx : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
    {
        public Image Outline;
        public TextMeshProUGUI Label;
        public bool TintLabel;
        public float HoverScale = 1.05f;
        // Botão "ativo" (aba escolhida etc.): fica destacado em verde.
        public bool Active;

        bool m_Hover;

        void OnEnable() => Paint();

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e) { m_Hover = true; Paint(); }
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e) { m_Hover = false; Paint(); }

        public void SetActive(bool active) { Active = active; Paint(); }

        void Paint()
        {
            transform.localScale = Vector3.one * (m_Hover ? HoverScale : 1f);
            if (Outline) Outline.color = Active ? Ui.Mint : m_Hover ? Color.white : new Color(1, 1, 1, 0.55f);
            if (Label && TintLabel) Label.color = Active ? Ui.Mint : m_Hover ? Color.white : Ui.TextIdle;
        }
    }

    // Texto que se atualiza sozinho a cada frame (valores em tempo real).
    public class LiveText : MonoBehaviour
    {
        public Func<string> Source;
        TextMeshProUGUI m_Text;

        void Awake() => m_Text = GetComponent<TextMeshProUGUI>();

        void Update()
        {
            if (Source == null || !m_Text) return;
            string v;
            try { v = Source(); } catch { v = "-"; }
            if (m_Text.text != v) m_Text.text = v;
        }
    }
}
