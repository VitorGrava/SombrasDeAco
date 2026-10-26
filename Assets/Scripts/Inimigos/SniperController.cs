using UnityEngine;

public class SniperController : MonoBehaviour
{
    [Header("Eixo do lado da tela")]
    public MovimentoInimigo.Eixo eixoLateral = MovimentoInimigo.Eixo.Z;

    private Rigidbody rb;
    private Collider col;

    [SerializeField]
    private Transform campoDeVisaoObjeto;

    public CampoDeVisao campoDeVisao;

    // ====== PATRULHA ======
    public LayerMask limitePatrulha;

    private Vector3 direcao;
    private float velocidade = 3f;

    // ====== PARADA / CHECAGEM ======
    private bool estaParado = false;
    private float tempodeChecagem = 2f;
    private float tempoParado = 0f;

    // ====== ROTAÇÃO DO SNIPER ======
    private float rotacaoAlvoY;
    private float velocidadeRotacao = 20f;

    // ====== ROTAÇÃO DO CAMPO DE VISÃO ======
    public float grauRotacao = -30f;
    public float velocidadeRotacaoFOV = 5f;

    private Quaternion rotacaoOriginalFOV;
    private Quaternion rotacaoBaixoFOV;

    [Header("TIRO")]
    public GameObject balaPrefab;
    public Transform pontoDisparo;
    public float intervaloEntreTiros = 1.5f;
    private float tempoUltimoTiro = -999f;

    [Header("Debug")]
    public bool logDebug = true;

    private bool EmZ => eixoLateral == MovimentoInimigo.Eixo.Z;
    private float AnguloPositivo => EmZ ? 0f : 90f;    // +Z: 0° | +X: 90°
    private float AnguloNegativo => EmZ ? 180f : 270f; // -Z: 180° | -X: 270°

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (campoDeVisao == null)
            campoDeVisao = GetComponentInChildren<CampoDeVisao>();

        if (campoDeVisao == null)
            Debug.LogWarning($"{name}: nenhum CampoDeVisao nos filhos. O Sniper nunca vai atirar!", this);
        else
            campoDeVisao.eixoLateral = eixoLateral; // FOV precisa usar o mesmo eixo do Sniper

        if (campoDeVisaoObjeto == null)
            campoDeVisaoObjeto = campoDeVisao != null ? campoDeVisao.transform : transform;

        if (balaPrefab == null || pontoDisparo == null)
            Debug.LogWarning($"{name}: balaPrefab ou pontoDisparo não atribuído. O Sniper não vai atirar.", this);

        direcao = EmZ ? Vector3.forward : Vector3.right;
        rotacaoAlvoY = AnguloPositivo;

        // "Olhar pra baixo" = inclinar a partir da rotação original, no eixo local Z do FOV.
        // Como a frente do FOV é o .right local, girar em Z local abaixa a frente.
        rotacaoOriginalFOV = campoDeVisaoObjeto.localRotation;
        rotacaoBaixoFOV = rotacaoOriginalFOV * Quaternion.Euler(0f, 0f, grauRotacao);

