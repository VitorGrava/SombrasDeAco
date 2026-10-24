using UnityEngine;

public class MovimentoCamera : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public Transform limiteEsquerda;
    public Transform limiteDireita;

    [Header("Suavização")]
    [SerializeField] private float suavizacaoX = 0.15f;
    [SerializeField] private float suavizacaoY = 0.2f;

    [Header("Look-Ahead (antecipa direção)")]
    [SerializeField] private float distanciaLookAhead = 2.5f;
    [SerializeField] private float suavizacaoLookAhead = 0.3f;

    [Header("Dead Zone (câmera só move fora dela)")]
    [SerializeField] private float deadZoneX = 0.5f; // distância mínima pra câmera reagir
    [SerializeField] private bool usarDeadZone = true;

    [Header("Seguir Y")]
    [SerializeField] private bool seguirY = false;
    [SerializeField] private float offsetY = 0f;

    [Header("Offset base")]
    [SerializeField] private float offsetX = 0f;

    // Internos
    private Vector3 velocidadeAtual;
    private float lookAheadAtual;
    private float velocidadeLookAhead;
    private float ultimaDirecaoX = 1f;
    private Rigidbody2D playerRb;

    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody2D>();

        // Inicializa câmera direto na posição do player (sem slide inicial)
        transform.position = new Vector3(
            player.position.x + offsetX,
            seguirY ? player.position.y + offsetY : transform.position.y,
            transform.position.z
        );
    }

    private void FixedUpdate()
    {
        float velocidadePlayerX = playerRb != null ? playerRb.linearVelocity.x : 0f;

        AtualizarLookAhead(velocidadePlayerX);

        float alvoX = player.position.x + offsetX + lookAheadAtual;

        // Dead zone: só move a câmera se o player saiu da zona central
        if (usarDeadZone)
        {
            float diferencaX = alvoX - transform.position.x;
            if (Mathf.Abs(diferencaX) < deadZoneX)
                alvoX = transform.position.x; // congela no eixo X
        }

        // Limita dentro do mapa
        alvoX = Mathf.Clamp(alvoX, limiteEsquerda.position.x, limiteDireita.position.x);

        float alvoY = seguirY
            ? player.position.y + offsetY
            : transform.position.y;

        Vector3 posicaoAlvo = new Vector3(alvoX, alvoY, transform.position.z);

        // SmoothDamp com velocidade diferente por eixo
        float novoX = Mathf.SmoothDamp(transform.position.x, posicaoAlvo.x, ref velocidadeAtual.x, suavizacaoX);
        float novoY = Mathf.SmoothDamp(transform.position.y, posicaoAlvo.y, ref velocidadeAtual.y, suavizacaoY);

        transform.position = new Vector3(novoX, novoY, transform.position.z);
    }

    private void AtualizarLookAhead(float velocidadeX)
    {
        if (velocidadeX > 0.1f)
            ultimaDirecaoX = 1f;
        else if (velocidadeX < -0.1f)
            ultimaDirecaoX = -1f;

        // Se o player está se movendo, antecipa; se parado, volta ao centro
        float lookAheadAlvo = Mathf.Abs(velocidadeX) > 0.1f
            ? ultimaDirecaoX * distanciaLookAhead
            : 0f;

        lookAheadAtual = Mathf.SmoothDamp(
            lookAheadAtual,
            lookAheadAlvo,
            ref velocidadeLookAhead,
            suavizacaoLookAhead
        );
    }
}