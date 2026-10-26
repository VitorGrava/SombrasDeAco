using UnityEngine;
using UnityEngine.SceneManagement;

// Gatilho de fim de fase: quando o player entra, carrega a próxima cena.
// Fase 1 -> "Fase 2" | Fase 2 -> "Ganhou"
public class JogoFinal : MonoBehaviour
{
    [Tooltip("Cena carregada quando o player passa por aqui (precisa estar no Build Profiles)")]
    public string proximaCena = "Ganhou";

    private bool jogadorColidiu = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !jogadorColidiu)
        {
            jogadorColidiu = true;
            FimDaFase();        // ← chama direto aqui, uma única vez
        }
    }

    private void FimDaFase()
    {
        SceneManager.LoadScene(proximaCena);
    }
}
