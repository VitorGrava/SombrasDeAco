using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    // Usado pelo JOGAR do menu e pelo JOGAR NOVAMENTE de Perdeu/Ganhou:
    // sempre recomeça da primeira fase
    public void PlayGame()
    {
        SceneManager.LoadScene("Fase 1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
