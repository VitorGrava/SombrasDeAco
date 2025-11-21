using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; 
public class ControleStamina : MonoBehaviour
{
    [Header("Paramentros da Stamina")]
    public float stamina = 100f;
    [SerializeField]private float staminaMax = 100f;
    [SerializeField]public bool estaCorrendo = false;

    [Header("Parametos de Recuperação de Stamina")] 
    [Range(0, 50)][SerializeField]private float taxaConsumo = 0.5f; 
    [Range(0, 50)][SerializeField]private float taxaRegeneracao = 0.5f;

    [Header("Elementos da UI Stamina")]
    [SerializeField]private Image progressoStaminaUI = null;
    [SerializeField]private CanvasGroup sliderCanvasGroup = null;

    [SerializeField] private float delayRegeneracao = 2f;
    private bool podeRegenerar = true;

    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
       
    }

    private IEnumerator IniciarRegeneracao()
    {
        yield return new WaitForSeconds(delayRegeneracao);

        estaCorrendo = false;
        podeRegenerar = true;
    }
    private void Update()
    {
        if (!estaCorrendo && podeRegenerar)
        {
            if (stamina < staminaMax)
            {
                stamina += taxaRegeneracao * Time.deltaTime;
                stamina = Mathf.Clamp(stamina, 0, staminaMax); // ADICIONADO: impede stamina negativa/maior que o máximo

                UpdateStamina(1);

                if (stamina >= staminaMax)
                    sliderCanvasGroup.alpha = 0; // ALTERADO: simplificado
            }
        }
    }

    public void Correndo()
    {
        if (stamina > 0)
        {
            estaCorrendo = true;
            podeRegenerar = false;

            stamina -= taxaConsumo * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, staminaMax); // ADICIONADO: impede valores negativos

            UpdateStamina(1);
            
            StopCoroutine("IniciarRegeneracao");
            StartCoroutine("IniciarRegeneracao");
            
        }
    }

    void UpdateStamina(int value)
    {
        progressoStaminaUI.fillAmount = stamina / staminaMax;

        if (value == 0)
        {
            sliderCanvasGroup.alpha = 0;
        }
        else
        {
            sliderCanvasGroup.alpha = 1;
        }
    }


}
