using UnityEngine;
using UnityEngine.SceneManagement;

public class JogoFinal : MonoBehaviour
{
    private bool jogadorColidiu = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !jogadorColidiu)
        {
            jogadorColidiu = true;
            FimDoJogo();        // ← chama direto aqui, uma única vez
        }
    }

    private void FimDoJogo()
    {
        SceneManager.LoadScene("Ganhou");
    }
}