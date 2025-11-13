using UnityEngine;

public class GerenciadorEstadoJogador : MonoBehaviour
{
    
    public static GerenciadorEstadoJogador Instancia {get; private set; }
    
    private bool estaEscondido = false;

    private void Awake()
    {
        if (Instancia == null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetEscondido(bool Estado)
    {
        estaEscondido = Estado;
    }

    public bool EstaEscondido()
    {
        return estaEscondido;
    }
}
