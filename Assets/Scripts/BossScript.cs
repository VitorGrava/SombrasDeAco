using UnityEngine;

public class BossScript : MonoBehaviour
{
    public LayerMask camdachao;

    public float velocidadeMovimento = 5f;
    public float distanciaPatrulha = 3f; // O quanto ele se afasta do centro

    private float posicaoXInicial;

    void Start()
    {
        // Salva a posição X de onde o Boss começou
        posicaoXInicial = transform.position.x;
    }

    void Update()
    {
        // O Seno cria um valor que vai de -1 a 1 repetidamente
        // Multiplicamos pela distanciaPatrulha para definir o alcance
        float deslocamentoX = Mathf.Sin(Time.time * velocidadeMovimento) * distanciaPatrulha;

        // Atualiza a posição apenas no eixo X, mantendo o Y e Z originais
        transform.position = new Vector3(posicaoXInicial + deslocamentoX, transform.position.y, transform.position.z);
    }
}