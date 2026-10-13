using System;
using System.IO;
using Core;
using UnityEngine;

namespace NineKingsGodMode
{
    // Estado do Modo Deus: qual partida é "Deus" e proteção do progresso do perfil.
    public static class GodMode
    {
        // A partida atual é uma partida Deus.
        public static bool Active { get; private set; }

        // Checkbox "Modo Deus" da tela de escolha do rei.
        public static bool Pending { get; private set; }

        // Enquanto o Modo Deus estiver ligado (na escolha do rei ou na partida), nada do perfil é gravado.
        public static bool ProtectProfile => Active || Pending;

        // Liga/desliga o checkbox. Ligado: todos os reis e perks liberados só na memória.
        public static void SetPending(bool on)
        {
            if (on == Pending) return;
            Pending = on;
            if (on) UnlockEverythingInMemory();
            else if (!Active) ReloadProfile();
        }

        static void UnlockEverythingInMemory()
        {
            foreach (var settings in KingManagement.KingManager.kingSettings.Values)
                if (settings != null) settings.ForceEnable = true;
            if (CheatSystem.CheatsManager.cheats != null) CheatSystem.CheatsManager.cheats.PerksEnabled = true;
        }

        static string StateDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "Library/Application Support/9KingsGodMode");
        static string StateFile => Path.Combine(StateDir, "run.txt");

        // Seed da partida Deus salva (para o Continuar).
        static int? SavedSeed()
        {
            try { return File.Exists(StateFile) && int.TryParse(File.ReadAllText(StateFile).Trim(), out var s) ? s : (int?)null; }
            catch { return null; }
        }

        static void SaveSeed(int? seed)
        {
            try
            {
                Directory.CreateDirectory(StateDir);
                if (seed.HasValue) File.WriteAllText(StateFile, seed.Value.ToString());
                else if (File.Exists(StateFile)) File.Delete(StateFile);
            }
            catch (Exception e) { Debug.LogWarning("[GodMode] estado: " + e.Message); }
        }

        // Chamado antes de Wave.StartNewGame (nova partida ou Continuar).
        public static void OnRunStarting(int? seed, bool multiplayer)
        {
            bool god = !multiplayer && (seed.HasValue ? SavedSeed() == seed.Value : Pending);
            if (Active && !god) DiscardRunProgress();
            Active = god;
            if (god) UnlockEverythingInMemory();
        }

        // Chamado depois de Wave.StartNewGame.
        public static void OnRunStarted(int? seed)
        {
            if (seed.HasValue) return; // Continuar: nada muda
            SaveSeed(Active ? Wave.Data?.Seed : null);
        }

        // Saiu da partida Deus (menu principal): volta ao progresso real gravado no disco.
        public static void OnLeftGameplay()
        {
            if (!Active && !Pending) return;
            DiscardRunProgress();
        }

        // Saiu da escolha do rei sem começar partida.
        public static void OnLeftKingSelection()
        {
            if (Active || !Pending) return;
            Pending = false;
            ReloadProfile();
        }

        static void DiscardRunProgress()
        {
            Active = false;
            Pending = false;
            ReloadProfile();
        }

        // Descarta o que mudou na memória e volta ao progresso real gravado no disco.
        static void ReloadProfile()
        {
            Game.LoadSaveData(applyAudioMixerVolumes: false);
            Debug.Log("[GodMode] progresso do perfil recarregado do disco");
        }

        public static string ProfileSavePath => Application.persistentDataPath + "/9KingsSettings.json";
    }
}
