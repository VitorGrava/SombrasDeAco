using System;
using UnityEngine;

public abstract class Interactor : MonoBehaviour
{
    [SerializeField] private float raioColisor;
    public bool playerPodeInteragir;
    public bool playerInteragiu = false;

    protected virtual void Start()
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
            playerPodeInteragir = true;
        }
    }
    

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerPodeInteragir = false;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerPodeInteragir == true)
        {
            Interagir();
        }
        
    
    }


    public void SetRaioColisor(float raio)
    {
        raioColisor = raio;
    }


    protected abstract void Interagir();

}    
