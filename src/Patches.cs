using System.Collections.Generic;
using Achievements;
using Core;
using HarmonyLib;
using SaveSystemV2;
using Steamworks;

namespace NineKingsGodMode
{
    [HarmonyPatch(typeof(Wave), nameof(Wave.StartNewGame))]
    static class Patch_StartNewGame
    {
        static void Prefix(int? seed, bool isMultiplayer) => GodMode.OnRunStarting(seed, isMultiplayer);
        static void Postfix(int? seed) => GodMode.OnRunStarted(seed);
    }

    // Perfil: nada é gravado enquanto a partida Deus estiver ativa.
    [HarmonyPatch(typeof(SaveSystem), nameof(SaveSystem.Save), typeof(string))]
    static class Patch_SaveSystemSave
    {
        static bool Prefix(string path) => !(GodMode.ProtectProfile && path == GodMode.ProfileSavePath);
    }

    // Conquistas Steam bloqueadas na partida Deus.
    [HarmonyPatch(typeof(Steam), nameof(Steam.UnlockAchievement))]
    static class Patch_UnlockAchievement
    {
        static bool Prefix() => !GodMode.ProtectProfile;
    }
}

namespace NineKingsGodMode
{
    // O console do jogo varre todos os assemblies carregados; os do Harmony/mod quebram essa varredura.
    [HarmonyLib.HarmonyPatch]
    static class Patch_ConsoleLoadableTypes
    {
        static System.Reflection.MethodBase TargetMethod() =>
            HarmonyLib.AccessTools.Method(HarmonyLib.AccessTools.TypeByName("Console.GameConsoleCommandRegistry"), "GetLoadableTypes");

        static bool Prefix(System.Reflection.Assembly assembly, ref System.Collections.Generic.IEnumerable<System.Type> __result)
        {
            string name = assembly.GetName().Name ?? "";
            if (assembly.IsDynamic || name.StartsWith("0Harmony") || name.StartsWith("Harmony") || name.StartsWith("MonoMod")
                || name == "NineKingsGodMode")
            {
                __result = System.Type.EmptyTypes;
                return false;
            }
            return true;
        }
    }
}

namespace NineKingsGodMode
{
    // Checkbox "Modo Deus" na tela "Escolha um rei".
    [HarmonyLib.HarmonyPatch(typeof(KingSelectionUI.KingSelection_Main), nameof(KingSelectionUI.KingSelection_Main.Initialize))]
    static class Patch_KingSelectionToggle
    {
        static void Postfix(KingSelectionUI.KingSelection_Main __instance) => KingSelectionToggle.Create(__instance);
    }
}
