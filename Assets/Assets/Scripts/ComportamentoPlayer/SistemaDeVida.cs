using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SistemaDeVida : MonoBehaviour
{
    public bool estaMorto = false;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Inimigo") && !estaMorto)
        {
            Destroy(gameObject);
            SceneManager.LoadScene("Perdeu");
            estaMorto = true;
        }else estaMorto = false;
    }
    
    
}
