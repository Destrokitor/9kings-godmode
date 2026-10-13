using System;
using HarmonyLib;
using UnityEngine;

namespace NineKingsGodMode
{
    // Chamado pela Unity na inicialização (registrado em RuntimeInitializeOnLoads.json pelo install.py).
    public static class Loader
    {
        public const string EnvVar = "GODMODE_9K";

        public static void Init()
        {
            // Só liga quando o jogo é aberto pelo atalho do Modo Deus.
            if (Environment.GetEnvironmentVariable(EnvVar) != "1") return;
            try
            {
                new Harmony("pedro.ninekings.godmode").PatchAll(typeof(Loader).Assembly);
                var host = new GameObject("NineKingsGodMode");
                UnityEngine.Object.DontDestroyOnLoad(host);
                host.AddComponent<GodModeHost>();
                // canal de teste (só existe no build de desenvolvimento)
                Type.GetType("NineKingsGodMode.Dev.DevChannel")?.GetMethod("Attach")?.Invoke(null, new object[] { host });
                Debug.Log("[GodMode] carregado");
            }
            catch (Exception e)
            {
                Debug.LogError("[GodMode] falhou ao carregar: " + e);
            }
        }
    }
}
