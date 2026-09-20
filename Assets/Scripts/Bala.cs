using UnityEngine;
using UnityEngine.SceneManagement;

public class Bala : MonoBehaviour
{
    public Transform player;
    public LayerMask destroyBala;
    private Rigidbody rb;

    public float velocidade = 25f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Buscar o player aqui também, não no Start!
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Atirar()
    {
        if (player == null) return;

        Vector3 direcao = (player.position - transform.position).normalized;
        rb.linearVelocity = direcao * velocidade;
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