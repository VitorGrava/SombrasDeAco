using UnityEngine;

public class Porta : Interactor
{
    public Transform player;

    [SerializeField] private float deslocamentoZ;
    private bool estaEscondido = false;

    protected override void Start()
    {
        player = GameObject.Find("Player").transform;

        // Caixa estreita: larga o suficiente pro player, fina em Z (profundidade)
        // X = largura da porta, Y = altura, Z = quão "fundo" detecta
        SetTamanhoColisor(
            tamanho: new Vector3(1.2f, 2.5f, 4f),
            offset: new Vector3(0.6f, 0f, 0f)   // ajuste Z se quiser empurrar pra frente/trás
        );

        base.Start();
    }

    protected override void Interagir()
    {
        if (!estaEscondido)
            Esconder();
        else
            SairEsconderijo();
    }

    private void Esconder()
    {
        player.position = new Vector3(player.position.x, player.position.y, player.position.z + deslocamentoZ);
        estaEscondido = true;
        GerenciadorEstadoJogador.Instancia.SetEscondido(true);
        Debug.Log("Player se escondeu!");
    }

    private void SairEsconderijo()
    {
        player.position = new Vector3(player.position.x, player.position.y, player.position.z - deslocamentoZ);
        estaEscondido = false;
        GerenciadorEstadoJogador.Instancia.SetEscondido(false);
        Debug.Log("Player saiu do esconderijo!");
    }
}