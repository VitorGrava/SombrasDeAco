using UnityEngine;

public class MovimentoCamera : MonoBehaviour
{
    public Transform player;
    public Transform LimiteMapaEsquerda;
    public Transform LimiteMapaDireita;
    public float velocidadeSuavizada = 0.125f; // Velocidade de suavização do movimento da câmera
    public float CameraOffSet;

    // LateUpdate é chamado após o Update, ideal para seguir objetos que já se moveram
    void LateUpdate()
    {
        // Calcula a posição X desejada da câmera, limitada entre os dois extremos do mapa
        float posicaoXLimitada = Mathf.Clamp(player.position.x, LimiteMapaEsquerda.position.x, LimiteMapaDireita.position.x);

        // Cria a posição desejada da câmera com a posição limitada no eixo X
        Vector3 posicaoDesejada = new Vector3(posicaoXLimitada + CameraOffSet, transform.position.y, transform.position.z);
      
        // Atualiza a posição da câmera
        transform.position = posicaoDesejada;
    }
}

