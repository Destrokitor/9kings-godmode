using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Itens;
using Core.Kingdoms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NineKingsGodMode
{
    // Janela do Modo Deus (aberta pelo botão DEUS durante a partida).
    public class GodMenu : MonoBehaviour
    {
        public static GodMenu Instance { get; private set; }
        public static bool IsOpen => Instance != null;

        enum Tab { Resources, Cards, Kingdom, Decrees, Combat }

        static readonly (Tab tab, string label)[] Tabs =
        {
            (Tab.Resources, "Recursos"), (Tab.Cards, "Cartas"), (Tab.Kingdom, "Reino"),
            (Tab.Decrees, "Decretos"), (Tab.Combat, "Combate"),
        };

        // estado da janela que sobrevive a fechar/abrir
        static Tab s_Tab = Tab.Resources;
        static int s_CardPage, s_PolicyPage;
        static int s_CardDeck = -1; // -1 = todas
        static CardMode s_CardMode = CardMode.Hand;
        static Vector2Int? s_Area;

        enum CardMode { Hand, Loot, Ban }

        RectTransform m_Content;
        readonly List<Button> m_TabButtons = new List<Button>();
        float m_PrevTimeScale = 1f;

        public static void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public static void Open()
        {
            if (IsOpen || !GodMode.Active || !Actions.InRun) return;
            new GameObject("GodModeMenu").AddComponent<GodMenu>();
        }

        public static void Close()
        {
            if (Instance) Destroy(Instance.gameObject);
        }

        void Awake()
        {
            Instance = this;
            m_PrevTimeScale = Time.timeScale;
            Time.timeScale = 0f; // pausa a partida enquanto o menu está aberto
            Build();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = m_PrevTimeScale <= 0f ? 1f : m_PrevTimeScale;
        }

        void Update()
        {
            if (!GodMode.Active || !Actions.InRun) Close();
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        // Estrutura -------------------------------------------------------------------

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2292.74f, 1440f);
            scaler.matchWidthOrHeight = 1f;
            gameObject.AddComponent<GraphicRaycaster>();

            // fundo escurecido esverdeado, como o menu de pausa do jogo
            var dim = Ui.Image(transform, "RoundedRectangle", new Color(0.01f, 0.05f, 0.04f, 0.86f), "Backdrop");
            dim.type = UnityEngine.UI.Image.Type.Simple;
            Ui.Stretch(dim.rectTransform, -50);
            var glow = Ui.Image(transform, "CenterHorizontalGradient", new Color(0.35f, 1f, 0.76f, 0.06f), "Glow");
            glow.raycastTarget = false;
            Ui.Stretch(glow.rectTransform, -200);

            var window = Ui.Rect("Window", transform);
            window.sizeDelta = new Vector2(1900, 1300);
            Ui.VLayout(window, 10, TextAnchor.UpperCenter, 10);

            var title = Ui.Text(window, "MODO DEUS", 130, Ui.Mint, bold: true);
            Ui.Size(title, 1800, 140);

            var tabs = Ui.Rect("Tabs", window);
            Ui.HLayout(tabs, 14);
            foreach (var (tab, label) in Tabs)
            {
                var t = tab;
                m_TabButtons.Add(Ui.TextButton(tabs, label, () => ShowTab(t), 320, 80, 48));
            }

            m_Content = Ui.Rect("Content", window);
            Ui.Size(m_Content, 1820, 900);

            var bottom = Ui.Rect("Bottom", window);
            Ui.HLayout(bottom, 20);
            Ui.TextButton(bottom, "CONSOLE DO JOGO", Actions.OpenConsole, 520, 70, 40);
            Ui.TextButton(bottom, "FECHAR", Close, 320, 70, 48);
            var hint = Ui.Text(bottom, "Console: Command direito + Delete", 26, Ui.Dim);
            Ui.Size(hint, 520, 70);

            ShowTab(s_Tab);
        }

        void ShowTab(Tab tab)
        {
            s_Tab = tab;
            for (int i = 0; i < Tabs.Length; i++)
                m_TabButtons[i].GetComponent<HoverFx>().SetActive(Tabs[i].tab == tab);
            Rebuild();
        }

        void Rebuild()
        {
            foreach (Transform c in m_Content) Destroy(c.gameObject);
            var root = Ui.Rect("Tab", m_Content);
            Ui.Stretch(root);
            switch (s_Tab)
            {
                case Tab.Resources: BuildResources(root); break;
                case Tab.Cards: BuildCards(root); break;
                case Tab.Kingdom: BuildKingdom(root); break;
                case Tab.Decrees: BuildDecrees(root); break;
                case Tab.Combat: BuildCombat(root); break;
            }
        }

        // Peças -----------------------------------------------------------------------

        static RectTransform Column(Transform parent, string title, float width)
        {
            var col = Ui.Box(parent, title, 0.55f);
            Ui.VLayout(col, 10, TextAnchor.UpperCenter, 22);
            Ui.Size(col, width);
            var t = Ui.Text(col, title, 44, Ui.Mint, bold: true);
            Ui.Size(t, width - 50, 56);
            return col;
        }

        static RectTransform Row(Transform parent, float spacing = 8)
        {
            var r = Ui.Rect("Row", parent);
            Ui.HLayout(r, spacing);
            return r;
        }

        static TextMeshProUGUI Value(Transform parent, Func<string> source, float w = 200, Color? color = null)
        {
            var box = Ui.Image(parent, "RoundedRectangle", new Color(0, 0, 0, 0.9f), "Value");
            Ui.Size(box, w, 62);
            var t = Ui.Text(box.transform, "", 36, color ?? Color.white, bold: true);
            Ui.Stretch(t.rectTransform, 4);
            t.gameObject.AddComponent<LiveText>().Source = source;
            return t;
        }

        static void Label(Transform parent, string text, float w)
        {
            var t = Ui.Text(parent, text, 34, Color.white, bold: false, TextAlignmentOptions.MidlineRight);
            Ui.Size(t, w, 62);
        }

        // "rótulo [-g][-p] valor [+p][+g]"
        static void Stepper(Transform parent, string label, Func<string> value, Action<int> add, int small, int big, float labelW = 280)
        {
            var r = Row(parent);
            Label(r, label, labelW);
            Ui.Button(r, "-" + big, () => add(-big), 110, 62, 30, Ui.Red);
            Ui.Button(r, "-" + small, () => add(-small), 96, 62, 30, Ui.Red);
            Value(r, value);
            Ui.Button(r, "+" + small, () => add(small), 96, 62, 30, Ui.Mint);
            Ui.Button(r, "+" + big, () => add(big), 110, 62, 30, Ui.Mint);
        }

        static void Switch(Transform parent, string label, Func<bool> get, Action<bool> set, float labelW = 300)
        {
            var r = Row(parent);
            Label(r, label, labelW);
            Button b = null;
            b = Ui.Button(r, "", () => { set(!get()); Paint(); }, 300, 62, 32);
            void Paint()
            {
                var on = get();
                var t = b.GetComponentInChildren<TextMeshProUGUI>();
                t.text = on ? "LIGADO" : "DESLIGADO";
                t.color = on ? Ui.Mint : Ui.Dim;
            }
            Paint();
        }

        static void Hint(Transform parent, string text, float w)
        {
            var t = Ui.Text(parent, text, 26, Ui.Dim);
            t.textWrappingMode = TextWrappingModes.Normal;
            Ui.Size(t, w, 64);
        }

        // Abas --------------------------------------------------------------------------

        void BuildResources(RectTransform root)
        {
            Ui.HLayout(root, 24, TextAnchor.UpperCenter);

            var left = Column(root, "Partida", 1000);
            Stepper(left, "Ouro", () => Actions.Gold.ToString(), n => Actions.AddGold(n), 10, 100);
            var r = Row(left);
            Label(r, "", 280);
            Ui.Button(r, "+1.000", () => Actions.AddGold(1000), 180, 62, 30, Ui.Gold);
            Ui.Button(r, "+100.000", () => Actions.AddGold(100000), 220, 62, 30, Ui.Gold);
            Ui.Button(r, "Zerar", () => Actions.AddGold(-Actions.Gold), 150, 62, 30, Ui.Red);
            Stepper(left, "Vidas", () => Actions.Life + " / " + Actions.MaxLife, n => Actions.AddLife(n), 1, 5);
            Stepper(left, "Limite da mão", () => Actions.MaxHandCards.ToString(), n => Actions.AddMaxHandCards(n), 1, 5);
            Hint(left, "Tudo muda na hora. A partida fica pausada com o menu aberto.", 820);

            var right = Column(root, "Poderes", 780);
            Switch(right, "Invencível", () => Actions.Godlike, v => Actions.Godlike = v);
            Switch(right, "Vidas infinitas", () => Actions.InfiniteLives, v => Actions.InfiniteLives = v);
            Switch(right, "Castelo atira sozinho", () => Actions.CastleAutoshoot, v => Actions.CastleAutoshoot = v);

            var speedTitle = Ui.Text(right, "Velocidade do jogo", 34, Color.white);
            Ui.Size(speedTitle, 760, 56);
            var sr = Row(right, 6);
            Ui.Button(sr, "Normal", () => Actions.SetSpeed(null), 150, 62, 28);
            foreach (var s in Actions.Speeds)
            {
                var speed = s;
                Ui.Button(sr, speed + "x", () => Actions.SetSpeed(speed), 110, 62, 28);
            }
            var cur = Row(right);
            Label(cur, "Atual", 330);
            Value(cur, () => Actions.SpeedLabel, 300);
        }

        void BuildCards(RectTransform root)
        {
            Ui.VLayout(root, 10, TextAnchor.UpperCenter);

            // filtro por rei
            var decks = Actions.Decks;
            var fr = Row(root, 6);
            Mark(Ui.Button(fr, "Todas", () => { s_CardDeck = -1; s_CardPage = 0; Rebuild(); }, 130, 58, 24), s_CardDeck == -1);
            for (int i = 0; i < decks.Count; i++)
            {
                int idx = i;
                Mark(Ui.Button(fr, decks[i].Key, () => { s_CardDeck = idx; s_CardPage = 0; Rebuild(); }, 130, 58, 24), s_CardDeck == idx);
            }

            // o que o clique faz
            var mr = Row(root, 10);
            Label(mr, "Clique na carta para:", 360);
            ModeButton(mr, "Pôr na mão", CardMode.Hand);
            ModeButton(mr, "Forçar no saque", CardMode.Loot);
            ModeButton(mr, "Banir / liberar", CardMode.Ban);

            var cards = s_CardDeck >= 0 && s_CardDeck < decks.Count ? decks[s_CardDeck].Value : Actions.AllCards;
            const int perPage = 14;
            int pages = Math.Max(1, (cards.Count + perPage - 1) / perPage);
            s_CardPage = Mathf.Clamp(s_CardPage, 0, pages - 1);

            var grid = Ui.Rect("Grid", root);
            var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(240, 270);
            gl.spacing = new Vector2(12, 12);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 7;
            gl.childAlignment = TextAnchor.UpperCenter;
            Ui.Size(grid, 1760, 560);
            foreach (var card in cards.Skip(s_CardPage * perPage).Take(perPage)) CardTile(grid, card);

            Pager(root, () => s_CardPage, v => s_CardPage = v, pages);
        }

        void ModeButton(Transform parent, string label, CardMode mode)
        {
            Mark(Ui.Button(parent, label, () => { s_CardMode = mode; Rebuild(); }, 320, 58, 28), s_CardMode == mode);
        }

        static Button Mark(Button b, bool active)
        {
            b.GetComponent<HoverFx>().SetActive(active);
            return b;
        }

        void CardTile(Transform grid, CardSo card)
        {
            var btn = Ui.Button(grid, "", () =>
            {
                switch (s_CardMode)
                {
                    case CardMode.Hand: Actions.AddCardToHand(card); break;
                    case CardMode.Loot: Actions.ToggleForcedLoot(card); break;
                    case CardMode.Ban: Actions.ToggleBan(card); break;
                }
            }, -1, -1, 22, name: "Card");
            btn.GetComponentInChildren<TextMeshProUGUI>().gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Ui.VLayout(btn, 4, TextAnchor.MiddleCenter, 12);

            var art = Ui.Rect("Art", btn.transform).gameObject.AddComponent<Image>();
            art.sprite = card.Miniature ? card.Miniature : card.ItemSprite;
            art.preserveAspect = true;
            art.raycastTarget = false;
            Ui.Size(art, 200, 170);

            var name = Ui.Text(btn.transform, Actions.CardName(card), 24, Color.white, bold: true);
            name.textWrappingMode = TextWrappingModes.Normal;
            Ui.Size(name, 216, 56);

            var state = Ui.Text(btn.transform, "", 22, Ui.Gold, bold: true);
            Ui.Size(state, 216, 24);
            state.gameObject.AddComponent<LiveText>().Source = () =>
                Actions.IsBanned(card) ? "BANIDA" : Actions.ForcedLoot == Actions.EntityName(card) ? "NO SAQUE" : "";
        }

        void Pager(Transform parent, Func<int> get, Action<int> set, int pages)
        {
            var pr = Row(parent, 16);
            Ui.Button(pr, "<", () => { set((get() - 1 + pages) % pages); Rebuild(); }, 110, 62, 40);
            Value(pr, () => "Página " + (get() + 1) + " / " + pages, 360);
            Ui.Button(pr, ">", () => { set((get() + 1) % pages); Rebuild(); }, 110, 62, 40);
        }

        void BuildKingdom(RectTransform root)
        {
            Ui.HLayout(root, 24, TextAnchor.UpperCenter);

            var areas = Actions.Areas.Where(a => a != null).ToList();
            var left = Column(root, "Terrenos", 1000);
            if (areas.Count > 0)
            {
                var pos = areas.Select(a => a.GetGridPosition()).ToList();
                int minX = pos.Min(p => p.x), maxX = pos.Max(p => p.x), minY = pos.Min(p => p.y), maxY = pos.Max(p => p.y);
                var grid = Ui.Rect("Grid", left);
                var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
                int cols = maxX - minX + 1;
                float cell = Mathf.Min(180, 900f / cols - 8);
                gl.cellSize = new Vector2(cell, cell * 0.72f);
                gl.spacing = new Vector2(8, 8);
                gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gl.constraintCount = cols;
                gl.childAlignment = TextAnchor.UpperCenter;
                Ui.Size(grid, 940, (cell * 0.72f + 8) * (maxY - minY + 1));
                for (int y = maxY; y >= minY; y--)
                    for (int x = minX; x <= maxX; x++)
                        AreaTile(grid, KingdomAreaManager.GetByGridPosition(x, y), new Vector2Int(x, y));
            }
            Hint(left, "Clique num terreno para escolher. O castelo não pode ser limpo.", 940);

            var right = Column(root, "Ações", 720);
            var sel = Ui.Text(right, "", 32, Ui.Gold, bold: true);
            sel.textWrappingMode = TextWrappingModes.Normal;
            Ui.Size(sel, 660, 90);
            sel.gameObject.AddComponent<LiveText>().Source = () =>
            {
                var a = Selected();
                return a == null ? "Nenhum terreno escolhido" : Actions.AreaLabel(a).Replace("\n", " · ");
            };
            ActionPair(right, "Subir nível", () => Actions.LevelUp(Selected()), "+5 níveis", () => Actions.LevelUp(Selected(), 5));
            ActionPair(right, "Descer nível", () => Actions.LevelDown(Selected()), "Limpar", () => Actions.Clear(Selected()));
            ActionPair(right, "+10 tropas", () => Actions.AddTroops(Selected(), 10), "-10 tropas", () => Actions.AddTroops(Selected(), -10));
            ActionPair(right, "Buff x2", () => Actions.Buff(Selected(), 2), "Buff x10", () => Actions.Buff(Selected(), 10));
            ActionPair(right, "Desbloquear", () => Actions.Unlock(Selected()), "Liberar todos", Actions.UnlockAll);
            var r = Row(right);
            Ui.Button(r, "Subir nível de todos", Actions.LevelUpAll, 650, 62, 30, Ui.Mint);
        }

        static KingdomArea Selected() => s_Area.HasValue ? KingdomAreaManager.GetByGridPosition(s_Area.Value.x, s_Area.Value.y) : null;

        void ActionPair(Transform parent, string a, Action actA, string b, Action actB)
        {
            var r = Row(parent, 12);
            Ui.Button(r, a, () => { actA(); Rebuild(); }, 320, 62, 30);
            Ui.Button(r, b, () => { actB(); Rebuild(); }, 320, 62, 30);
        }

        void AreaTile(Transform grid, KingdomArea area, Vector2Int pos)
        {
            if (area == null)
            {
                Ui.Rect("Empty", grid);
                return;
            }
            bool selected = s_Area == pos;
            var b = Ui.Button(grid, "", () => { s_Area = pos; Rebuild(); }, -1, -1, 28,
                area.Unlocked ? Color.white : Ui.Dim);
            Mark(b, selected);
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            t.textWrappingMode = TextWrappingModes.Normal;
            t.gameObject.AddComponent<LiveText>().Source = () => Actions.AreaLabel(area);
        }

        void BuildDecrees(RectTransform root)
        {
            Ui.VLayout(root, 12, TextAnchor.UpperCenter);
            var top = Row(root, 16);
            Label(top, "Clique num decreto para ativar.", 700);
            Ui.Button(top, "Limpar decretos", () => Actions.ClearPolicies(), 380, 62, 30, Ui.Red);

            var all = Actions.AllPolicies;
            const int perPage = 24;
            int pages = Math.Max(1, (all.Count + perPage - 1) / perPage);
            s_PolicyPage = Mathf.Clamp(s_PolicyPage, 0, pages - 1);

            var grid = Ui.Rect("Grid", root);
            var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(420, 84);
            gl.spacing = new Vector2(12, 12);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 4;
            gl.childAlignment = TextAnchor.UpperCenter;
            Ui.Size(grid, 1760, 640);
            foreach (var p in all.Skip(s_PolicyPage * perPage).Take(perPage))
            {
                var policy = p;
                var b = Ui.Button(grid, Actions.PolicyName(policy), () => Actions.AddPolicy(policy), -1, -1, 26);
                b.GetComponentInChildren<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            }
            Pager(root, () => s_PolicyPage, v => s_PolicyPage = v, pages);
        }

        void BuildCombat(RectTransform root)
        {
            Ui.HLayout(root, 24, TextAnchor.UpperCenter);

            var left = Column(root, "Reis", 980);
            foreach (var k in Actions.OtherKings.ToList())
            {
                var king = k;
                var r = Row(left, 12);
                var n = Ui.Text(r, Actions.KingName(king), 34, Color.white, false, TextAlignmentOptions.MidlineRight);
                Ui.Size(n, 420, 62);
                Value(r, () => king.IsEnemy ? "INIMIGO" : "AMIGO", 220);
                Ui.Button(r, "", () => Actions.ToggleWar(king), 240, 62, 30)
                    .GetComponentInChildren<TextMeshProUGUI>().gameObject.AddComponent<LiveText>().Source =
                    () => king.IsEnemy ? "Fazer paz" : "Declarar guerra";
            }
            Hint(left, "Guerra e paz mudam quem ataca você nos próximos anos.", 920);

            var right = Column(root, "Batalha", 700);
            var rr = Row(right);
            Ui.Button(rr, "Remover inimigos da batalha", Actions.RemoveEnemies, 640, 70, 30, Ui.Red);
            Switch(right, "Invencível", () => Actions.Godlike, v => Actions.Godlike = v, 300);
            Switch(right, "Castelo atira", () => Actions.CastleAutoshoot, v => Actions.CastleAutoshoot = v, 300);
            Hint(right, "Para turbinar tropas, use a aba Reino (Buff, níveis e tropas).", 640);
        }
    }
}
