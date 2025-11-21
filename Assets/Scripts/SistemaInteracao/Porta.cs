using Unity.VisualScripting;
using UnityEngine;


public class Porta : Interactor
{
    public Transform player;

    [SerializeField] private float deslocamentoZ;
    private bool estaEscondido = false;
    
    protected override void Interagir()
    {
        Debug.Log("Porta Detectada");
        if (!estaEscondido)
        {
            Esconder();
        }
        else
        {
            SairEsconderijo();
        }

        //MudarDirecao();
       
    }

    protected override void Start()
    {
        player = GameObject.Find("Player").transform;
        SetRaioColisor(2.5f);
        base.Start();
    }

    private void Esconder()
    {
        player.transform.position = new Vector3(player.position.x, player.position.y, player.position.z + deslocamentoZ);
        estaEscondido = true;
        GerenciadorEstadoJogador.Instancia.SetEscondido(true);
        Debug.Log("Player se escondeu!");
    }

    private void SairEsconderijo()
    {
        player.transform.position = new Vector3(player.position.x, player.position.y, player.position.z - deslocamentoZ);
        estaEscondido = false;
        GerenciadorEstadoJogador.Instancia.SetEscondido(false);
        Debug.Log("Player saiu do esconderijo!");
        
    }
}
