using UnityEngine;

// Porta onde o player se esconde (E entra / E sai).
// Fases com o lado da tela em Z e profundidade em X: o objeto fica no vão da porta
// (X = fundo da porta, Z = centro dela) e o player é puxado pra dentro, de frente pra câmera.
// Enquanto escondido o PlayerController para e os inimigos ignoram o player
// (todos consultam GerenciadorEstadoJogador.EstaEscondido()).
public class PortaEsconderijo : Interactor
{
    [Header("Esconderijo")]
    [Tooltip("Rotação Y do player escondido (90 = de frente pra câmera, que olha pra -X)")]
    public float rotacaoEscondidoY = 90f;

    private PlayerController player;
    private Rigidbody rb;
    private Animator animator;

    private bool escondido;
    private float xAntes;
    private Quaternion rotacaoAntes;
    private bool kinematicAntes;

    protected override void Start()
    {
        // Área de interação: cobre do vão da porta até a linha onde o player anda (+X)
        // X = profundidade, Y = altura, Z = largura da porta
        SetTamanhoColisor(
            tamanho: new Vector3(2.6f, 2.5f, 1.2f),
            offset: new Vector3(0.9f, 0.25f, 0f)
        );

        base.Start();

        player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning($"{name}: nenhum PlayerController na cena, porta desligada.", this);
            enabled = false;
            return;
        }
        rb = player.GetComponent<Rigidbody>();
        animator = player.GetComponentInChildren<Animator>();
    }

    protected override void Interagir()
    {
        if (!escondido)
            Esconder();
        else
            SairEsconderijo();
    }

    private void Esconder()
    {
        // Outra porta já está com o player escondido
        if (GerenciadorEstadoJogador.Instancia != null && GerenciadorEstadoJogador.Instancia.EstaEscondido())
            return;

        // Só entra com o pé no chão (evita "grudar" na porta no meio do pulo)
        if (!player.noChao)
            return;

        xAntes = rb.position.x;
        rotacaoAntes = rb.rotation;

        // O chão do cenário acaba na fachada: no vão da porta não tem collider.
        // Kinematic segura o player parado ali (sem cair e sem ser empurrado).
        rb.linearVelocity = Vector3.zero;
        kinematicAntes = rb.isKinematic;
        rb.isKinematic = true;
        Vector3 pos = new Vector3(transform.position.x, rb.position.y, transform.position.z);
        Quaternion rot = Quaternion.Euler(0f, rotacaoEscondidoY, 0f);
        rb.position = pos;
        rb.rotation = rot;
        player.transform.SetPositionAndRotation(pos, rot);

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsCrouching", false);
        }

        escondido = true;
        if (GerenciadorEstadoJogador.Instancia != null)
            GerenciadorEstadoJogador.Instancia.SetEscondido(true);
        else
            Debug.LogWarning($"{name}: GerenciadorEstadoJogador não existe na cena, os inimigos não vão saber que o player se escondeu.", this);

        Debug.Log("Player se escondeu!");
    }

    private void SairEsconderijo()
    {
        Vector3 pos = new Vector3(xAntes, rb.position.y, transform.position.z);
        rb.position = pos;
        rb.rotation = rotacaoAntes;
        player.transform.SetPositionAndRotation(pos, rotacaoAntes);
        rb.isKinematic = kinematicAntes;
        rb.linearVelocity = Vector3.zero;

        escondido = false;
        if (GerenciadorEstadoJogador.Instancia != null)
            GerenciadorEstadoJogador.Instancia.SetEscondido(false);

        Debug.Log("Player saiu do esconderijo!");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = escondido ? Color.green : new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(0.3f, 2f, 0.8f));
    }
#endif
}
