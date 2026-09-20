using UnityEngine;
public class BossScript : MonoBehaviour
{
    public LayerMask camdachao;
    public float velocidadeMovimento =6f;
    public float distanciaPatrulha = 20f;
    private float posicaoXInicial;
    private Rigidbody rb;

    void Start()
    {
        posicaoXInicial = transform.position.x;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() // era Update
    {
        float deslocamentoX = Mathf.Sin(Time.time * velocidadeMovimento) * distanciaPatrulha;

        Vector3 novaPosicao = new Vector3(
            posicaoXInicial + deslocamentoX,
            rb.position.y, // Y controlado pela física
            rb.position.z
        );

        rb.MovePosition(novaPosicao); // respeita a física
    }
}