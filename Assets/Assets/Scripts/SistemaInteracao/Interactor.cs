using System;
using UnityEngine;

public abstract class Interactor : MonoBehaviour
{
    [Header("Colisor de Interação")]
    [SerializeField] private Vector3 tamanhoColisor = new Vector3(1f, 2f, 0.5f);
    [SerializeField] private Vector3 offsetColisor = new Vector3(0f, 0f, 0f);

    public bool playerPodeInteragir;
    public bool playerInteragiu = false;

    protected virtual void Start()
    {
        ColisorConfig();
    }

    private void ColisorConfig()
    {
        BoxCollider colisor = gameObject.AddComponent<BoxCollider>();
        colisor.size = tamanhoColisor;
        colisor.center = offsetColisor;
        colisor.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerPodeInteragir = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerPodeInteragir = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerPodeInteragir)
            Interagir();
    }

    public void SetTamanhoColisor(Vector3 tamanho, Vector3 offset = default)
    {
        tamanhoColisor = tamanho;
        offsetColisor = offset;
    }

    protected abstract void Interagir();
}