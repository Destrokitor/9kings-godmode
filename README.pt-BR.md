# 9 Kings — Modo Deus

*Read in English: [README.md](README.md)*

Mod para o 9 Kings (Mac/Steam): janela "Modo Deus" no visual do jogo para controlar a partida
em tempo real, mais o console de desenvolvedor do próprio jogo liberado.

## Como jogar
1. Dê duplo clique em **`Play 9 Kings God Mode.command`**. Pela Steam o jogo abre normal, sem o mod.
2. **Start → Single Player → New Run**. Na tela "Choose a King", ligue **Modo Deus** (canto superior
   direito). Todos os reis e perks ficam liberados nessa partida.
3. Escolha o rei e jogue. Na partida, clique em **DEUS** (abaixo do ouro).
4. Console do jogo: botão "Console do jogo" no menu, ou **Command direito + Delete**.

O Modo Deus continua ao usar **Continue**. Enquanto ele está ligado nada do seu perfil é gravado
(XP, níveis, desbloqueios, estatísticas, conquistas Steam); ao voltar ao menu o perfil real é recarregado.
Nunca liga no multiplayer.

Passe o mouse e deixe parado sobre uma carta, decreto ou terreno para ver o efeito (texto oficial do jogo).

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
`RuntimeInitializeOnLoads.json`. O mod só liga quando a variável `GODMODE_9K=1` existe (o `.command`
define). O `.command` roda `install.py` antes de abrir — se uma atualização da Steam apagar o registro,
ele é refeito sozinho.

- `python3 install.py status | install | uninstall` (uninstall restaura os JSONs originais).
- Código: `src/` — `GodMode.cs` (estado/proteção), `Patches.cs` (Harmony), `Actions.cs` (ações),
  `GodMenu.cs` (janela), `Ui.cs` (componentes no estilo do jogo), `KingSelectionToggle.cs`, `GodModeHost.cs`.
- Compilar: `DOTNET_ROOT=/opt/homebrew/opt/dotnet/libexec dotnet build -c Release`.
- Build de teste: `-p:Dev=1` inclui `src/Dev/` (canal de comandos por arquivo, ativo só com `GODMODE_DEV_DIR`).

## Compatibilidade
Testado no **9 Kings 0.9.8.3** (acesso antecipado) no macOS. O jogo ainda recebe atualizações (a versão 1.0 está anunciada para 12 de novembro de 2026) e uma atualização pode quebrar o mod até ele ser ajustado. Se o jogo não abrir depois de uma atualização, rode `python3 install.py uninstall` para restaurar os arquivos originais.

## Requisitos e instalação
- **9 Kings** (Steam) no **macOS** (o instalador altera a pasta `Data` do app do Mac; Windows e Linux não são suportados) e `python3`.
1. Baixe o zip na página **Releases** e descompacte.
2. Dê duplo clique em **`Play 9 Kings God Mode.command`**: ele instala o mod na primeira vez e abre o jogo com ele ligado.

## Aviso
Mod não oficial, feito por fã. **Não tem ligação nem aprovação da Sad Socket.** 9 Kings e seus nomes e artes pertencem aos donos; este repositório não contém nenhum arquivo do jogo. Use por sua conta e risco e guarde uma cópia do seu save (`~/Library/Application Support/SadSocket/9Kings`).

## Apoie

Este mod é e sempre será **gratuito e completo**. Se você gostou e quiser agradecer, doações voluntárias são bem-vindas (nada fica bloqueado por doação e não há vantagens):

- **PIX:** `pedro.comitre@gmail.com`

## Licença
[MIT](LICENSE).

O Harmony (`lib/0Harmony.dll`) é incluído sem alterações, sob a licença MIT dele; veja [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
