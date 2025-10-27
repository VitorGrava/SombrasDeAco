using System;
using UnityEngine;

public abstract class Interactor : MonoBehaviour
{
    [SerializeField] private float raioColisor;
    public bool playerInteracao; 

    private void Start()
    {
        ColisorConfig();
    }   
 
    private void ColisorConfig()
    {
        SphereCollider colisor = gameObject.AddComponent<SphereCollider>();
        colisor.radius = raioColisor;
        colisor.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInteracao = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInteracao == true)
        {
            Interagir();
        }
        
    
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInteracao = false;
        }
    }
    
    protected abstract void Interagir();

}    
