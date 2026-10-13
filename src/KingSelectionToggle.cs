using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NineKingsGodMode
{
    // Botão liga/desliga "Modo Deus" no canto superior direito da tela de escolha do rei.
    // É um clone do botão "Back" da própria tela, com o ícone on/off do jogo.
    public static class KingSelectionToggle
    {
        public static bool Reloading { get; private set; }

        public static void Create(Component screen)
        {
            var back = screen.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "BackToMenuBtn");
            if (back == null) return;

            var go = Object.Instantiate(back.gameObject, back.transform.parent);
            go.name = "GodModeToggle";
            // a animação do "Back" mexe na posição/cor dos filhos; o hover fica por nossa conta
            var trigger = go.GetComponent<EventTrigger>();
            if (trigger) Object.DestroyImmediate(trigger);
            var animator = go.GetComponent<Animator>();
            if (animator) Object.DestroyImmediate(animator);
            var btn = go.GetComponent<Button>();
            btn.onClick = new Button.ButtonClickedEvent();
            btn.transition = Selectable.Transition.None;
            var bg = go.GetComponent<Image>();
            bg.color = new Color(1, 1, 1, 0f); // só área de clique, como o "Back"
            foreach (var g in go.GetComponentsInChildren<Graphic>(true)) g.canvasRenderer.SetAlpha(1f);

            // canto superior direito, na mesma altura do "Back" (que é girado 180°; o clone não)
            var rt = (RectTransform)go.transform;
            var brt = (RectTransform)back.transform;
            var corners = new Vector3[4];
            brt.GetWorldCorners(corners);
            var parentRt = (RectTransform)brt.parent;
            float backCenterY = parentRt.InverseTransformPoint((corners[0] + corners[2]) / 2f).y;
            rt.localEulerAngles = Vector3.zero;
            rt.localScale = Vector3.one;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(brt.rect.width * 2.2f, brt.rect.height);
            rt.anchoredPosition = new Vector2(-110f, backCenterY);

            // texto novo (o clonado não redesenha), mesma fonte/tamanho do "Back"
            var old = go.GetComponentInChildren<TMP_Text>();
            float size = old.fontSize;
            Object.DestroyImmediate(old.gameObject);
            var txt = Ui.Text(go.transform, "Modo Deus", size, Color.white, bold: true, TextAlignmentOptions.MidlineRight);
            var trt = txt.rectTransform;
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(0.74f, 1);
            trt.offsetMin = trt.offsetMax = Vector2.zero;

            var icon = go.transform.Find("Image")?.GetComponent<Image>();
            if (icon)
            {
                var irt = icon.rectTransform;
                irt.anchorMin = irt.anchorMax = new Vector2(0.86f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = Vector2.zero;
                irt.sizeDelta = new Vector2(17 * 7, 9 * 7);
                irt.localEulerAngles = Vector3.zero;
                irt.localScale = Vector3.one;
                icon.preserveAspect = true;
            }

            var hover = go.AddComponent<ToggleHover>();
            void Paint()
            {
                bool on = GodMode.Pending;
                float a = hover.Hovered ? 1f : 0.6f;
                txt.color = on ? Ui.Mint : new Color(1, 1, 1, a);
                if (icon)
                {
                    icon.sprite = Ui.Sprite(on ? "on_off_0" : "on_off_1"); // 0 = "ON", 1 = "OFF"
                    icon.color = on ? Color.white : new Color(1, 1, 1, 0.5f);
                }
            }

            hover.Changed = Paint;
            btn.onClick.AddListener(() =>
            {
                GodMode.SetPending(!GodMode.Pending);
                Paint();
                ReloadScreen();
            });
            Paint();
        }

        // Recarrega a tela para mostrar os reis liberados/bloqueados.
        static void ReloadScreen()
        {
            Reloading = true;
            var op = SceneManager.UnloadSceneAsync("KingSelection");
            if (op == null) { Reloading = false; return; }
            op.completed += _ =>
            {
                SceneManager.LoadScene("KingSelection", LoadSceneMode.Additive);
                Reloading = false;
            };
        }
    }
}

namespace NineKingsGodMode
{
    public class ToggleHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool Hovered;
        public System.Action Changed;
        public void OnPointerEnter(PointerEventData e) { Hovered = true; Changed?.Invoke(); }
        public void OnPointerExit(PointerEventData e) { Hovered = false; Changed?.Invoke(); }
    }
}
