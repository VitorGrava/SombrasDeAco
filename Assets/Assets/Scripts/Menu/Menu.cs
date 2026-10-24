using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject painelPausa;
    private bool pausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado) Continuar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        painelPausa.SetActive(true);
        Time.timeScale = 0f;
        pausado = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continuar()
    {
        painelPausa.SetActive(false);
        Time.timeScale = 1f;
        pausado = false;
        // Se o seu jogo trava o cursor, reative aqui:
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f; // importante!
        SceneManager.LoadScene("Menu"); // nome da sua cena de menu
    }

    public void Sair()
    {
        Application.Quit();
    }
}