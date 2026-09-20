using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControleStamina : MonoBehaviour
{
    [Header("Parâmetros da Stamina")]
    public float stamina = 100f;
    [SerializeField] private float staminaMax = 100f;
    public bool estaCorrendo = false;

    [Header("Parâmetros de Recuperação de Stamina")]
    [Range(0, 50)][SerializeField] private float taxaConsumo = 0.5f;
    [Range(0, 50)][SerializeField] private float taxaRegeneracao = 0.5f;

    [Header("Elementos da UI Stamina")]
    [SerializeField] private Image progressoStaminaUI = null;
    [SerializeField] private CanvasGroup sliderCanvasGroup = null;

    [SerializeField] private float delayRegeneracao = 2f;

    // ← controla se a coroutine já está rodando
    private bool coroutineRodando = false;

    void Update()
    {
        if (!estaCorrendo)
            TentarRegenerar();
    }

    private void TentarRegenerar()
    {
        if (stamina >= staminaMax)
        {
            sliderCanvasGroup.alpha = 0;
            return;
        }

        // ← só regenera se a coroutine de delay já terminou
        if (coroutineRodando) return;

        stamina += taxaRegeneracao * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, staminaMax);
        AtualizarUI();
    }

    public void Correndo()
    {
        if (stamina <= 0) return;

        estaCorrendo = true;

        stamina -= taxaConsumo * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, staminaMax);
        AtualizarUI();

        // ← só inicia a coroutine se ela não estiver rodando
        if (!coroutineRodando)
            StartCoroutine(DelayRegeneracao());
    }

    private IEnumerator DelayRegeneracao()
    {
        coroutineRodando = true;

        // ← fica renovando o delay enquanto ainda estiver correndo
        while (estaCorrendo)
            yield return null;

        yield return new WaitForSeconds(delayRegeneracao);
        coroutineRodando = false;
        estaCorrendo = false;
    }

    private void AtualizarUI()
    {
        progressoStaminaUI.fillAmount = stamina / staminaMax;
        sliderCanvasGroup.alpha = 1;
    }
}