        SoltarLimitesFilhos();
        ConfigurarRigidbody();
    }

    // Limites colocados como filhos do Sniper andam junto com ele e o raio nunca os alcança.
    // Soltamos no mundo e viramos trigger para não bloquear o player.
    // A posição é recolocada na linha de patrulha: o deslocamento local do filho vira
    // deslocamento no eixo lateral, independente da rotação da instância na cena
    // (o Sniper é girado para 0°/180° em runtime, então a rotação original não vale).
    private void SoltarLimitesFilhos()
    {
        if (limitePatrulha.value == 0)
        {
            Debug.LogWarning($"{name}: limitePatrulha está vazia no Inspector!", this);
            return;
        }

        Vector3 centro = col != null ? col.bounds.center : transform.position;

        int soltos = 0;
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
        {
            if (c == col) continue;
            if (((1 << c.gameObject.layer) & limitePatrulha.value) == 0) continue;

            Vector3 local = c.transform.localPosition;
            float offset = Mathf.Abs(local.z) >= Mathf.Abs(local.x) ? local.z : local.x;

            Vector3 pos = centro; // mesma profundidade e altura do raio
            if (EmZ) pos.z += offset;
            else     pos.x += offset;

            c.transform.SetParent(null, true);
            c.transform.SetPositionAndRotation(pos, Quaternion.identity);
            c.isTrigger = true;
            soltos++;

            if (logDebug)
                Debug.Log($"{name}: limite '{c.name}' posicionado em {pos}", this);
        }

        if (logDebug && soltos > 0)
            Debug.Log($"{name}: {soltos} limite(s) filho(s) soltos no mundo.", this);

        bool existeLimite = false;
        foreach (Collider c in FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (((1 << c.gameObject.layer) & limitePatrulha.value) != 0) { existeLimite = true; break; }
        }

        if (!existeLimite)
            Debug.LogWarning($"{name}: nenhum Collider na layer de limitePatrulha na cena. O Sniper vai andar pra sempre!", this);
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

    void Update()
    {
        AtualizarCampoDeVisao();

        if (estaParado)
        {
            TempoParado();
            ChecarPlayerParaAtirar();
            return;
        }

        AtualizarRotacao();
    }

    void FixedUpdate()
    {
        if (estaParado) return;

        SniperPatrulha();
        ChecarLimite();
    }

    private void SniperPatrulha()
    {
        rb.MovePosition(rb.position + direcao * velocidade * Time.fixedDeltaTime);
    }

    private void ChecarLimite()
    {
        // Alcance = metade da largura do collider no eixo lateral + folga
        Bounds b = col != null ? col.bounds : new Bounds(transform.position, Vector3.one);
        float meiaLargura = EmZ ? b.extents.z : b.extents.x;
        float distancia = meiaLargura + 0.3f;

        Debug.DrawRay(b.center, direcao * distancia, Color.red); // visível na Scene

        if (Physics.Raycast(b.center, direcao, out RaycastHit hit, distancia,
                            limitePatrulha, QueryTriggerInteraction.Collide))
        {
            if (logDebug)
                Debug.Log($"{name}: bateu no limite '{hit.collider.name}', parando {tempodeChecagem}s", this);

            estaParado = true;
            tempoParado = 0f;

            // A direção já inverte aqui, mas a rotação só acontece depois da parada
            // (AtualizarRotacao não roda enquanto estaParado), então ele olha pra baixo
            // virado para o lado do limite.
            direcao *= -1f;
            rotacaoAlvoY = (EmZ ? direcao.z : direcao.x) > 0 ? AnguloPositivo : AnguloNegativo;
        }
    }

    private void AtualizarRotacao()
    {
        float yNovo = Mathf.LerpAngle(
            transform.eulerAngles.y,
            rotacaoAlvoY,
            velocidadeRotacao * Time.deltaTime
        );

        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
    }

    private void TempoParado()
    {
        tempoParado += Time.deltaTime;
        if (tempoParado >= tempodeChecagem)
        {
            estaParado = false;
            tempoParado = 0f;

            if (logDebug)
                Debug.Log($"{name}: voltando a patrulhar, girando para {rotacaoAlvoY}°", this);
        }
    }

    private void ChecarPlayerParaAtirar()
    {
        if (campoDeVisao == null || !campoDeVisao.playerInSight) return;
        if (balaPrefab == null || pontoDisparo == null) return;
        if (Time.time - tempoUltimoTiro < intervaloEntreTiros) return;

        tempoUltimoTiro = Time.time;

        GameObject b = Instantiate(balaPrefab, pontoDisparo.position, pontoDisparo.rotation);

        Bala bala = b.GetComponent<Bala>();
        if (bala != null)
            bala.Atirar();
        else
            Debug.LogWarning($"{name}: balaPrefab não tem o componente Bala!", this);

        if (logDebug)
            Debug.Log($"{name}: tiro disparado de {pontoDisparo.position}", this);
    }

    private void AtualizarCampoDeVisao()
    {
        Quaternion alvo = estaParado ? rotacaoBaixoFOV : rotacaoOriginalFOV;

        campoDeVisaoObjeto.localRotation = Quaternion.Lerp(
            campoDeVisaoObjeto.localRotation,
            alvo,
            velocidadeRotacaoFOV * Time.deltaTime
        );
    }
}
