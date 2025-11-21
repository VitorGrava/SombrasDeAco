using UnityEngine;
using UnityEngine.SceneManagement;
public class Bala : MonoBehaviour
{
    public Transform player;          // posição do player
    public LayerMask destroyBala;     // layer para destruir só a bala
    private Rigidbody rb;

    public float velocidade = 25f;

    void Start()
    {
        // Player é encontrado automaticamente
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Atirar();
    }

    public void Atirar()
    {
        if (player == null) return;

        // Direção calculada no momento do tiro
        Vector3 direcao = (player.position - transform.position).normalized;

        // Move a bala usando velocidade e física
        rb.velocity = direcao * velocidade;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Se acertar o jogador → destruir ambos
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject); // destrói player
            Destroy(gameObject);       // destrói bala
            SceneManager.LoadScene("Perdeu");
        }

        // Se acertar layer de destruição da bala
        if (((1 << other.gameObject.layer) & destroyBala) != 0)
        {
            Destroy(gameObject); // só a bala
        }
    }
}