using UnityEngine;

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
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;   // ← era 0.01f, muito pequeno

    // Multiplicadores de queda (sensação de peso)
    public float gravityMultiplier = 2.5f;     // cai mais rápido
    public float lowJumpMultiplier = 2.0f;     // pulo curto ao soltar cedo

    private float coyoteCounter;
    private float jumpBufferCounter;
    // ----------------------------

    private float moveX;

    public enum EstadoMovimento { Andando, Agachando, Correndo }
    private EstadoMovimento estadoAtual;

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

    // ─────────────────────────────────────────
    //  Update: APENAS leitura de input do pulo
    //  GetButtonDown é confiável só no Update!
    // ─────────────────────────────────────────
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
    }

    void FixedUpdate()
    {
        if (!GerenciadorEstadoJogador.Instancia.EstaEscondido())
        {
            MovimentacaoPlayer();
            AtualizarEstadoMovimento();
            Pulo();
            AplicarGravidade();
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

                // Só consome stamina se estiver se movendo
                if (moveX != 0)
                {
                    controleStamina.estaCorrendo = true;
                    controleStamina.Correndo();
                }
                else
                {
                    controleStamina.estaCorrendo = false;
                }
                break;
        }


        Rotacao(moveX);
        animator.SetBool("IsMoving", moveX != 0);

    }

    void AtualizarEstadoMovimento()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            estadoAtual = controleStamina.stamina > 0
                ? EstadoMovimento.Correndo
                : EstadoMovimento.Andando;
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            estadoAtual = EstadoMovimento.Agachando;
            return;
        }

        estadoAtual = EstadoMovimento.Andando;
    }

    // ─────────────────────────────────────────
    //  SISTEMA DE PULO
    //  jumpBufferCounter já vem do Update()
    // ─────────────────────────────────────────
    void Pulo()
    {
        // Decrementa o buffer aqui (não lê GetButtonDown aqui!)
        jumpBufferCounter -= Time.fixedDeltaTime;

        // Coyote time
        if (noChao)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // Executa o pulo
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
            rb.AddForce(new Vector3(0f, forcaPulo, 0f), ForceMode.Impulse);

            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }
    }

    // ─────────────────────────────────────────
    //  GRAVIDADE MELHORADA
    //  Cai pesado; pulo curto ao soltar cedo
    // ─────────────────────────────────────────
    void AplicarGravidade()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Caindo — aplica multiplicador de queda
            rb.AddForce(Vector3.up * Physics.gravity.y * (gravityMultiplier - 1f) * rb.mass * Time.fixedDeltaTime, ForceMode.Force);
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            // Subindo, mas o jogador soltou o botão → pulo mais curto
            rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f) * rb.mass * Time.fixedDeltaTime, ForceMode.Force);
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