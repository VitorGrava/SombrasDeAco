using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    // Velocidades
    public float velocidadeAndando;
    public float velocidadeAgachando;
    public float velocidadeCorrendo;

    // Rotação
    public float velocidadeRotacao;
    private float RotYLoc = 90f;

    // Pulo
    public float forcaPulo;
    public bool noChao;

    // ---- MELHORIAS DO PULO ----
    public float coyoteTime = 0.15f;       // Tempo extra de pulo após sair do chão
    public float jumpBufferTime = 0.01f;   // Tempo de buffer para o botão de pulo

    private float coyoteCounter;
    private float jumpBufferCounter;
    // ----------------------------

    private float moveX; 

    public enum EstadoMovimento { Andando, Agachando, Correndo }
    private EstadoMovimento estadoAtual;

    // Componentes
    private Animator animator;
    private Rigidbody rb;
    private Vector3 posicaoAtual;

    [HideInInspector] public ControleStamina controleStamina;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        controleStamina = GetComponent<ControleStamina>();
    }

    public void SetVelocidadeCorrrendo(float velocidade)
    {
        velocidadeCorrendo = velocidade;
    }

    void FixedUpdate()
    {
        if (!GerenciadorEstadoJogador.Instancia.EstaEscondido())
        {
            MovimentacaoPlayer();
            AtualizarEstadoMovimento();
            Pulo();
        }
    }

    void MovimentacaoPlayer()
    {
        moveX = Input.GetAxis("Horizontal");
        posicaoAtual = new Vector3(moveX, 0f, 0f);

        switch (estadoAtual)
        {
            case EstadoMovimento.Andando:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeAndando * Time.fixedDeltaTime);
                animator.SetBool("IsCrouching", false);
                controleStamina.estaCorrendo = false;
                break;

            case EstadoMovimento.Agachando:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeAgachando * Time.fixedDeltaTime);
                animator.SetBool("IsCrouching", true);
                break;

            case EstadoMovimento.Correndo:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeCorrendo * Time.fixedDeltaTime);
                animator.SetBool("IsCrouching", false);
                controleStamina.estaCorrendo = true;
                controleStamina.Correndo();
                break;
        }

        Rotacao(moveX);
        animator.SetBool("IsMoving", moveX != 0);
    }

    void AtualizarEstadoMovimento()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (controleStamina.stamina > 0)
                estadoAtual = EstadoMovimento.Correndo;
            else
                estadoAtual = EstadoMovimento.Andando;

            return;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            estadoAtual = EstadoMovimento.Agachando;
            return;
        }

        estadoAtual = EstadoMovimento.Andando;
    }

    // -----------------------------
    //   SISTEMA DE PULO MELHORADO
    // -----------------------------
    void Pulo()
    {
        // BUFFER DO PULO
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.fixedDeltaTime;

        // COYOTE TIME
        if (noChao)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // EXECUTA O PULO
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
            rb.AddForce(new Vector3(0f, forcaPulo, 0f), ForceMode.Impulse);

            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pulavel"))
            noChao = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pulavel"))
            noChao = false;
    }

    private void Rotacao(float moveX)
    {
        if (moveX > 0) RotYLoc = 90f;
        else if (moveX < 0) RotYLoc = 270f;

        float yAtual = transform.eulerAngles.y;
        float yNovo = Mathf.LerpAngle(yAtual, RotYLoc, velocidadeRotacao * Time.deltaTime);

        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
    }
}
