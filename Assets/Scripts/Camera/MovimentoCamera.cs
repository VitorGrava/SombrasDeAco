using UnityEngine;

public class MovimentoCamera : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    [Tooltip("Opcional: se vazio, a câmera não é limitada.")]
    public Transform limiteEsquerda;
    public Transform limiteDireita;

    [Header("Suavização")]
    [SerializeField] private float suavizacaoZ = 0.15f;
    [SerializeField] private float suavizacaoY = 0.2f;

    [Header("Look-Ahead (antecipa direção)")]
    [SerializeField] private float distanciaLookAhead = 2.5f;
    [SerializeField] private float suavizacaoLookAhead = 0.3f;

    [Header("Dead Zone")]
    [SerializeField] private bool usarDeadZone = true;
    [SerializeField] private float deadZoneZ = 0.5f;

    [Header("Seguir Y")]
    [SerializeField] private bool seguirY = false;
    [SerializeField] private float offsetY = 0f;

    [Header("Offset base")]
    [SerializeField] private float offsetZ = 0f;

    private float velocidadeRefZ;
    private float velocidadeRefY;
    private float lookAheadAtual;
    private float velocidadeLookAhead;
    private float ultimaDirecao = 1f;
    private float ultimoZPlayer;
    private float velocidadePlayerSuave;

    private void Start()
    {
        if (player == null)
        {
            // PlayerController primeiro (o filho "Detectado" também tem a tag Player)
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }
        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }
        if (player == null) { enabled = false; return; }

        ultimoZPlayer = player.position.z;

        // Começa direto na posição do player (sem slide inicial)
        transform.position = new Vector3(
            transform.position.x,
            seguirY ? player.position.y + offsetY : transform.position.y,
            player.position.z + offsetZ
        );
    }

    private void LateUpdate()
    {
        // Player destruído ao morrer (a cena "Perdeu" carrega no frame seguinte)
        if (player == null) return;

        // O player anda com MovePosition, então rb.linearVelocity fica 0:
        // a velocidade vem da variação da posição (já interpolada) entre frames
        if (Time.deltaTime > 0f)
        {
            float velocidadeFrame = (player.position.z - ultimoZPlayer) / Time.deltaTime;
            velocidadePlayerSuave = Mathf.Lerp(velocidadePlayerSuave, velocidadeFrame, 10f * Time.deltaTime);
        }
        ultimoZPlayer = player.position.z;
        AtualizarLookAhead(velocidadePlayerSuave);

        float alvoZ = player.position.z + offsetZ + lookAheadAtual;

        // Dead zone: o alvo fica preso na borda da zona (em vez de parar a câmera de vez),
        // assim ela acompanha sem o efeito trava-e-anda
        if (usarDeadZone)
        {
            float diferenca = alvoZ - transform.position.z;
            if (Mathf.Abs(diferenca) < deadZoneZ)
                alvoZ = transform.position.z;
            else
                alvoZ -= Mathf.Sign(diferenca) * deadZoneZ;
        }

        // Limites do mapa (no eixo Z)
        if (limiteEsquerda != null && limiteDireita != null)
        {
            float min = Mathf.Min(limiteEsquerda.position.z, limiteDireita.position.z);
            float max = Mathf.Max(limiteEsquerda.position.z, limiteDireita.position.z);
            alvoZ = Mathf.Clamp(alvoZ, min, max);
        }

        float alvoY = seguirY ? player.position.y + offsetY : transform.position.y;

        float novoZ = Mathf.SmoothDamp(transform.position.z, alvoZ, ref velocidadeRefZ, suavizacaoZ);
        float novoY = Mathf.SmoothDamp(transform.position.y, alvoY, ref velocidadeRefY, suavizacaoY);

        // X não muda: é a distância da câmera até o cenário
        transform.position = new Vector3(transform.position.x, novoY, novoZ);
    }

    private void AtualizarLookAhead(float velocidade)
    {
        if (velocidade > 0.1f) ultimaDirecao = 1f;
        else if (velocidade < -0.1f) ultimaDirecao = -1f;

        float alvo = Mathf.Abs(velocidade) > 0.1f ? ultimaDirecao * distanciaLookAhead : 0f;

        lookAheadAtual = Mathf.SmoothDamp(lookAheadAtual, alvo, ref velocidadeLookAhead, suavizacaoLookAhead);
    }
}