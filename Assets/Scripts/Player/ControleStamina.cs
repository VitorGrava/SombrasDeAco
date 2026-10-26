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

    // O Player é prefab e não pode guardar referência do Canvas da cena:
    // se os campos estiverem vazios, procura a barra do prefab Canvas pelo nome
    void Start()
    {
        if (sliderCanvasGroup == null)
        {
            GameObject grupo = GameObject.Find("BarraStamina_CanvasGrupo");
            if (grupo != null) sliderCanvasGroup = grupo.GetComponent<CanvasGroup>();
        }

        if (progressoStaminaUI == null)
        {
            GameObject barra = GameObject.Find("StaminaBar_FW");
            if (barra != null) progressoStaminaUI = barra.GetComponent<Image>();
        }

        if (sliderCanvasGroup == null || progressoStaminaUI == null)
            Debug.LogWarning($"{name}: barra de stamina não encontrada (coloque o prefab Canvas na cena).", this);
    }

    void Update()
    {
        if (!estaCorrendo)
            TentarRegenerar();
    }

    private void TentarRegenerar()
    {
        if (stamina >= staminaMax)
        {
            if (sliderCanvasGroup != null) sliderCanvasGroup.alpha = 0;
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

    // UI é opcional: sem ela a stamina continua funcionando.
    // (Uma exceção aqui interrompia o PlayerController no meio: o player não virava
    // e a animação de correr não era ligada enquanto corria.)
    private void AtualizarUI()
    {
        if (progressoStaminaUI != null) progressoStaminaUI.fillAmount = stamina / staminaMax;
        if (sliderCanvasGroup != null) sliderCanvasGroup.alpha = 1;
    }
}