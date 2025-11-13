using System;
using UnityEngine;

public class SistemaDeVida : MonoBehaviour
{
    public bool estaMorto = false;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Inimigo") && !estaMorto)
        {
            Destroy(gameObject);
            estaMorto = true;
        }else estaMorto = false;
    }
    
    
}
