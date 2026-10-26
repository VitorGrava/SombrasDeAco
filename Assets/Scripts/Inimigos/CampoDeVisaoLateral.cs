using UnityEngine;

public class CampoDeVisaoLateral : MonoBehaviour
{
    [Header("Campo de Visão")]
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 120f;

    [Header("Camadas")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Referências")]
    [Tooltip("Opcional. Se vazio, usa este objeto como ponto dos olhos.")]
    public Transform eyePosition;
    public float checkInterval = 0.2f;

    [Header("Estado")]
    public bool playerInSight;

    private Transform corpo; // objeto do inimigo (0° = +Z, 180° = -Z)

    void Start()
    {
        if (eyePosition == null) eyePosition = transform;

        MovimentoInimigo mov = GetComponentInParent<MovimentoInimigo>();
        corpo = mov != null ? mov.transform : transform;

        StartCoroutine(FOVCoroutine());
    }

    System.Collections.IEnumerator FOVCoroutine()
    {
        var espera = new WaitForSeconds(checkInterval);
        while (true)
        {
            yield return espera;
            FOVCheck();
        }
    }

    void FOVCheck()
    {
        playerInSight = false;

        if (GerenciadorEstadoJogador.Instancia != null &&
            GerenciadorEstadoJogador.Instancia.EstaEscondido())
            return;

        Collider[] alvos = Physics.OverlapSphere(eyePosition.position, viewRadius, playerLayer);

        foreach (var alvo in alvos)
        {
            Vector3 origem = eyePosition.position;
            Vector3 destino = alvo.bounds.center;      // centro do player, não os pés
            Vector3 dir = destino - origem;

            // Plano lateral (Y,Z): ignora a profundidade (X)
            Vector3 dirPlano = new Vector3(0f, dir.y, dir.z).normalized;
            Vector3 frente = new Vector3(0f, 0f, Mathf.Sign(corpo.forward.z));

            if (Vector3.Angle(frente, dirPlano) > viewAngle / 2f) continue;

            float dist = dir.magnitude;
            if (Physics.Raycast(origem, dir.normalized, dist, obstacleLayer)) continue;

            playerInSight = true;
            return;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform olho = eyePosition != null ? eyePosition : transform;
        MovimentoInimigo mov = GetComponentInParent<MovimentoInimigo>();
        Transform c = mov != null ? mov.transform : transform;

        Vector3 frente = new Vector3(0f, 0f, Mathf.Sign(c.forward.z));
        Vector3 a = Quaternion.AngleAxis(-viewAngle / 2f, Vector3.right) * frente;
        Vector3 b = Quaternion.AngleAxis( viewAngle / 2f, Vector3.right) * frente;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(olho.position, viewRadius);
        Gizmos.color = playerInSight ? Color.red : Color.blue;
        Gizmos.DrawLine(olho.position, olho.position + a * viewRadius);
        Gizmos.DrawLine(olho.position, olho.position + b * viewRadius);
        Gizmos.DrawLine(olho.position, olho.position + frente * viewRadius);
    }
#endif
}