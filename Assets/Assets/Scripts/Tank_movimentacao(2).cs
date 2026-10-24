using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Tank_movimentacao : MonoBehaviour
{
    // ============================================================
    //  PATRULHA
    // ============================================================
    public enum EixoMovimento { LateralX, FrenteTrasZ }

    [Header("Patrulha")]
    [Tooltip("LateralX = anda de um lado pro outro (esquerda/direita). FrenteTrasZ = anda pra frente e para trás.")]
    public EixoMovimento eixoMovimento = EixoMovimento.LateralX;
    [Tooltip("Se marcado, o tank gira 180° toda vez que inverte a direção da patrulha. Se desmarcado, ele mantém a mesma rotação e só desliza para o outro lado.")]
    public bool girarAoInverterDirecao = true;
    public float velocidadePatrulha = 3f;
    public float distanciaPatrulha = 15f;
    public float velocidadeRotacao = 5f;
    public float suavizacaoMovimento = 0.5f;

    // ============================================================
    //  PERSEGUIÇÃO (opcional — desliga se o boss só deve atirar parado)
    // ============================================================
    [Header("Perseguição")]
    public bool perseguirPlayer = false;
    public float velocidadeSeguir = 5f;

    // ============================================================
    //  DETECÇÃO (Campo de Visão)
    // ============================================================
    [Header("Detecção")]
    [Tooltip("Arraste aqui o objeto filho que tem o componente CampoDeVisao. Se deixar vazio, o script procura sozinho nos filhos.")]
    public CampovisaoTank campovisaoTank;

    // ============================================================
    //  TIRO
    // ============================================================
    [Header("Tiro")]
    public GameObject balaPrefab;
    public Transform pontoDisparo;
    public float intervaloEntreTiros = 1.5f;
    private float tempoUltimoTiro = 0f;

    // ============================================================
    //  REFERÊNCIAS
    // ============================================================
    [Header("Referências")]
    public Transform player;

    private Rigidbody rb;

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
        pontoInicial = transform.position;
        rotacaoAlvoY = transform.eulerAngles.y; // evita giro indesejado logo no Start

        // Se não foi arrastado manualmente no Inspector, tenta achar nos filhos.
        // Isso evita o NullReferenceException que travava o jogo antes.
        if (campovisaoTank == null)
            campovisaoTank = GetComponentInChildren<CampovisaoTank>();

        ConfigurarRigidbody();
        EncontrarPlayer();
    }

    private void ConfigurarRigidbody()
    {
        // Trava o eixo Y sempre, e trava o eixo horizontal que NÃO está sendo usado
        // pelo movimento (evita deslizamento indesejado no eixo parado).
        RigidbodyConstraints eixoParado = eixoMovimento == EixoMovimento.FrenteTrasZ
            ? RigidbodyConstraints.FreezePositionX
            : RigidbodyConstraints.FreezePositionZ;

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY |
                         eixoParado;

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
        // playerAvistado só é true se o campoDeVisao existir E detectar o player.
        // Assim, mesmo que o CampoDeVisao não esteja configurado, o boss
        // continua patrulhando em vez de travar o jogo.
        bool playerAvistado = campovisaoTank != null && campovisaoTank.playerInSight;

        if (playerAvistado)
        {
            if (perseguirPlayer && player != null)
                SeguirPlayer();
            else
                PararEMirar();

            TentarAtirar();
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
        bool moveEmZ = eixoMovimento == EixoMovimento.FrenteTrasZ;

        // Posição atual no eixo de movimento (X ou Z, dependendo da opção)
        float posAtual = moveEmZ ? transform.position.z : transform.position.x;
        float posInicial = moveEmZ ? pontoInicial.z : pontoInicial.x;

        float limiteMin = posInicial - (distanciaPatrulha / 2f);
        float limiteMax = posInicial + (distanciaPatrulha / 2f);

        // Ângulos de rotação: em Z, 0°=frente / 180°=trás. Em X, 90°/270° como antes.
        float anguloPositivo = moveEmZ ? 0f : 90f;
        float anguloNegativo = moveEmZ ? 180f : 270f;

        if (posAtual >= limiteMax && direcaoMovimento > 0)
        {
            direcaoMovimento = -1f;
            if (girarAoInverterDirecao) rotacaoAlvoY = anguloNegativo;
        }
        else if (posAtual <= limiteMin && direcaoMovimento < 0)
        {
            direcaoMovimento = 1f;
            if (girarAoInverterDirecao) rotacaoAlvoY = anguloPositivo;
        }

        float velocidadeAlvo = direcaoMovimento * velocidadePatrulha;

        velocidadeAtualX = Mathf.SmoothDamp(
            velocidadeAtualX,
            velocidadeAlvo,
            ref velocidadeRefX,
            suavizacaoMovimento
        );

        Vector3 novaPos = rb.position;

        if (moveEmZ)
            novaPos.z += velocidadeAtualX * Time.fixedDeltaTime;
        else
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
    //  PARAR E MIRAR (quando não persegue, só gira pro player pra atirar)
    // ============================================================
    private void PararEMirar()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();
            rotacaoAlvoY = Quaternion.LookRotation(dir).eulerAngles.y;
        }

        // Freia suavemente o movimento horizontal
        velocidadeAtualX = Mathf.SmoothDamp(velocidadeAtualX, 0f, ref velocidadeRefX, suavizacaoMovimento);
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
    //  TIRO
    // ============================================================

    private void TentarAtirar()
    {
        // --- VERIFICAÇÕES (mostra o motivo se o tiro não sair) ---
        if (balaPrefab == null)
        {
            Debug.LogWarning("[TIRO] Falta o balaPrefab no Inspector", this);
            return;
        }
        if (pontoDisparo == null)
        {
            Debug.LogWarning("[TIRO] Falta o pontoDisparo no Inspector", this);
            return;
        }
        if (Time.time - tempoUltimoTiro < intervaloEntreTiros) return; // sem log, senão enche o Console

        tempoUltimoTiro = Time.time;

        // --- DISPARO ---
        GameObject b = Instantiate(balaPrefab, pontoDisparo.position, pontoDisparo.rotation);
        Debug.Log("[TIRO] Bala criada em " + pontoDisparo.position + " | direção: " + pontoDisparo.forward, b);

        Bala bala = b.GetComponent<Bala>();
        if (bala != null)
        {
            bala.Atirar();
            Debug.Log("[TIRO] bala.Atirar() chamado com sucesso");
        }
        else
        {
            Debug.LogWarning("[TIRO] O prefab da bala NÃO tem o script Bala", b);
        }
    }
}
