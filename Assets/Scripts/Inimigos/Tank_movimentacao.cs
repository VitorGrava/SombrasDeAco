using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Tank_movimentacao : MonoBehaviour
{
    public enum EixoMovimento { LateralX, FrenteTrasZ }

    [Header("Movimento")]
    public EixoMovimento eixoMovimento = EixoMovimento.LateralX;
    public float velocidadeSeguir = 5f;
    [Tooltip("Para de se aproximar quando o player estiver a essa distância (no eixo de movimento).")]
    public float distanciaParar = 4f;
    public float velocidadeRotacao = 5f;
    public float suavizacaoMovimento = 0.5f;

    [Header("Ativação")]
    [Tooltip("Marcado = já segue o player ao dar Play. Desmarcado = só segue depois que o TankSpawnTrigger chamar IniciarPerseguicao.")]
    public bool seguirDesdeInicio = false;
    [Tooltip("SÓ PARA TESTE: anda sempre na direção em que está virado ao dar Play, ignorando o player.")]
    public bool andarAoDarPlay = false;

    [Header("Limite do mapa (opcional)")]
    [Tooltip("Se marcado, o tank não passa desses valores no eixo de movimento.")]
    public bool limitarArea = false;
    public float limiteMin = -20f;
    public float limiteMax = 20f;

    [Header("Referências")]
    public Transform player;

    private Rigidbody rb;
    private bool perseguicaoAtiva = false;
    private float rotacaoAlvoY;
    private float velocidadeAtual = 0f;
    private float velocidadeRef = 0f;
    private float sinalTeste = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rotacaoAlvoY = transform.eulerAngles.y;

        RigidbodyConstraints eixoParado = eixoMovimento == EixoMovimento.FrenteTrasZ
            ? RigidbodyConstraints.FreezePositionX
            : RigidbodyConstraints.FreezePositionZ;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         eixoParado;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        EncontrarPlayer();

        if (seguirDesdeInicio) perseguicaoAtiva = true;

        if (andarAoDarPlay)
        {
            bool emZ = eixoMovimento == EixoMovimento.FrenteTrasZ;
            float f = emZ ? transform.forward.z : transform.forward.x;
            sinalTeste = f >= 0f ? 1f : -1f;
        }
    }

    // Chamado pelo TankSpawnTrigger
    public void IniciarPerseguicao(Transform alvo)
    {
        if (alvo != null) player = alvo;
        perseguicaoAtiva = true;
    }

    private void EncontrarPlayer()
    {
        if (player != null) return;
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }

    void FixedUpdate()
    {
        if (player == null) EncontrarPlayer();

        bool moveEmZ = eixoMovimento == EixoMovimento.FrenteTrasZ;
        float velocidadeAlvo = 0f;

        if (perseguicaoAtiva && player != null)
        {
            float posTank = moveEmZ ? rb.position.z : rb.position.x;
            float posPlayer = moveEmZ ? player.position.z : player.position.x;
            float delta = posPlayer - posTank;
            float sinal = delta >= 0f ? 1f : -1f;

            if (Mathf.Abs(delta) > 0.1f)
            {
                float anguloPositivo = moveEmZ ? 0f : 90f;
                float anguloNegativo = moveEmZ ? 180f : 270f;
                rotacaoAlvoY = sinal > 0f ? anguloPositivo : anguloNegativo;
            }

            if (Mathf.Abs(delta) > distanciaParar)
                velocidadeAlvo = sinal * velocidadeSeguir;

            if (limitarArea)
            {
                if (posTank >= limiteMax && velocidadeAlvo > 0f) velocidadeAlvo = 0f;
                if (posTank <= limiteMin && velocidadeAlvo < 0f) velocidadeAlvo = 0f;
            }
        }

        // Teste: ignora o player e anda para onde está virado
        if (andarAoDarPlay)
        {
            velocidadeAlvo = sinalTeste * velocidadeSeguir;

            if (limitarArea)
            {
                float posTank = moveEmZ ? rb.position.z : rb.position.x;
                if (posTank >= limiteMax && velocidadeAlvo > 0f) velocidadeAlvo = 0f;
                if (posTank <= limiteMin && velocidadeAlvo < 0f) velocidadeAlvo = 0f;
            }
        }

        MoverNoEixo(velocidadeAlvo, moveEmZ);
        AtualizarRotacao();
    }

    private void MoverNoEixo(float velocidadeAlvo, bool moveEmZ)
    {
        velocidadeAtual = Mathf.SmoothDamp(velocidadeAtual, velocidadeAlvo, ref velocidadeRef, suavizacaoMovimento);

        Vector3 v = rb.linearVelocity;
        if (moveEmZ) v.z = velocidadeAtual;
        else v.x = velocidadeAtual;
        rb.linearVelocity = v;
    }

    private void AtualizarRotacao()
    {
        float yNovo = Mathf.LerpAngle(rb.rotation.eulerAngles.y, rotacaoAlvoY, velocidadeRotacao * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Euler(0f, yNovo, 0f));
    }
}