using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CampoDeVisao : MonoBehaviour
{
    [Header("Configuração do Campo de Visão")] [Tooltip("Raio máximo de detecção do jogador")]
    public float viewRadius = 5f;

    [Tooltip("Ângulo de abertura do campo de visão (em graus)")] [Range(0, 360)]
    public float viewAngle = 120f;

    [Header("Camadas")] [Tooltip("Layer(s) identificando o jogador")]
    public LayerMask playerLayer;

    [Tooltip("Layer(s) de obstáculos (paredes, plataformas, etc)")]
    public LayerMask obstacleLayer;

    [Header("Referências")] [Tooltip("Transform do inimigo ou ponto central do campo de visão")]
    public Transform eyePosition;

    [Tooltip("Tempo entre as checagens de FOV (segundos)")]
    public float checkInterval = 0.2f;

    [Header("Estado")] public bool playerInSight;

    private Transform playerTransform;
    


    void Start()
    {
        if (eyePosition == null)
        {
            eyePosition = this.transform;
        }

        StartCoroutine(FOVCoroutine());
    }

    IEnumerator FOVCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            FOVCheck();
        }
    }

    void FOVCheck()
    {
        playerInSight = false;
        Collider[] players = Physics.OverlapSphere(eyePosition.position, viewRadius, playerLayer);

        foreach (var target in players)
        {
            playerTransform = target.transform;

            // Vetor direção do olho até o jogador
            Vector3 directionToPlayer = (playerTransform.position - eyePosition.position).normalized;

            // Dica: Corrija o plano se seu personagem só se move em X e Y (side scroller)
            directionToPlayer.z = 0; // Supondo jogo lateral, Z fixo.

            // Direção de frente do inimigo (ajuste para seu eixo principal)
            Vector3 fwd = eyePosition.right; // right = para a direita (comum em side scroller)

            float angle = Vector3.Angle(fwd, directionToPlayer);

            if (angle < viewAngle / 2f)
            {
                float distToPlayer = Vector3.Distance(eyePosition.position, playerTransform.position);

                // Raycast até o jogador para checar obstáculos
                if (!Physics.Raycast(eyePosition.position, directionToPlayer, distToPlayer, obstacleLayer))
                {
                    
                    // O jogador foi visto!
                    playerInSight = true;
                    Debug.Log(playerInSight);
                    // Aqui você pode chamar um evento, acionar ataque, emitir som, etc.
                }
            }
        }
    }

    // Visualização no Editor (Gizmos)
    private void OnDrawGizmosSelected()
    {
        if (eyePosition == null)
            eyePosition = this.transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition.position, viewRadius);

        Vector3 fwd = eyePosition.right; // Ajuste se precisar para seu eixo de visão

        Vector3 angleA = Quaternion.Euler(0, 0, -viewAngle / 2) * fwd;
        Vector3 angleB = Quaternion.Euler(0, 0, viewAngle / 2) * fwd;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(eyePosition.position, eyePosition.position + angleA * viewRadius);
        Gizmos.DrawLine(eyePosition.position, eyePosition.position + angleB * viewRadius);

        // Se detectar jogador, desenhe um raio até ele
        if (playerInSight && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePosition.position, playerTransform.position);
        }
    }

    
}
