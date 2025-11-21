using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class JogoFinal : MonoBehaviour
{
    private bool JogadorColidiu = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            JogadorColidiu = true;
        }
    }

    void Update()
    {
        if(JogadorColidiu)
        {
            FimDoJogo();    
        }       
    }

    private void FimDoJogo()
    {
        SceneManager.LoadScene("Ganhou");
    }
}
