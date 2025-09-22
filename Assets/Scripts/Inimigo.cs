using System;
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    CampoDeVisao campoDeVisao;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        campoDeVisao = GetComponent<CampoDeVisao>();
    }

    void Update()
    {
        if (campoDeVisao !=null && campoDeVisao.AcharAlvosVisiveis())
        {
            Debug.Log("uhummm funcionou true");
        }
     

        
    }   
}
    