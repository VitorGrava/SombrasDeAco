using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class MovimentoInimigo : MonoBehaviour
{
    [Header("Parâmetros de Patrulha")]
    [Tooltip("Velocidade máxima de movimento do inimigo.")]
    public float velocidadePatrulha = 3f;

    [Tooltip("Velocidade de perseguição do inimigo.")]
    public float velocidadeSeguir;

    [Tooltip("Distância total percorrida entre os pontos de patrulha.")]
    public float distanciaPatrulha = 5f;

    [Tooltip("Velocidade de rotação do inimigo (suavizada).")]
    public float velocidadeRotacao = 5f;

    [Header("Humanização do Movimento")]
    [Tooltip("Tempo de suavização do movimento (maior = mais inércia).")]
    public float suavizacaoMovimento = 0.5f;

    [Header("Referências")]
    public Transform player;

    private Rigidbody rb;
    private Animator animator;
    private CampoDeVisao campoDeVisao;

    // Controle de movimento
    private Vector3 pontoInicial;
    private float direcaoMovimento = 1f;

    // Controle de rotação
    private float rotacaoAlvoY;

    // Controle de suavização
    private float velocidadeAtualX = 0f;
    private float velocidadeRefX = 0f;
   

    void Start()
    {
        InicializarComponentes();
        ConfigurarRigidbody();
        EncontrarPlayer();
    }

    void FixedUpdate()
    {
        if (campoDeVisao.playerInSight)
        {
            SeguirPlayer();
        }
        else
        {
            MovimentarPatrulha();
        }
        
        AtualizarRotacao();
    }

    // =========================
    // == MÉTODOS DE INICIALIZAÇÃO ==
    // =========================

    private void InicializarComponentes()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        campoDeVisao = GetComponentInChildren<CampoDeVisao>();
        pontoInicial = transform.position;
    }

    private void ConfigurarRigidbody()
    {
        if (rb == null) return;

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezePositionZ;
}

    private void EncontrarPlayer()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[MovimentoInimigo] Objeto com tag 'Player' não encontrado.");
        }
    }

    // =========================
    // == MOVIMENTAÇÃO ==
    // =========================

    private void MovimentarPatrulha()
    {
        float limiteEsquerdo = pontoInicial.x - (distanciaPatrulha / 2f);
        float limiteDireito = pontoInicial.x + (distanciaPatrulha / 2f);

        // Muda a direção ao atingir os limites
        if (transform.position.x >= limiteDireito && direcaoMovimento > 0)
        {
            direcaoMovimento = -1f;
            rotacaoAlvoY = 0f;
        }
        else if (transform.position.x <= limiteEsquerdo && direcaoMovimento < 0)
        {
            direcaoMovimento = 1f;
            rotacaoAlvoY = 180f;
        }

        // Calcula velocidade suavizada (inércia)
        float velocidadeAlvoX = direcaoMovimento * velocidadePatrulha;
        velocidadeAtualX = Mathf.SmoothDamp(
            velocidadeAtualX,
            velocidadeAlvoX,
            ref velocidadeRefX,
            suavizacaoMovimento
        );

        // Aplica movimento
        Vector3 movimento = new Vector3(velocidadeAtualX, 0f, 0f);
        rb.MovePosition(rb.position + movimento * Time.fixedDeltaTime);

        //AtualizarAnimacao();
    }

    // =========================
    // == ROTAÇÃO ==
    // =========================

    private void AtualizarRotacao()
    {
        float yAtual = transform.eulerAngles.y;
        float yNovo = Mathf.LerpAngle(yAtual, rotacaoAlvoY, velocidadeRotacao * Time.fixedDeltaTime);
        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
    }

    // =========================
    // == ANIMAÇÃO ==
    // =========================

    // private void AtualizarAnimacao()
    // {
    //     if (animator == null) return;
    //
    //     bool estaAndando = Mathf.Abs(velocidadeAtualX) > 0.1f;
    //     animator.SetBool("IsMoving", estaAndando);
    //     animator.SetBool("IsCrouching", false);
    // }

    // =========================
    // == FUTURO: SEGUIR PLAYER ==
    // =========================

    private void SeguirPlayer()
    {
        Vector3 direcaoPlayer = player.position - transform.position;
        direcaoPlayer.Normalize();
        transform.position += direcaoPlayer * velocidadeSeguir * Time.deltaTime;
    }
}
