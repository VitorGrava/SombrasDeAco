using UnityEngine;

public class MovimentoPlayer : MonoBehaviour
{
   public float velocidadeAndando;
   public float velocidadeAgachando;
   public float velocidadeRotacao;
   public float forcaPulo;
   public bool noChao;
   private Rigidbody rb;
   private float RotYLoc = 90f; //rotação alvo no eixo Y
   private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MovimentacaoPlayer();
        Pulo();
        
        
    }

    void MovimentacaoPlayer()
    {   
        float moveX = Input.GetAxis("Horizontal");
        Vector3 posicaoAtual = new Vector3(-moveX, 0f, 0f);
        velocidadeAgachando = velocidadeAndando * 0.3f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            rb.MovePosition(rb.position + posicaoAtual * velocidadeAgachando * Time.fixedDeltaTime);
            animator.SetBool("IsCrouching", true);
        }
        else
        {
            rb.MovePosition(rb.position + posicaoAtual * velocidadeAndando * Time.fixedDeltaTime);
            animator.SetBool("IsCrouching", false);
        }

        
        
        Rotacao(moveX);
        if (moveX!= 0)
        {
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
    }
    
    void Pulo()
    {
        if (Input.GetButtonDown("Jump") && noChao == true)
        {
            //Vector3 PosPulo = transform.position;
            rb.AddForce(new Vector3(0f, forcaPulo, 0f), ForceMode.Impulse);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Pulavel")
        {
            noChao = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Pulavel")
        {
            noChao = false;
        }
    }

    private void Rotacao(float moveX)
    {
        if (moveX > 0) RotYLoc = 90f;
        else if (moveX < 0) RotYLoc = 270f;

        float yAtual = transform.eulerAngles.y;
        float yNovo = Mathf.LerpAngle(yAtual, RotYLoc, velocidadeRotacao * Time.deltaTime);
        
        transform.eulerAngles = new Vector3(0f, yNovo, 0f);
    }

}
