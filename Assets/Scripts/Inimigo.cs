using System;
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    public float velocidade;
    public float velocidadeRotacao;
    public float limitePatrulha;
    public Transform origemPatrulha;
    public bool indoDireita;
    private Quaternion rotacaoAlvo;
    
    CampoDeVisao campoDeVisao;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        campoDeVisao = GetComponent<CampoDeVisao>();
        rotacaoAlvo = Quaternion.Euler(0, 0, 0); 
    }

    void Update()
    {
        MovimentoPatrulha();
    }   
    
    private void MovimentoPatrulha()
    {
        float direcao = indoDireita ? 1 : -1;

        // Move no eixo X
        transform.Translate(Vector2.right * direcao * velocidade * Time.deltaTime, Space.World);

        float distancia = transform.position.x - origemPatrulha.position.x;
        if (indoDireita && distancia >= limitePatrulha)
        {
            Rotacao();
        }
        else if (!indoDireita && distancia <= -limitePatrulha)
        {
            Rotacao();
        }
    }

    private void Rotacao(float moveX)
    {
        if (moveX > 0) RotYLoc = 0f;      // direita
        else if (moveX < 0) RotYLoc = 180f; // esquerda

        float yAtual = transform.eulerAngles.y;
        float yNovo = Mathf.LerpAngle(yAtual, RotYLoc, velocidadeRotacao * Time.deltaTime);

        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
    }

}
    