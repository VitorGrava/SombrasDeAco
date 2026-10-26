using UnityEngine;

// Fica no modelo animado do player (EliasKael), junto com o Animator.
// Roda depois do Animator (LateUpdate) e corrige duas coisas do FBX:
//  1. O CrouchWalk quase não dobra as pernas: aqui dobramos coxa/canela e inclinamos o tronco
//  2. As animações não têm altura de quadril: descemos o esqueleto até o pé encostar no chão
[RequireComponent(typeof(Animator))]
public class AjustePoseModelo : MonoBehaviour
{
    [Header("Agachar")]
    [Tooltip("Quanto a coxa dobra pra frente (graus). A canela dobra o dobro pra trás.")]
    public float anguloJoelho = 40f;
    [Tooltip("Inclinação do tronco pra frente quando agachado (graus)")]
    public float inclinacaoTronco = 25f;
    public float velocidadeTransicao = 8f;

    [Header("Pés no chão")]
    public bool manterPesNoChao = true;

    [Header("Ossos (preenchidos sozinhos pelo nome)")]
    public Transform raizEsqueleto; // metarig
    public Transform coxaE, coxaD, canelaE, canelaD, tronco;
    public Transform peE, peD;      // ponta do pé (foot.X_end)

    private Animator animator;
    private float peso;
    private Vector3 posicaoRaizOriginal;
    private float alturaPeOriginal;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (raizEsqueleto == null) raizEsqueleto = transform.Find("metarig");
        if (coxaE == null) coxaE = Procurar("thigh.L");
        if (coxaD == null) coxaD = Procurar("thigh.R");
        if (canelaE == null) canelaE = Procurar("shin.L");
        if (canelaD == null) canelaD = Procurar("shin.R");
        if (tronco == null) tronco = Procurar("spine.002");
        if (peE == null) peE = Procurar("foot.L_end");
        if (peD == null) peD = Procurar("foot.R_end");

        if (raizEsqueleto == null || coxaE == null || coxaD == null || canelaE == null || canelaD == null || peE == null || peD == null)
        {
            Debug.LogWarning($"{name}: ossos do esqueleto não encontrados, AjustePoseModelo desligado.", this);
            enabled = false;
            return;
        }

        // Na pose de repouso o pé já está no chão: guarda essa altura como referência
        posicaoRaizOriginal = raizEsqueleto.localPosition;
        alturaPeOriginal = Mathf.Min(peE.position.y, peD.position.y) - transform.position.y;
    }

    void LateUpdate()
    {
        bool agachado = animator.GetBool("IsCrouching");
        bool noChao = animator.GetBool("IsGrounded");

        peso = Mathf.MoveTowards(peso, agachado && noChao ? 1f : 0f, velocidadeTransicao * Time.deltaTime);

        // O modelo olha para -Z local: girar em +X local leva o joelho pra frente
        Vector3 eixo = transform.right;

        if (peso > 0f)
        {
            float a = anguloJoelho * peso;
            Dobrar(coxaE, eixo, a);
            Dobrar(coxaD, eixo, a);
            Dobrar(canelaE, eixo, -2f * a);
            Dobrar(canelaD, eixo, -2f * a);
            if (tronco != null) Dobrar(tronco, eixo, -inclinacaoTronco * peso);
        }

        raizEsqueleto.localPosition = posicaoRaizOriginal;
        if (manterPesNoChao && noChao)
        {
            float alturaPe = Mathf.Min(peE.position.y, peD.position.y) - transform.position.y;
            raizEsqueleto.position += Vector3.down * (alturaPe - alturaPeOriginal);
        }
    }

    private static void Dobrar(Transform osso, Vector3 eixo, float graus)
    {
        osso.rotation = Quaternion.AngleAxis(graus, eixo) * osso.rotation;
    }

    private Transform Procurar(string nome)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == nome) return t;
        return null;
    }
}
