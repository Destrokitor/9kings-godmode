using System;
using System.Collections.Generic;
using System.Linq;
using CheatSystem;
using Core;
using Core.Itens;
using Core.Kingdoms;
using GameplayInterface;
using I2.Loc;
using Policies;
using UnityEngine;

namespace NineKingsGodMode
{
    // Ações do Modo Deus. Sem UI: cada função muda a partida na hora.
    public static class Actions
    {
        public static bool InRun => Wave.OnGameplay && Wave.Data != null;

        static void Sfx(string name)
        {
            try { Audio.Sfx(name).Play(); } catch { }
        }

        // Recursos -----------------------------------------------------------------

        public static int Gold => InRun ? Wave.Data.Currency : 0;

        public static void AddGold(int amount)
        {
            if (!InRun) return;
            // nunca deixa negativo
            amount = Math.Max(amount, -Wave.Data.Currency);
            Wave.Data.ModifyCurrencyBy(amount);
        }

        public static int Life => PlayerLife.Instance ? PlayerLife.Instance.HealthPoints : 0;
        public static int MaxLife => PlayerLife.Instance ? PlayerLife.Instance.MaxHealthPoints : 0;

        public static void AddLife(int amount)
        {
            if (!InRun || !PlayerLife.Instance) return;
            PlayerLife.Instance.SetLife(Math.Max(1, Life + amount));
        }

        public static bool Godlike
        {
            get => CheatsManager.cheats != null && CheatsManager.cheats.GodLike;
            set { if (CheatsManager.cheats != null) CheatsManager.cheats.GodLike = value; }
        }

        public static bool InfiniteLives
        {
            get => CheatsManager.cheats != null && CheatsManager.cheats.InfinityLifes;
            set
            {
                if (CheatsManager.cheats == null) return;
                CheatsManager.cheats.InfinityLifes = value;
                if (value && PlayerLife.Instance) PlayerLife.Instance.SetLife(PlayerLife.Instance.MaxHealthPoints);
            }
        }

        public static bool CastleAutoshoot
        {
            get => CheatsManager.cheats != null && CheatsManager.cheats.CastleAutoshoot;
            set { if (CheatsManager.cheats != null) CheatsManager.cheats.CastleAutoshoot = value; }
        }

        public static readonly float[] Speeds = { 0.5f, 1f, 2f, 4f, 8f };

        public static void SetSpeed(float? speed) => Game.OverrideSpeed = speed;

        public static string SpeedLabel => Game.OverrideSpeed.HasValue ? Game.OverrideSpeed.Value + "x" : "normal";

        // Mão de cartas: limite por partida solo.
        public static int MaxHandCards => Wave.Options.MaxCardsSingleplayer;

        public static void AddMaxHandCards(int n)
        {
            Wave.Options.EnableMaxCardsSingleplayer = true;
            Wave.Options.MaxCardsSingleplayer = Math.Max(1, Wave.Options.MaxCardsSingleplayer + n);
        }

        // Cartas --------------------------------------------------------------------

        // Baralhos dos reis: (nome do rei, cartas). Só cartas jogáveis, sem as internas/de chefes.
        static List<KeyValuePair<string, List<CardSo>>> s_Decks;

        public static List<KeyValuePair<string, List<CardSo>>> Decks
        {
            get
            {
                if (s_Decks != null) return s_Decks;
                s_Decks = new List<KeyValuePair<string, List<CardSo>>>();
                foreach (var entry in Wave.Options.Kings.GroupArray)
                {
                    var king = entry?.King;
                    if (king == null || king.KingDeck == null) continue;
                    var cards = king.KingDeck.GroupArray
                        .Where(d => d != null && d.Card != null && IsPlayerCard(d.Card))
                        .Select(d => d.Card).Distinct()
                        .OrderBy(CardName).ToList();
                    if (cards.Count == 0) continue;
                    string name;
                    try { name = LocalizationManager.GetTermTranslation(king.KingShortNameKey) ?? ((UnityEngine.Object)king).name; }
                    catch { name = ((UnityEngine.Object)king).name; }
                    s_Decks.Add(new KeyValuePair<string, List<CardSo>>(name, cards));
                }
                return s_Decks;
            }
        }

        // Esconde cartas internas (ex.: "ZMerchant", "zOLD...") e sem arte.
        static bool IsPlayerCard(CardSo c)
        {
            string n = c.name ?? "";
            if (n.StartsWith("zOLD") || (n.Length > 1 && n[0] == 'Z' && char.IsUpper(n[1]))) return false;
            return c.Miniature != null || c.ItemSprite != null;
        }

        public static List<CardSo> AllCards => Decks.SelectMany(d => d.Value).Distinct().ToList();

        static Dictionary<CardSo, string> s_EntityNames;

        // Nome interno usado pelo jogo para achar a carta (Game.Entities[nome]).
        public static string EntityName(CardSo c)
        {
            if (s_EntityNames == null)
            {
                s_EntityNames = new Dictionary<CardSo, string>();
                foreach (var e in Game.Entities.entities)
                    if (e?.Entity != null && !s_EntityNames.ContainsKey(e.Entity)) s_EntityNames[e.Entity] = e.Name;
            }
            return s_EntityNames.TryGetValue(c, out var n) ? n : c.name;
        }

