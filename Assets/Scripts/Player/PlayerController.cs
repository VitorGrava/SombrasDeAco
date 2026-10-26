using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Velocidades
    public float velocidadeAndando;
    public float velocidadeAgachando;
    public float velocidadeCorrendo;

    // Rotação
    public float velocidadeRotacao;
    private float RotYLoc = 0f;

    // Pulo
    public float forcaPulo;
    public bool noChao;

    // ---- MELHORIAS DO PULO ----
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    // Multiplicadores de queda (sensação de peso)
    public float gravityMultiplier = 2.5f;     // cai mais rápido
    public float lowJumpMultiplier = 2.0f;     // pulo curto ao soltar cedo

    private float coyoteCounter;
    private float jumpBufferCounter;
    // ----------------------------

    private float moveInput;

    public enum EstadoMovimento { Andando, Agachando, Correndo }
    private EstadoMovimento estadoAtual;

    private Animator animator;
    private Rigidbody rb;
    private Vector3 posicaoAtual;

    [HideInInspector] public ControleStamina controleStamina;

    // Chãos "Pulavel" encostados agora: ao passar de um chão pro outro o Exit
    // do primeiro não pode zerar noChao enquanto ainda pisa no segundo
    private readonly System.Collections.Generic.HashSet<Collider> chaosTocando = new System.Collections.Generic.HashSet<Collider>();

    // Animação de pulo só entra depois de um tempinho no ar (evita piscar em degraus/emendas)
    public float tempoNoArParaAnimar = 0.12f;
    private float tempoNoAr;

    // ---- COLISOR AGACHADO ----
    [Tooltip("Altura do colisor agachado, em fração da altura em pé")]
    [Range(0.3f, 1f)] public float fatorAlturaAgachado = 0.6f;
    private CapsuleCollider capsula;
    private float alturaEmPe;
    private Vector3 centroEmPe;
    private bool colisorAgachado;
    // --------------------------

    void Start()
    {
        // O Animator fica no modelo filho (EliasKael), não na raiz
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        controleStamina = GetComponent<ControleStamina>();

        capsula = GetComponent<CapsuleCollider>();
        if (capsula != null)
        {
            alturaEmPe = capsula.height;
            centroEmPe = capsula.center;
        }

        // Física roda no FixedUpdate e a câmera por frame: sem interpolação o player treme
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void SetVelocidadeCorrrendo(float velocidade)
    {
        velocidadeCorrendo = velocidade;
    }

    // Update: apenas leitura de input do pulo (GetButtonDown só é confiável aqui)
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
    }

    void FixedUpdate()
    {
        if (GerenciadorEstadoJogador.Instancia != null && GerenciadorEstadoJogador.Instancia.EstaEscondido())
            return;

        MovimentacaoPlayer();
        AtualizarEstadoMovimento();
        AtualizarColisorAgachado();
        Pulo();
        AplicarGravidade();
    }

    void MovimentacaoPlayer()
    {
        moveInput = Input.GetAxis("Horizontal");

        // Movimento no eixo Z (lado da tela)
        posicaoAtual = new Vector3(0f, 0f, moveInput);

        switch (estadoAtual)
        {
            case EstadoMovimento.Andando:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeAndando * Time.fixedDeltaTime);
                if (animator != null) animator.SetBool("IsCrouching", false);
                if (controleStamina != null) controleStamina.estaCorrendo = false;
                break;

            case EstadoMovimento.Agachando:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeAgachando * Time.fixedDeltaTime);
                if (animator != null) animator.SetBool("IsCrouching", true);
                if (controleStamina != null) controleStamina.estaCorrendo = false;
                break;

            case EstadoMovimento.Correndo:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeCorrendo * Time.fixedDeltaTime);
                if (animator != null) animator.SetBool("IsCrouching", false);

                // Só consome stamina se estiver se movendo
                if (moveInput != 0)
                {
                    if (controleStamina != null)
                    {
                        controleStamina.estaCorrendo = true;
                        controleStamina.Correndo();
                    }
                }
                else
                {
                    if (controleStamina != null) controleStamina.estaCorrendo = false;
                }
                break;
        }

        Rotacao(moveInput);
        if (animator != null)
        {
            animator.SetBool("IsMoving", moveInput != 0);
            animator.SetBool("IsRunning", estadoAtual == EstadoMovimento.Correndo);
            tempoNoAr = noChao ? 0f : tempoNoAr + Time.fixedDeltaTime;
            animator.SetBool("IsGrounded", tempoNoAr < tempoNoArParaAnimar);
        }
    }

    void AtualizarEstadoMovimento()
    {
        // Agachar vem primeiro: segurando Shift ele não corre, mesmo com Ctrl apertado
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            estadoAtual = EstadoMovimento.Agachando;
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            estadoAtual = (controleStamina != null && controleStamina.stamina > 0)
                ? EstadoMovimento.Correndo
                : EstadoMovimento.Andando;
            return;
        }

        estadoAtual = EstadoMovimento.Andando;
    }

    // Agachado: encolhe o colisor mantendo a base no chão.
    // Se tem teto em cima (ex.: passou agachado por baixo de algo), continua agachado.
    void AtualizarColisorAgachado()
    {
        if (capsula == null) return;

        if (colisorAgachado && estadoAtual != EstadoMovimento.Agachando && TetoBloqueado())
            estadoAtual = EstadoMovimento.Agachando;

        bool agachar = estadoAtual == EstadoMovimento.Agachando;
        if (agachar == colisorAgachado) return;
        colisorAgachado = agachar;

        float altura = agachar ? alturaEmPe * fatorAlturaAgachado : alturaEmPe;
        capsula.height = altura;
        capsula.center = centroEmPe - new Vector3(0f, (alturaEmPe - altura) / 2f, 0f);
    }

    bool TetoBloqueado()
    {
        Bounds b = capsula.bounds;
        float raio = capsula.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) * 0.95f;
        float falta = (alturaEmPe - capsula.height) * transform.lossyScale.y;
        Vector3 origem = new Vector3(b.center.x, b.max.y - raio, b.center.z);

        return Physics.SphereCast(origem, raio, Vector3.up, out _, falta,
                                  ~(1 << gameObject.layer), QueryTriggerInteraction.Ignore);
    }

    // SISTEMA DE PULO (jumpBufferCounter vem do Update)
    void Pulo()
    {
        jumpBufferCounter -= Time.fixedDeltaTime;

        // Coyote time
        if (noChao)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // Executa o pulo
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            // Zera só a velocidade vertical, preservando o movimento
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(new Vector3(0f, forcaPulo, 0f), ForceMode.Impulse);

            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }
    }

    // GRAVIDADE MELHORADA: cai pesado; pulo curto ao soltar cedo
    void AplicarGravidade()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.up * Physics.gravity.y * (gravityMultiplier - 1f) * rb.mass * Time.fixedDeltaTime, ForceMode.Force);
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f) * rb.mass * Time.fixedDeltaTime, ForceMode.Force);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pulavel"))
        {
            chaosTocando.Add(collision.collider);
            noChao = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pulavel"))
        {
            chaosTocando.Remove(collision.collider);
            chaosTocando.RemoveWhere(c => c == null);
            noChao = chaosTocando.Count > 0;
        }
    }

    // Rotação: 0° = +Z (direita da tela), 180° = -Z (esquerda da tela)
    private void Rotacao(float input)
    {
        if (input > 0) RotYLoc = 0f;
        else if (input < 0) RotYLoc = 180f;

        // Gira pelo Rigidbody (não pelo transform) para não brigar com a interpolação
        float yAtual = rb.rotation.eulerAngles.y;
        float yNovo = Mathf.LerpAngle(yAtual, RotYLoc, velocidadeRotacao * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Euler(0f, yNovo, 0f));
    }
}