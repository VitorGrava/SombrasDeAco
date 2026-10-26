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
        // DontDestroyOnLoad só funciona na raiz; dentro do prefab KitDaFase ele vive só na cena
        // (cada fase traz o seu, e o estado já é zerado a cada cena)
        if (transform.parent == null)
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