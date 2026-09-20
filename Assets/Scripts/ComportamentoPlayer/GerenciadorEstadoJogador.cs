using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorEstadoJogador : MonoBehaviour
{
    public static GerenciadorEstadoJogador Instancia { get; private set; }

    private bool estaEscondido = false;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        // ← escuta toda vez que uma cena nova carrega
        SceneManager.sceneLoaded += OnCenaCarregada;
    }

    private void OnDestroy()
    {
        // ← boa prática: remove o listener ao destruir
        SceneManager.sceneLoaded -= OnCenaCarregada;
    }

    private void OnCenaCarregada(Scene cena, LoadSceneMode modo)
    {
        // ← reseta o estado sempre que uma cena nova começa
        estaEscondido = false;
    }

    public void SetEscondido(bool estado)
    {
        estaEscondido = estado;
    }

    public bool EstaEscondido()
    {
        return estaEscondido;
    }
}