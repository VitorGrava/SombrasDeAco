using UnityEngine;

public class CampovisaoTank : MonoBehaviour
{
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

    private Transform playerTransform;

    void Start()
    {
        if (eyePosition == null)
            eyePosition = this.transform;

        StartCoroutine(FOVCoroutine());
    }

    System.Collections.IEnumerator FOVCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            FOVCheck();
        }
    }

    void FOVCheck()
    {
        // Se o player estiver escondido, nunca está à vista
        if (GerenciadorEstadoJogador.Instancia.EstaEscondido())
        {
            playerInSight = false;
            return;
        }

        playerInSight = false;

        Collider[] players = Physics.OverlapSphere(eyePosition.position, viewRadius, playerLayer);
        Debug.Log("Colliders encontrados: " + players.Length);

        foreach (var target in players)
        {
            playerTransform = target.transform;

            Vector3 directionToPlayer = playerTransform.position - eyePosition.position;
            
            directionToPlayer.y = 0; // projeta no plano horizontal (Y é a vertical do mundo)
            directionToPlayer.Normalize();

            Vector3 fwd = eyePosition.forward; // frente real do objeto
            float angle = Vector3.Angle(fwd, directionToPlayer);

            if (angle < viewAngle / 2f)
            {
                float distToPlayer = Vector3.Distance(eyePosition.position, playerTransform.position);

                if (!Physics.Raycast(eyePosition.position, directionToPlayer, distToPlayer, obstacleLayer))
                {
                    playerInSight = true;
                }
            }
        }
    }

    // Gizmos só compilam no Editor — protegido corretamente
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (eyePosition == null)
            eyePosition = this.transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition.position, viewRadius);

        Vector3 fwd = eyePosition.forward;
        Vector3 angleA = Quaternion.Euler(0, -viewAngle / 2, 0) * fwd;
        Vector3 angleB = Quaternion.Euler(0, viewAngle / 2, 0) * fwd;


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(eyePosition.position, eyePosition.position + angleA * viewRadius);
        Gizmos.DrawLine(eyePosition.position, eyePosition.position + angleB * viewRadius);

        if (playerInSight && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePosition.position, playerTransform.position);
        }
    }
#endif
}