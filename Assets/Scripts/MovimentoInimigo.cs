using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class MovimentoInimigo : MonoBehaviour
{
    [Header("Parâmetros de Patrulha")]
    public float velocidadePatrulha = 3f;
    public float velocidadeSeguir = 5f;
    public float distanciaPatrulha = 5f;
    public float velocidadeRotacao = 5f;

    [Header("Humanização do Movimento")]
    public float suavizacaoMovimento = 0.5f;

    [Header("Referências")]
    public Transform player;

    private Rigidbody rb;
    private Animator animator;
    private CampoDeVisao campoDeVisao;

    // Movimento
    private Vector3 pontoInicial;
    private float direcaoMovimento = 1f;

    // Rotação
    private float rotacaoAlvoY;

    // Suavização do movimento
    private float velocidadeAtualX = 0f;
    private float velocidadeRefX = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        campoDeVisao = GetComponentInChildren<CampoDeVisao>();

        pontoInicial = transform.position;

        ConfigurarRigidbody();
        EncontrarPlayer();
    }

    private void ConfigurarRigidbody()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY|
                         RigidbodyConstraints.FreezePositionZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void EncontrarPlayer()
    {
        if (player != null) return;

        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }


    void FixedUpdate()
    {
        if (GerenciadorEstadoJogador.Instancia.EstaEscondido())
        {
            MovimentarPatrulha();
        }
        else if (campoDeVisao.playerInSight)
        {
            SeguirPlayer();
        }
        else
        {
            MovimentarPatrulha();
        }

        AtualizarRotacao();
    }

    // ============================================================
    //  MOVIMENTO DE PATRULHA
    // ============================================================
    private void MovimentarPatrulha()
    {
        float limiteEsq = pontoInicial.x - (distanciaPatrulha / 2f);
        float limiteDir = pontoInicial.x + (distanciaPatrulha / 2f);

        // Inverte a direção quando chega nos limites
        if (transform.position.x >= limiteDir && direcaoMovimento > 0)
        {
            direcaoMovimento = -1f;
            rotacaoAlvoY = 270f;
        }
        else if (transform.position.x <= limiteEsq && direcaoMovimento < 0)
        {
            direcaoMovimento = 1f;
            rotacaoAlvoY = 90f;
        }

        // Suavização
        float velocidadeAlvoX = direcaoMovimento * velocidadePatrulha;

        velocidadeAtualX = Mathf.SmoothDamp(
            velocidadeAtualX,
            velocidadeAlvoX,
            ref velocidadeRefX,
            suavizacaoMovimento
        );

        // Movimento horizontal (NÃO mexe no Y)
        Vector3 novaPos = rb.position;
        novaPos.x += velocidadeAtualX * Time.fixedDeltaTime;
        novaPos.y = rb.position.y;

        rb.MovePosition(novaPos);
        
    }

    // ============================================================
    //  PERSEGUIR PLAYER
    // ============================================================
    private void SeguirPlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        dir.Normalize();

        rotacaoAlvoY = Quaternion.LookRotation(dir).eulerAngles.y;

        Vector3 novaPos = rb.position;
        novaPos += dir * velocidadeSeguir * Time.fixedDeltaTime;
        novaPos.y = rb.position.y;

        rb.MovePosition(novaPos);

      
    }

    // ============================================================
    //  ROTAÇÃO
    // ============================================================
    private void AtualizarRotacao()
    {
        float yNovo = Mathf.LerpAngle(
            transform.eulerAngles.y,
            rotacaoAlvoY,
            velocidadeRotacao * Time.fixedDeltaTime
        );

        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
    }

    // ============================================================
    //  ANIMAÇÃO
    // ============================================================
    
}
