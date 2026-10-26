# Sombras de Aço — como baixar e abrir no Windows

## 1. Instale (uma vez só)

1. **Git para Windows** — https://git-scm.com/download/win
   Instale com as opções padrão (o **Git LFS** já vem junto).
2. **Unity Hub** — https://unity.com/download
3. Pelo Unity Hub, instale o **Unity 6000.3.24f1** (exatamente essa versão).
   Se ela não aparecer na lista, use: https://unity.com/releases/editor/archive → procure `6000.3.24f1` → "Install" (abre no Hub).

> Não precisa de nenhum módulo extra para jogar no editor. Só marque **Windows Build Support** se for gerar o `.exe`.

## 2. Baixe o projeto

> ⚠️ **Não use o botão "Download ZIP" do GitHub.** Os modelos 3D grandes (`.fbx`) ficam no Git LFS
> e o ZIP do GitHub vem só com "atalhos" de texto no lugar deles — as fases abrem vazias.

Abra o **Git Bash** (ou PowerShell) numa pasta **com caminho curto**, por exemplo `C:\Projetos`, e rode:

```bash
git lfs install
git clone https://github.com/VitorGrava/SombrasDeAco2.git
cd SombrasDeAco2
git lfs pull
```

Confira se os modelos vieram de verdade: `Assets\Modelos\Cenarios\Fases\Modelo_Fase_1.fbx` tem que ter **~126 MB**
(se tiver só 1 KB, rode `git lfs pull` de novo).

## 3. Abra no Unity

1. Unity Hub → **Projects** → **Add** → **Add project from disk** → escolha a pasta `SombrasDeAco2`.
2. Confira se a versão ao lado do projeto é **6000.3.24f1** e clique para abrir.
3. **A primeira abertura demora** (de 10 a 30 min): o Unity recria a pasta `Library` (~4 GB). É normal.
4. Se aparecer um aviso de pacotes/"Safe Mode", escolha **Ignore**/continuar e espere terminar de importar.

## 4. Jogue

1. Na janela **Project**, abra `Assets/Cenas/Menus/Main.unity` (duplo clique).
2. Aperte **Play** (▶).

Ordem das cenas: **Main → Fase 1 → Fase 2 → Ganhou**; morrendo vai para **Perdeu** (Jogar novamente volta para a Fase 1).

| Tecla | Ação |
|---|---|
| A / D | andar |
| Ctrl | correr (gasta stamina) |
| Shift | agachar (não corre agachado) |
| Espaço | pular |
| E | esconder / sair da porta |

## 5. Onde está cada coisa

| Pasta | Conteúdo |
|---|---|
| `Assets/Cenas` | `Menus` (Main, Perdeu, Ganhou), `Fases` (Fase 1, Fase 2), `Antigas` (testes antigos) |
| `Assets/Prefabs` | `Personagens/Player`, `Inimigos/Sniper` e `Porradeiro`, `Fase/KitDaFase`, `PortaEsconderijo`, `FimDaFase`… |
| `Assets/Scripts` | `Player`, `Inimigos`, `Interacao`, `Cenas`, `Camera` |
| `Assets/Modelos` | `Personagens`, `Inimigos`, `Cenarios` (modelos das fases) |
| `Assets/Animacoes` | animações do player e dos inimigos |

Para montar fase nova arrastando os prefabs: veja `Assets/Prefabs/COMO_MONTAR_UMA_FASE.md`.

## 6. Se der problema

| Problema | Solução |
|---|---|
| Fase abre vazia / sem cenário | os `.fbx` não baixaram: na pasta do projeto rode `git lfs pull` e reabra o Unity |
| Unity pede para "atualizar" o projeto | você abriu com outra versão — feche e instale a **6000.3.24f1** |
| Materiais rosa | o Unity não terminou de importar; espere, ou menu **Assets → Reimport All** |
| Clico em Jogar e nada acontece | **File → Build Profiles**: as cenas `Main`, `Fase 1`, `Fase 2`, `Perdeu`, `Ganhou` precisam estar na lista |
| Erros estranhos depois de trocar de versão/branch | feche o Unity, apague a pasta `Library` e abra de novo (ele recria) |
| Caminho muito longo / erro ao clonar | clone numa pasta curta, tipo `C:\Projetos` |

## 7. Mandando suas mudanças de volta (GitHub)

```bash
git add -A
git commit -m "o que você mudou"
git push
```

Antes de começar a mexer, sempre `git pull` para pegar o que o outro fez.
Evitem editar **a mesma cena** ao mesmo tempo — conflito em arquivo `.unity` é difícil de resolver.
