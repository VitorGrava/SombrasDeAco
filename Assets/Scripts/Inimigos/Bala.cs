using UnityEngine;
using UnityEngine.SceneManagement;

public class Bala : MonoBehaviour
{
    public Transform player;
    public LayerMask destroyBala;
    private Rigidbody rb;

    public float velocidade = 25f;
    public float tempoDeVida = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Buscar o player aqui também, não no Start!
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
        else
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }

        Destroy(gameObject, tempoDeVida); // não acumula balas perdidas
    }

    public void Atirar()
    {
        if (player == null)
        {
            Debug.LogWarning("Bala: nenhum objeto com a tag 'Player' na cena.", this);
            return;
        }

        // Mira no centro do corpo, não no pivô (pé), senão a bala bate no chão antes
        Collider c = player.GetComponentInChildren<Collider>();
        Vector3 alvo = c != null ? c.bounds.center : player.position;

        Vector3 direcao = (alvo - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direcao);
        rb.linearVelocity = direcao * velocidade;

        Debug.DrawRay(transform.position, direcao * 10f, Color.yellow, 1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            SceneManager.LoadScene("Perdeu");
        }

        if (((1 << other.gameObject.layer) & destroyBala) != 0)
        {
            Destroy(gameObject);
        }
    }
}
