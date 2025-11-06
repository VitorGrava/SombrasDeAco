using Unity.VisualScripting;
using UnityEngine;

public class MovimentoPlayer : MonoBehaviour
{
   // Velocidade padrão ao andar
   public float velocidadeAndando;

   // Velocidade ao agachar (escondida no Inspector pois é calculada em tempo real)
   [HideInInspector]
   public float velocidadeAgachando;
   
   // Velocidade ao correr (também calculada em tempo real)
   [HideInInspector]
   public float velocidadeCorrendo;
   
   // Velocidade em que o personagem rotaciona ao mudar de direção
   public float velocidadeRotacao;
   
   // Força aplicada no pulo
   public float forcaPulo;
   
   // Indica se o player está encostando no chão (controle feito pela colisão)
   public bool noChao;
   
   public enum EstadoMovimento
   {
       Andando,
       Agachando,
       Correndo
   }
   private EstadoMovimento estadoAtual;
   // Rotação alvo no eixo Y para onde o personagem deve olhar
   private float RotYLoc = 90f;

   // Input capturado no eixo horizontal (A/D ou seta → / ←)
   private float moveX;

   // Controle do nível de stamina (gasta ao correr e recupera andando)
   private float stamina = 100f; 
   private float staminaMax = 100f;
   private int taxaConsumo = 20;
   private float taxaRecuperacao = 10f;
   
   
   public float tempoDelayRecuperacao = 5f; // segundos
   private float tempoRecuperacao = 0f;
   private bool podeRecuperar = false;


   
   // Referências aos componentes Unity
   private Animator animator; // Controla as animações do personagem
   private Rigidbody rb;      // Usado para a física e movimentação real do player
   private Vector3 posicaoAtual; // Direção atual do movimento

   void Start()
   {
        // Obtém automaticamente os componentes do objeto
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
   }

   // FixedUpdate é a função ideal para movimentação via física (Rigidbody)
   void FixedUpdate()
   {
       MovimentacaoPlayer();
       AtualizarEstadoMovimento();
       DelayRecuperacao();         
       ControleStamina();          
       Pulo();
       Debug.Log(stamina);
      
   }

   // Função responsável por todo o movimento horizontal do player
   void MovimentacaoPlayer()
   {   
        // Captura valor -1 a 1 do movimento horizontal
        moveX = Input.GetAxis("Horizontal");

        // Cria vetor de movimento baseado no input
        posicaoAtual = new Vector3(moveX, 0f, 0f);

        // Calcula as velocidades dinâmicas com base na velocidade padrão
        velocidadeCorrendo = velocidadeAndando + 5;
        velocidadeAgachando = velocidadeAndando * 0.3f;

        // Define tipo de movimento baseado no input de teclas
        switch (estadoAtual)
        {
            case EstadoMovimento.Andando:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeAndando * Time.fixedDeltaTime);
                animator.SetBool("IsCrouching", false);
                break;
            case EstadoMovimento.Agachando:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeAgachando * Time.fixedDeltaTime);
                animator.SetBool("IsCrouching", true);
                break;
            case EstadoMovimento.Correndo:
                rb.MovePosition(rb.position + posicaoAtual * velocidadeCorrendo * Time.fixedDeltaTime);
                animator.SetBool("IsCrouching", false);
                break;
        }

        // Rotaciona o personagem para a direção do movimento
        Rotacao(moveX);

        // Controla animação de "andar/correr"
        animator.SetBool("IsMoving", moveX != 0);
   }

   void AtualizarEstadoMovimento()
   {
       if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
       {
           if (stamina > 0)
           {
               estadoAtual = EstadoMovimento.Correndo;
               return;
           }
       }

       if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
       {
           estadoAtual = EstadoMovimento.Agachando;
           return;
       }

       estadoAtual = EstadoMovimento.Andando;
   }
   
   // Função de salto
   void Pulo()
   {
        // Só pula se apertar "Jump" e estiver no chão
        if (Input.GetButtonDown("Jump") && noChao == true)
        {
            rb.AddForce(new Vector3(0f, forcaPulo, 0f), ForceMode.Impulse);
        }
   }

   // Detecta que o player está pisando em um objeto com tag "Pulavel"
   private void OnCollisionStay(Collision collision)
   {
        if (collision.gameObject.tag == "Pulavel")
        {
            noChao = true;
        }
   }

   // Detecta que o player saiu do chão
   private void OnCollisionExit(Collision collision)
   {
        if (collision.gameObject.tag == "Pulavel")
        {
            noChao = false;
        }
   }

   // Gira o personagem conforme a direção em que ele está se movendo
   private void Rotacao(float moveX)
   {
        if (moveX > 0) RotYLoc = 90f;     // Direita
        else if (moveX < 0) RotYLoc = 270f; // Esquerda

        // Rotação suave (Lerp) para não girar instantaneamente
        float yAtual = transform.eulerAngles.y;
        float yNovo = Mathf.LerpAngle(yAtual, RotYLoc, velocidadeRotacao * Time.deltaTime);
        
        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
   }

   private void ControleStamina()
   {
       if (estadoAtual == EstadoMovimento.Correndo)
       {
           stamina -= taxaConsumo * Time.deltaTime;
           stamina = Mathf.Clamp(stamina, 0f, staminaMax);
           if (stamina  < 1)
           {
               stamina = 0;
           }
       }
       else if (podeRecuperar)
       {
           stamina += taxaRecuperacao * Time.deltaTime;
           stamina = Mathf.Clamp(stamina, 0f, staminaMax);
       }

   }

   void DelayRecuperacao()
   {
       if (estadoAtual == EstadoMovimento.Correndo)
       {
           tempoRecuperacao = 0f;
           podeRecuperar = false;
       }
       else
       {
            tempoRecuperacao += Time.deltaTime;
            if (tempoRecuperacao >= tempoDelayRecuperacao)
            {
                podeRecuperar = true;
            }
       }
   }

}

   

