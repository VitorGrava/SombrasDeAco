using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class MovimentoInimigo : MonoBehaviour
{
    public enum Eixo { X, Z }

    [Header("Eixo do lado da tela")]
    [Tooltip("Eixo do mundo que corresponde a esquerda/direita na tela")]
    public Eixo eixoLateral = Eixo.X;

    [Header("Parâmetros de Patrulha")]
    public float velocidadePatrulha = 3f;
    public float velocidadeSeguir = 5f;
    public float distanciaPatrulha = 5f;
    public float velocidadeRotacao = 5f;

    [Header("Perseguição")]
    public float distanciaParada = 1f;

    [Header("Humanização do Movimento")]
    public float suavizacaoMovimento = 0.5f;

    [Header("Referências")]
    public Transform player;

    private Rigidbody rb;
    private Animator animator;
    private CampoDeVisaoLateral campoDeVisao;

    private float pontoInicial;
    private float direcaoMovimento = 1f;
    private float rotacaoAlvoY;

    private float velocidadeAtual = 0f;
    private float velocidadeRef = 0f;

    private bool EmZ => eixoLateral == Eixo.Z;

    // Ângulos: Z -> 0° (+Z) / 180° (-Z) | X -> 90° (+X) / 270° (-X)
    private float AnguloPositivo => EmZ ? 0f : 90f;
    private float AnguloNegativo => EmZ ? 180f : 270f;

    private float Pos(Vector3 v) => EmZ ? v.z : v.x;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        campoDeVisao = GetComponentInChildren<CampoDeVisaoLateral>();

        if (campoDeVisao == null)
            Debug.LogWarning($"{name}: CampoDeVisaoLateral não encontrado nos filhos!", this);

        pontoInicial = Pos(transform.position);
        rotacaoAlvoY = AnguloPositivo;

        ConfigurarRigidbody();
        EncontrarPlayer();
    }

    private void ConfigurarRigidbody()
    {
        RigidbodyConstraints profundidade = EmZ
            ? RigidbodyConstraints.FreezePositionX
            : RigidbodyConstraints.FreezePositionZ;

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY |
                         profundidade;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void EncontrarPlayer()
    {
        if (player != null) return;

        // Prefab não guarda referência da cena: acha o player sozinho.
        // PlayerController primeiro (o filho "Detectado" também tem a tag Player)
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) { player = pc.transform; return; }

        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }

    void FixedUpdate()
    {
        bool escondido = GerenciadorEstadoJogador.Instancia != null
                         && GerenciadorEstadoJogador.Instancia.EstaEscondido();
        bool vendoPlayer = campoDeVisao != null && campoDeVisao.playerInSight;

        if (!escondido && vendoPlayer && player != null)
            SeguirPlayer();
        else
            MovimentarPatrulha();

        AtualizarRotacao();
    }

    private void MovimentarPatrulha()
    {
        float limiteMin = pontoInicial - (distanciaPatrulha / 2f);
        float limiteMax = pontoInicial + (distanciaPatrulha / 2f);
        float atual = Pos(rb.position);

        if (atual >= limiteMax && direcaoMovimento > 0) direcaoMovimento = -1f;
        else if (atual <= limiteMin && direcaoMovimento < 0) direcaoMovimento = 1f;

        rotacaoAlvoY = direcaoMovimento > 0 ? AnguloPositivo : AnguloNegativo;

        Mover(direcaoMovimento * velocidadePatrulha);
    }

    private void SeguirPlayer()
    {
        float diferenca = Pos(player.position) - Pos(rb.position);
        float dir = Mathf.Sign(diferenca);

        direcaoMovimento = dir;
        rotacaoAlvoY = dir > 0 ? AnguloPositivo : AnguloNegativo;

        float alvo = Mathf.Abs(diferenca) > distanciaParada ? dir * velocidadeSeguir : 0f;
        Mover(alvo);
    }

    private void Mover(float velocidadeAlvo)
    {
        velocidadeAtual = Mathf.SmoothDamp(velocidadeAtual, velocidadeAlvo,
                                           ref velocidadeRef, suavizacaoMovimento);

        Vector3 novaPos = rb.position;
        if (EmZ) novaPos.z += velocidadeAtual * Time.fixedDeltaTime;
        else     novaPos.x += velocidadeAtual * Time.fixedDeltaTime;

        rb.MovePosition(novaPos);
    }

    private void AtualizarRotacao()
    {
        float yNovo = Mathf.LerpAngle(transform.eulerAngles.y, rotacaoAlvoY,
                                      velocidadeRotacao * Time.fixedDeltaTime);
        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
    }
}   