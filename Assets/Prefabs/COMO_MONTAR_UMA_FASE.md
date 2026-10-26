# Como montar uma fase nova

Eixos das fases: **Z = esquerda/direita da tela**, **X = profundidade** (a câmera olha para -X).
O player anda na linha **x = 3,1**.

## 1. Arraste para a cena

| Prefab | Pasta | O que faz |
|---|---|---|
| `KitDaFase` | `Prefabs/Fase` | Câmera que segue o player, HUD de stamina, GerenciadorEstadoJogador e os limites. Deixe na posição **0,0,0**. |
| `Player` | `Prefabs/Personagens` | Personagem completo (animações, stamina, pulo, agachar, esconder). Acha a câmera e a barra de stamina sozinho. |
| `Sniper` | `Prefabs/Inimigos` | Patrulha entre os filhos `LimiteFrente`/`LimiteTras`, olha pra baixo e atira. |
| `Porradeiro` | `Prefabs/Inimigos` | Patrulha e persegue o player. Acha o player sozinho. |
| `PortaEsconderijo` | `Prefabs/Fase` | Coloque no vão de uma porta: E esconde / E sai. |
| `FimDaFase` | `Prefabs/Fase` | Gatilho que carrega a cena em `Proxima Cena`. |
| `ParedeInvisivel` | `Prefabs/Fase` | Bloqueia a passagem (ex.: começo da fase). |
| `LimitePatrulhaSniper` | `Prefabs/Fase` | Limite extra de patrulha do Sniper solto na cena. |

## 2. Ajuste

- **Chão:** todo chão onde o player pode pular precisa da tag **Pulavel** e de um collider.
- **KitDaFase > Limites:** mova `LimiteEsquerda`/`LimiteDireita` (até onde a câmera vai),
  `ParedeEsquerda` (início da fase) e `FimDaFase` (fim da fase; troque `Proxima Cena`).
- **Sniper:** coloque em cima da plataforma e ajuste o Z de `LimiteFrente`/`LimiteTras`
  para ele parar antes da borda.
- **PortaEsconderijo:** X = dentro do batente (o player entra até ali), Z = centro da porta, Y = chão.

## 3. Build

Adicione a cena em **File > Build Profiles** (a ordem atual é Main, Fase 1, Fase 2, Perdeu, Ganhou).
