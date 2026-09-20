using UnityEngine;

public class SniperController : MonoBehaviour
{
    
    private Rigidbody rb;
    private Transform sniper;

    [SerializeField] 
    private Transform campoDeVisaoObjeto;

    public CampoDeVisao campoDeVisao;

// ====== CONFIGURAÇÕES DE PATRULHA ======
    public LayerMask limitePatrulha;

    private Vector3 direcao = Vector3.right;
    private float velocidade = 3f;

// ====== PARADA / CHECAGEM ======
    private bool estaParado = false;
    private float tempodeChecagem = 2f;
    private float tempoParado = 0f;

// ====== ROTAÇÃO DO SNIPER ======
    private float rotacaoAlvoY = 90f;
    private float velocidadeRotacao = 20f;

// ====== ROTAÇÃO DO CAMPO DE VISÃO ======
    public float grauRotacao = -30f; // ângulo para olhar para baixo
    public float velocidadeRotacaoFOV = 5f;

    private Quaternion rotacaoOriginalFOV;
    private Quaternion rotacaoBaixoFOV;
   
    
    [Header("TIRO")]
    public GameObject balaPrefab;     // Prefab da bala
    public Transform pontoDisparo;    // Origem do tiro (adicione um empty no sniper)
    public float intervaloEntreTiros = 1.5f;
    private float tempoUltimoTiro = 0f;
    void Start()
    {        
        campoDeVisao = GetComponentInChildren<CampoDeVisao>();
        rb = GetComponent<Rigidbody>();
        rotacaoOriginalFOV = campoDeVisaoObjeto.localRotation;
        rotacaoBaixoFOV = Quaternion.Euler(campoDeVisaoObjeto.localRotation.x, 270f, grauRotacao);   
        ConfigurarRigidbody();

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

    // Update is called once per frame
    void Update()
    {
        AtualizarCampoDeVisao();
        if (estaParado)
        {
            TempoParado();
            ChecarPlayerParaAtirar();
            return;
        }
        SniperPatrulha();
        AtualizarRotacao();
       

        
        ChecarLimite();
        
        
    }   

    private void SniperPatrulha()
    {
        rb.MovePosition(transform.position + direcao * velocidade * Time.deltaTime);
    }

    private void ChecarLimite()
    {
        // Raycast curto à frente
        float distancia = 0.5f;

        if (Physics.Raycast(transform.position, direcao, distancia, limitePatrulha))
        {
            estaParado = true;
            tempoParado = 0f;
            // Inverter direção
            direcao *= -1;
            if (direcao.x > 0)
                rotacaoAlvoY = 90f; 
            else
                rotacaoAlvoY = 270f;

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
        }
    }
    
    private void ChecarPlayerParaAtirar()
    {
        if (!campoDeVisao.playerInSight) return;

        // Esperar recarga entre tiros
        if (Time.time - tempoUltimoTiro < intervaloEntreTiros) return;

        tempoUltimoTiro = Time.time;

        // Criar bala
        GameObject b = Instantiate(balaPrefab, pontoDisparo.position, pontoDisparo.rotation);

        // Chamar o método Atirar da bala
        Bala bala = b.GetComponent<Bala>();
        if (bala != null)
            bala.Atirar();
    }
    
    private void AtualizarCampoDeVisao()
    {
        if (estaParado)
        {
            // olha para baixo
            campoDeVisaoObjeto.localRotation = Quaternion.Lerp(
                campoDeVisaoObjeto.localRotation,
                rotacaoBaixoFOV,
                velocidadeRotacaoFOV * Time.deltaTime
            );
        }
        else
        {
            // volta ao normal
            campoDeVisaoObjeto.localRotation = Quaternion.Lerp(
                campoDeVisaoObjeto.localRotation,
                rotacaoOriginalFOV,
                velocidadeRotacaoFOV * Time.deltaTime
            );
        }
    }

    
}
