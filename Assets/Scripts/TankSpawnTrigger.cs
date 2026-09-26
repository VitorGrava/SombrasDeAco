using UnityEngine;

/// <summary>
/// Coloque num objeto vazio com um Collider marcado como "Is Trigger".
/// Quando o Player entra, o tank nasce atrás dele (fora da câmera) e começa a persegui-lo.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TankSpawnTrigger : MonoBehaviour
{
    [Header("Tank")]
    [Tooltip("Prefab do tank (com Tank_movimentacao). Será instanciado.")]
    public GameObject tankPrefab;
    [Tooltip("Alternativa: um tank que já está na cena, desativado. Se preenchido, ele é ativado e teleportado em vez de instanciar.")]
    public GameObject tankNaCena;

    [Header("Posição do spawn")]
    [Tooltip("Direção em que o player avança nessa fase (ex.: (1,0,0) = pra direita). O tank nasce no lado oposto.")]
    public Vector3 direcaoDoAvanco = Vector3.right;
    [Tooltip("Distância mínima atrás do player. Aumenta sozinha até sair da tela.")]
    public float distanciaMinima = 15f;
    [Tooltip("Margem extra fora da tela (0.15 = 15% além da borda).")]
    [Range(0f, 0.5f)] public float margemForaDaCamera = 0.15f;
    [Tooltip("Layer(s) do chão, para o tank nascer em cima dele.")]
    public LayerMask camadaChao;
    [Tooltip("Ajuste vertical se o pivot do tank não estiver na base.")]
    public float offsetY = 0.5f;

    [Header("Player")]
    public string tagPlayer = "Player";

    private bool jaDisparou = false;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (jaDisparou) return;

        bool ehPlayer = other.CompareTag(tagPlayer) ||
                        (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(tagPlayer));
        if (!ehPlayer) return;

        jaDisparou = true;

        Transform player = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
        SpawnarTank(player);
    }

    private void SpawnarTank(Transform player)
    {
        Vector3 avanco = direcaoDoAvanco;
        avanco.y = 0f;
        if (avanco.sqrMagnitude < 0.001f) avanco = Vector3.right;
        avanco.Normalize();

        Vector3 dirAtras = -avanco;
        Camera cam = Camera.main;

        // Afasta até a posição ficar fora da tela
        float dist = distanciaMinima;
        Vector3 pos = CalcularPosicao(player, dirAtras, dist);

        int seguranca = 0;
        while (cam != null && EstaNaTela(cam, pos) && seguranca++ < 50)
        {
            dist += 2f;
            pos = CalcularPosicao(player, dirAtras, dist);
        }

        Quaternion rot = Quaternion.LookRotation(avanco, Vector3.up); // de frente pro player

        GameObject tank;
        if (tankNaCena != null)
        {
            tank = tankNaCena;
            tank.transform.SetPositionAndRotation(pos, rot);
            tank.SetActive(true);
        }
        else if (tankPrefab != null)
        {
            tank = Instantiate(tankPrefab, pos, rot);
        }
        else
        {
            Debug.LogWarning("[TankSpawnTrigger] Nenhum tankPrefab nem tankNaCena configurado.", this);
            return;
        }

        Tank_movimentacao mov = tank.GetComponent<Tank_movimentacao>();
        if (mov != null)
            mov.IniciarPerseguicao(player);
        else
            Debug.LogWarning("[TankSpawnTrigger] O tank não tem o script Tank_movimentacao.", tank);
    }

    private Vector3 CalcularPosicao(Transform player, Vector3 dirAtras, float dist)
    {
        Vector3 pos = player.position + dirAtras * dist;

        // Acha o chão embaixo do ponto (senão usa a altura do player)
        if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 200f, camadaChao))
            pos.y = hit.point.y + offsetY;
        else
            pos.y = player.position.y;

        return pos;
    }

    private bool EstaNaTela(Camera cam, Vector3 pos)
    {
        Vector3 v = cam.WorldToViewportPoint(pos);
        float m = margemForaDaCamera;
        return v.z > 0f && v.x > -m && v.x < 1f + m && v.y > -m && v.y < 1f + m;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 avanco = direcaoDoAvanco;
        avanco.y = 0f;
        if (avanco.sqrMagnitude < 0.001f) return;
        avanco.Normalize();

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, avanco * 5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position - avanco * distanciaMinima, 1f);
    }
#endif
}
