# 9 Kings — Modo Deus

Mod para o 9 Kings (Mac/Steam): janela "Modo Deus" no visual do jogo para controlar a partida
em tempo real, mais o console de desenvolvedor do próprio jogo liberado.

## Como jogar
1. Abra pelo atalho **"9 Kings Modo Deus"** na Mesa. Pela Steam o jogo abre normal, sem o mod.
2. **Start → Single Player → New Run**. Na tela "Choose a King", ligue **Modo Deus** (canto superior
   direito). Todos os reis e perks ficam liberados nessa partida.
3. Escolha o rei e jogue. Na partida, clique em **DEUS** (abaixo do ouro).
4. Console do jogo: botão "Console do jogo" no menu, ou **Command direito + Delete**.

O Modo Deus continua ao usar **Continue**. Enquanto ele está ligado nada do seu perfil é gravado
(XP, níveis, desbloqueios, estatísticas, conquistas Steam); ao voltar ao menu o perfil real é recarregado.
Nunca liga no multiplayer.

## Abas
| Aba | O que faz |
|---|---|
| Recursos | ouro (+/−, +1.000, +100.000, zerar), vidas, limite de cartas na mão, invencível, vidas infinitas, castelo atira sozinho, velocidade 0,5x–8x |
| Cartas | todas as cartas por rei; clique para pôr na mão, forçar no saque ou banir/liberar |
| Reino | tabuleiro 5x5: subir/descer nível, +/−10 tropas, buff x2/x10, limpar, desbloquear terreno; liberar todos; subir nível de todos |
| Decretos | ativar qualquer decreto; limpar decretos |
| Combate | guerra/paz com cada rei; remover inimigos da batalha; invencível; castelo atira |

## Como funciona
O 9 Kings usa Unity 6 (Mono). O BepInEx/Doorstop não injeta nessa versão no Mac, então o mod é
carregado pelo próprio mecanismo da Unity: `install.py` copia `NineKingsGodMode.dll` + `0Harmony.dll`
(Harmony 2.4.3, com suporte a ARM64) para `Data/Managed` e registra `Loader.Init` em
`RuntimeInitializeOnLoads.json`. O mod só liga quando a variável `GODMODE_9K=1` existe (o atalho
define). O atalho roda `install.py` antes de abrir — se uma atualização da Steam apagar o registro,
ele é refeito sozinho.

- `python3 install.py status | install | uninstall` (uninstall restaura os JSONs originais).
- Código: `src/` — `GodMode.cs` (estado/proteção), `Patches.cs` (Harmony), `Actions.cs` (ações),
  `GodMenu.cs` (janela), `Ui.cs` (componentes no estilo do jogo), `KingSelectionToggle.cs`, `GodModeHost.cs`.
- Compilar: `DOTNET_ROOT=/opt/homebrew/opt/dotnet/libexec dotnet build -c Release`.