        public static string CardName(CardSo c)
        {
            try { return LocalizationManager.GetTermTranslation(c.ItemNameString) ?? c.name; }
            catch { return c.name; }
        }

        // Texto oficial da carta (com as cores do jogo).
        public static string CardDescription(CardSo c)
        {
            try { return c.GetDescription(); } catch { return ""; }
        }

        public static string CardType(CardSo c)
        {
            string type = "";
            try { type = LocalizationManager.GetTermTranslation(c.GetTypeLocalizationKey()) ?? ""; } catch { }
            var deck = Decks.FirstOrDefault(d => d.Value.Contains(c)).Key;
            return string.Join(" · ", new[] { type, deck }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static bool AddCardToHand(CardSo card)
        {
            if (!InRun || card == null) return false;
            return GameplayUI.HandCards.Create(card, failSfx: true, null, bypassSeasonCardRestriction: true);
        }

        public static string ForcedLoot => InRun ? Wave.Data.ForceLootCard : "";

        public static void ToggleForcedLoot(CardSo card)
        {
            if (!InRun) return;
            string name = EntityName(card);
            Wave.Data.ForceLootCard = Wave.Data.ForceLootCard == name ? "" : name;
        }

        public static bool IsBanned(CardSo card) => InRun && Wave.Data.BannedCards.Contains(card.ItemNameString);

        public static void ToggleBan(CardSo card)
        {
            if (!InRun) return;
            if (!Wave.Data.BannedCards.Remove(card.ItemNameString)) Wave.Data.BannedCards.Add(card.ItemNameString);
        }

        // Reino (terrenos) -------------------------------------------------------------

        public static KingdomArea[] Areas => KingdomAreaManager.areas ?? new KingdomArea[0];

        public static string AreaLabel(KingdomArea a)
        {
            if (a == null) return "";
            if (!a.Unlocked) return "bloqueado";
            if (a.Placed == null) return "vazio";
            return CardName(a.Placed) + "\nnv " + a.Placed.TooltipLevel;
        }

        public static void LevelUp(KingdomArea a, int times = 1)
        {
            if (a == null || a.Placed == null) return;
            for (int i = 0; i < times; i++) a.ForceLevelUp();
            a.PostLevelUpRoutine();
        }

        public static void LevelDown(KingdomArea a)
        {
            if (a == null || a.Placed == null || a.Placed.TooltipLevel <= 1) return;
            a.LevelDown();
            a.UpdateSprites();
        }

        public static void AddTroops(KingdomArea a, int n)
        {
            if (a == null || a.Placed == null) return;
            if (n > 0) a.AddTroops(n);
            else a.RemoveTroops(-n);
        }

        public static void Buff(KingdomArea a, double factor)
        {
            if (a == null || a.Placed == null) return;
            a.AddBoost(new Boost(hp: factor, damage: factor, atkSpeed: factor), showPopup: true);
        }

        public static void Clear(KingdomArea a)
        {
            if (a == null || a.Placed == null || a.IsCastle) return;
            a.Clear();
        }

        public static void Unlock(KingdomArea a)
        {
            if (a != null && !a.Unlocked) a.Unlock();
        }

        public static void UnlockAll()
        {
            foreach (var a in Areas) Unlock(a);
        }

        public static void LevelUpAll()
        {
            foreach (var a in Areas) LevelUp(a);
        }

        // Decretos --------------------------------------------------------------------

        static List<Policy> s_Policies;

        public static List<Policy> AllPolicies => s_Policies ?? (s_Policies = Policy.GetAll()
            .Where(p => p != null && !string.IsNullOrWhiteSpace(PolicyName(p)))
            .OrderBy(PolicyName).ToList());

        public static string PolicyName(Policy p)
        {
            try { return p.GetTranslatedName(); } catch { return ""; }
        }

        public static string PolicyDescription(Policy p)
        {
            try { return p.GetDescription(); } catch { return ""; }
        }

        public static void AddPolicy(Policy p)
        {
            if (!InRun || p == null) return;
            Wave.Data.AddPolicy(p, 1);
        }

        public static void ClearPolicies()
        {
            if (!InRun) return;
            GameplayUI.PolicyShowcase.Clear();
            PolicyVariables.Instance.Reset();
        }

        // Reis e combate ----------------------------------------------------------------

        public static IEnumerable<KingAffection> OtherKings => InRun
            ? Wave.Data.Kings.Where(k => k != null && k.IsEnabled && k != Wave.PlayerKing)
            : Enumerable.Empty<KingAffection>();

        public static string KingName(KingAffection k)
        {
            try
            {
                return LocalizationManager.GetTermTranslation(k.Scriptable.KingNameKey) ?? ((UnityEngine.Object)k.Scriptable).name;
            }
            catch { return ((UnityEngine.Object)k.Scriptable).name; }
        }

        public static void ToggleWar(KingAffection k)
        {
            if (k.IsEnemy) k.BecomeFriends();
            else k.BecomeEnemy();
        }

        public static void RemoveEnemies()
        {
            if (!InRun || Wave.Instance == null) return;
            Wave.Instance.DespawnWaveEnemies();
        }

        // Console de desenvolvedor do próprio jogo.
        public static void OpenConsole()
        {
            Wave.Options.EnableGameConsole = true;
            if (UnityEngine.Object.FindFirstObjectByType<global::Console.GameConsole>() != null) return;
            UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Console"));
        }
    }
}
