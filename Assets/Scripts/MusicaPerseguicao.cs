using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicaPerseguicao : MonoBehaviour
{
    public static MusicaPerseguicao Instancia { get; private set; }

    [Header("Configuração")]
    [Range(0f, 1f)] public float volumeMaximo = 0.7f;
    public float velocidadeFadeIn = 2f;
    public float velocidadeFadeOut = 1f;
    [Tooltip("Segundos que a música continua após o último inimigo perder o player")]
    public float tempoParaParar = 3f;

    private AudioSource fonte;
    private readonly HashSet<Object> perseguidores = new HashSet<Object>();
    private float timerParar;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        fonte = GetComponent<AudioSource>();
        fonte.loop = true;
        fonte.playOnAwake = false;
        fonte.spatialBlend = 0f; // 2D: volume igual em qualquer lugar
        fonte.volume = 0f;
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public void RegistrarPerseguicao(Object inimigo) => perseguidores.Add(inimigo);
    public void RemoverPerseguicao(Object inimigo) => perseguidores.Remove(inimigo);

    void Update()
    {
        if (perseguidores.Count > 0)
        {
            timerParar = tempoParaParar;

            if (!fonte.isPlaying) fonte.Play();
            fonte.volume = Mathf.MoveTowards(fonte.volume, volumeMaximo, velocidadeFadeIn * Time.deltaTime);
        }
        else
        {
            timerParar -= Time.deltaTime;

            if (timerParar <= 0f)
            {
                fonte.volume = Mathf.MoveTowards(fonte.volume, 0f, velocidadeFadeOut * Time.deltaTime);

                if (fonte.volume <= 0f && fonte.isPlaying)
                    fonte.Stop();
            }
        }
    }
}