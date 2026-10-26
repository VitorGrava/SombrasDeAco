using UnityEngine;

public class CampoDeVisao : MonoBehaviour
{
    [Header("Eixo do lado da tela")]
    [Tooltip("Eixo do mundo que corresponde a esquerda/direita na tela (o SniperController sobrescreve com o dele)")]
    public MovimentoInimigo.Eixo eixoLateral = MovimentoInimigo.Eixo.X;

    [Header("Configuração do Campo de Visão")]
    [Tooltip("Raio máximo de detecção do jogador")]
    public float viewRadius = 5f;

    [Tooltip("Ângulo de abertura do campo de visão (em graus)")]
    [Range(0, 360)]
    public float viewAngle = 120f;

    [Header("Camadas")]
    [Tooltip("Layer(s) identificando o jogador")]
    public LayerMask playerLayer;

    [Tooltip("Layer(s) de obstáculos (paredes, plataformas, etc)")]
    public LayerMask obstacleLayer;

    [Header("Referências")]
    [Tooltip("Transform do inimigo ou ponto central do campo de visão")]
    public Transform eyePosition;

    [Tooltip("Tempo entre as checagens de FOV (segundos)")]
    public float checkInterval = 0.2f;

    [Header("Estado")]
    public bool playerInSight;

    [Header("Debug")]
    public bool logDebug = true;

    private Transform playerTransform;
    private Transform dono; // raiz do inimigo, para o raio ignorar o próprio corpo

    private bool EmZ => eixoLateral == MovimentoInimigo.Eixo.Z;

    // Eixo de profundidade (câmera → cenário): o cone gira em torno dele
    private Vector3 EixoProfundidade => EmZ ? Vector3.right : Vector3.forward;

    void Start()
    {
        if (eyePosition == null)
            eyePosition = this.transform;

        Rigidbody rbDono = GetComponentInParent<Rigidbody>();
        dono = rbDono != null ? rbDono.transform : transform.root;

        if (playerLayer.value == 0)
            Debug.LogWarning($"{name}: playerLayer está vazia no Inspector. O player nunca será visto!", this);
        if (obstacleLayer.value == 0)
            Debug.LogWarning($"{name}: obstacleLayer está vazia no Inspector. Paredes não vão bloquear a visão.", this);

        StartCoroutine(FOVCoroutine());
    }

    System.Collections.IEnumerator FOVCoroutine()
    {
        var espera = new WaitForSeconds(checkInterval);
        while (true)
        {
            yield return espera;

            bool antes = playerInSight;
            FOVCheck();

            if (logDebug && antes != playerInSight)
                Debug.Log($"{dono.name}: playerInSight = {playerInSight}", this);
        }
    }

    void FOVCheck()
    {
        playerInSight = false;

        // Se o player estiver escondido, nunca está à vista
        if (GerenciadorEstadoJogador.Instancia != null &&
            GerenciadorEstadoJogador.Instancia.EstaEscondido())
            return;

        Collider[] players = Physics.OverlapSphere(eyePosition.position, viewRadius, playerLayer);

        foreach (var target in players)
        {
            playerTransform = target.transform;

            // Mira no centro do collider (o pivô costuma ficar no pé, colado no chão)
            Vector3 alvo = target.bounds.center;
            Vector3 paraPlayer = alvo - eyePosition.position;

            // Ângulo calculado no plano da tela: ignora a profundidade
            // ANTES de normalizar, senão o vetor deixa de ser unitário
            Vector3 noPlano = paraPlayer;
            if (EmZ) noPlano.x = 0;
            else     noPlano.z = 0;

            Vector3 fwd = eyePosition.right;
            if (EmZ) fwd.x = 0;
            else     fwd.z = 0;

            float angle = Vector3.Angle(fwd, noPlano);
            if (angle >= viewAngle / 2f) continue;

            // O raio usa o vetor real (com profundidade) para chegar de fato no player
            if (TemLinhaDeVisao(alvo, target))
            {
                playerInSight = true;
                Debug.DrawLine(eyePosition.position, alvo, Color.green, checkInterval);
                break;
            }
        }
    }

    // Raio contra obstáculos + player; ignora colliders do próprio inimigo e triggers.
    // O primeiro hit válido precisa ser o player.
    private bool TemLinhaDeVisao(Vector3 alvo, Collider player)
    {
        Vector3 origem = eyePosition.position;
        Vector3 dir = alvo - origem;
        float dist = dir.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(origem, dir / dist, dist + 0.1f,
                                               obstacleLayer | playerLayer,
                                               QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(dono)) continue;

            if (hit.collider == player || ((1 << hit.collider.gameObject.layer) & playerLayer.value) != 0)
                return true;

            Debug.DrawLine(origem, hit.point, Color.magenta, checkInterval);
            return false; // obstáculo na frente
        }

        // Nada no caminho (ex.: collider do player é trigger)
        return true;
    }

    // Gizmos só compilam no Editor
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform olho = eyePosition != null ? eyePosition : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(olho.position, viewRadius);

        // Cone no plano da tela: gira em torno do eixo de profundidade
        Vector3 fwd = olho.right;
        Vector3 angleA = Quaternion.AngleAxis(-viewAngle / 2, EixoProfundidade) * fwd;
        Vector3 angleB = Quaternion.AngleAxis( viewAngle / 2, EixoProfundidade) * fwd;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(olho.position, olho.position + angleA * viewRadius);
        Gizmos.DrawLine(olho.position, olho.position + angleB * viewRadius);
        Gizmos.DrawLine(olho.position, olho.position + fwd * viewRadius);

        if (playerInSight && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(olho.position, playerTransform.position);
        }
    }
#endif
}
