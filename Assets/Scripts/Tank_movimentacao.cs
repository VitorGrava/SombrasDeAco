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
    [Tooltip("Se marcado, o tank gira 180° toda vez que inverte a direção da patrulha.")]
    public bool girarAoInverterDirecao = true;
    public float velocidadePatrulha = 3f;
    public float distanciaPatrulha = 15f;
    public float velocidadeRotacao = 5f;
    public float suavizacaoMovimento = 0.5f;

    // ============================================================
    //  PERSEGUIÇÃO
    // ============================================================
    [Header("Perseguição")]
    [Tooltip("Persegue o player quando ele entra no campo de visão (comportamento antigo).")]
    public bool perseguirPlayer = false;
    public float velocidadeSeguir = 5f;
    [Tooltip("Distância (no eixo de movimento) em que o tank para de se aproximar do player.")]
    public float distanciaMinimaDoPlayer = 4f;

    [Header("Perseguição forçada (boss spawnado)")]
    [Tooltip("Marque para o tank já andar em modo boss assim que der Play (para testar sem o trigger). Anda para onde estiver virado.")]
    public bool modoBossDesdeInicio = false;
    [Tooltip("Quando a perseguição forçada está ativa, ele atira se o player estiver a essa distância e à frente do tank.")]
    public float alcanceTiroPerseguicao = 12f;
    [Tooltip("Quão alinhado o tank precisa estar do player para atirar (1 = exato, 0 = qualquer direção à frente).")]
    [Range(0f, 1f)] public float alinhamentoParaAtirar = 0.8f;

    // ============================================================
    //  DETECÇÃO (Campo de Visão)
    // ============================================================
    [Header("Detecção")]
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

    // Perseguição forçada (ligada pelo TankSpawnTrigger)
    private bool perseguicaoAtiva = false;
    private Vector3 direcaoFrente = Vector3.forward;
    private Collider[] collidersTank;

    [Header("Debug / Obstáculos")]
    [Tooltip("Loga no Console, 1x por segundo, o estado do tank e qualquer objeto que esteja bloqueando ele de lado.")]
    public bool debugMovimento = true;
    [Tooltip("Layers que o tank deve atravessar (ex.: sacos de areia, arame). Crie um layer, coloque esses objetos nele e marque aqui.")]
    public LayerMask camadasIgnoradas;
    private float proximoLogEstado = 0f;
    private float proximoLogBloqueio = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pontoInicial = transform.position;
        rotacaoAlvoY = transform.eulerAngles.y;

        if (campovisaoTank == null)
            campovisaoTank = GetComponentInChildren<CampovisaoTank>();

        ConfigurarRigidbody();
        EncontrarPlayer();
        collidersTank = GetComponentsInChildren<Collider>();
        IgnorarCamadasConfiguradas();

        if (modoBossDesdeInicio && !perseguicaoAtiva)
            IniciarPerseguicao(null);
    }

    /// <summary>
    /// Liga o modo boss: o tank anda sempre para a frente, na direção em que
    /// foi spawnado (não segue o player). Pode ser chamado logo após o Instantiate.
    /// O player é usado só para decidir se atira.
    /// </summary>
    public void IniciarPerseguicao(Transform alvo)
    {
        if (alvo != null) player = alvo;

        Vector3 f = transform.forward;
        f.y = 0f;
        direcaoFrente = f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;

        perseguicaoAtiva = true;
        Debug.Log("[TANK] Perseguição iniciada | direção: " + direcaoFrente + " | velocidade: " + velocidadeSeguir, this);
    }

    private void ConfigurarRigidbody()
    {
        RigidbodyConstraints eixoParado = eixoMovimento == EixoMovimento.FrenteTrasZ
            ? RigidbodyConstraints.FreezePositionX
            : RigidbodyConstraints.FreezePositionZ;

        // Y fica livre para a gravidade agir; trava só o eixo horizontal que NÃO é usado
        // e as rotações que fariam o tank tombar.
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         eixoParado;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // Faz o tank atravessar os objetos dos layers marcados em camadasIgnoradas
    private void IgnorarCamadasConfiguradas()
    {
        if (camadasIgnoradas.value == 0) return;

        Collider[] todos = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        foreach (Collider c in todos)
        {
            if (((1 << c.gameObject.layer) & camadasIgnoradas.value) == 0) continue;

            foreach (Collider t in collidersTank)
                if (c != t) Physics.IgnoreCollision(c, t);
        }
    }

    // Loga o que está barrando o tank de lado (o chão é ignorado)
    void OnCollisionStay(Collision col)
    {
        if (!debugMovimento || Time.time < proximoLogBloqueio) return;
        if (col.contactCount == 0 || col.GetContact(0).normal.y > 0.5f) return;

        proximoLogBloqueio = Time.time + 1f;
        Debug.Log("[TANK] BLOQUEADO por: " + col.gameObject.name +
                  " | layer: " + LayerMask.LayerToName(col.gameObject.layer), col.gameObject);
    }

    private void EncontrarPlayer()
    {
        if (player != null) return;

        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }


    void FixedUpdate()
    {
        if (debugMovimento && Time.time >= proximoLogEstado)
        {
            proximoLogEstado = Time.time + 1f;
            Debug.Log("[TANK] modoBoss=" + perseguicaoAtiva + " | vel=" + rb.linearVelocity + " | x=" + rb.position.x.ToString("F2"), this);
        }

        if (perseguicaoAtiva)
        {
            // Modo boss: só vai para a frente, independente do player e do campo de visão
            AndarParaFrente();

            if (PlayerNaMira())
                TentarAtirar();
        }
        else
        {
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
        }

        AtualizarRotacao();
    }

    // ============================================================
    //  MOVIMENTO DE PATRULHA
    // ============================================================
    private void MovimentarPatrulha()
    {
        bool moveEmZ = eixoMovimento == EixoMovimento.FrenteTrasZ;

        float posAtual = moveEmZ ? transform.position.z : transform.position.x;
        float posInicial = moveEmZ ? pontoInicial.z : pontoInicial.x;

        float limiteMin = posInicial - (distanciaPatrulha / 2f);
        float limiteMax = posInicial + (distanciaPatrulha / 2f);

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

        MoverNoEixo(direcaoMovimento * velocidadePatrulha, moveEmZ);
    }

    // ============================================================
    //  MODO BOSS: SEMPRE PARA A FRENTE (direção travada no spawn)
    // ============================================================
    private void AndarParaFrente()
    {
        bool moveEmZ = eixoMovimento == EixoMovimento.FrenteTrasZ;
        float componente = moveEmZ ? direcaoFrente.z : direcaoFrente.x;
        float sinal = componente >= 0f ? 1f : -1f;

        MoverNoEixo(sinal * velocidadeSeguir, moveEmZ);
    }

    // ============================================================
    //  PERSEGUIR PLAYER (comportamento antigo, via campo de visão)
    // ============================================================
    private void SeguirPlayer()
    {
        bool moveEmZ = eixoMovimento == EixoMovimento.FrenteTrasZ;

        float delta = moveEmZ
            ? player.position.z - rb.position.z
            : player.position.x - rb.position.x;

        float sinal = delta >= 0f ? 1f : -1f;

        // Vira pro lado do player
        if (Mathf.Abs(delta) > 0.1f)
        {
            float anguloPositivo = moveEmZ ? 0f : 90f;
            float anguloNegativo = moveEmZ ? 180f : 270f;
            rotacaoAlvoY = sinal > 0f ? anguloPositivo : anguloNegativo;
        }

        // Só anda se ainda estiver longe o suficiente
        float velocidadeAlvo = Mathf.Abs(delta) > distanciaMinimaDoPlayer
            ? sinal * velocidadeSeguir
            : 0f;

        MoverNoEixo(velocidadeAlvo, moveEmZ);
    }

    // ============================================================
    //  PARAR E MIRAR
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

        MoverNoEixo(0f, eixoMovimento == EixoMovimento.FrenteTrasZ);
    }

    // ============================================================
    //  MOVIMENTO COMUM (suaviza e aplica no Rigidbody)
    // ============================================================
    private void MoverNoEixo(float velocidadeAlvo, bool moveEmZ)
    {
        velocidadeAtualX = Mathf.SmoothDamp(
            velocidadeAtualX,
            velocidadeAlvo,
            ref velocidadeRefX,
            suavizacaoMovimento
        );

        // Usa a velocidade do Rigidbody só no eixo horizontal, preservando a velocidade
        // vertical (gravidade). MovePosition zerava o efeito da queda.
        Vector3 v = rb.linearVelocity;

        if (moveEmZ)
            v.z = velocidadeAtualX;
        else
            v.x = velocidadeAtualX;

        rb.linearVelocity = v;
    }

    // ============================================================
    //  ROTAÇÃO
    // ============================================================
    private void AtualizarRotacao()
    {
        float yNovo = Mathf.LerpAngle(
            rb.rotation.eulerAngles.y,
            rotacaoAlvoY,
            velocidadeRotacao * Time.fixedDeltaTime
        );

        rb.MoveRotation(Quaternion.Euler(0f, yNovo, 0f));
    }

    // ============================================================
    //  TIRO
    // ============================================================

    // Usado só na perseguição forçada: player perto e na frente do tank
    private bool PlayerNaMira()
    {
        if (player == null) return false;

        if (GerenciadorEstadoJogador.Instancia != null &&
            GerenciadorEstadoJogador.Instancia.EstaEscondido())
            return false;

        Vector3 para = player.position - transform.position;
        para.y = 0f;

        if (para.magnitude > alcanceTiroPerseguicao) return false;

        return Vector3.Dot(transform.forward, para.normalized) >= alinhamentoParaAtirar;
    }

    private void TentarAtirar()
    {
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
        if (Time.time - tempoUltimoTiro < intervaloEntreTiros) return;

        tempoUltimoTiro = Time.time;

        GameObject b = Instantiate(balaPrefab, pontoDisparo.position, pontoDisparo.rotation);

        // A bala não pode colidir com o tank que a disparou (senão ele bate nela e trava)
        if (collidersTank != null)
        {
            foreach (Collider cBala in b.GetComponentsInChildren<Collider>())
                foreach (Collider cTank in collidersTank)
                    Physics.IgnoreCollision(cBala, cTank);
        }

        Bala bala = b.GetComponent<Bala>();
        if (bala != null)
            bala.Atirar();
        else
            Debug.LogWarning("[TIRO] O prefab da bala NÃO tem o script Bala", b);
    }
}