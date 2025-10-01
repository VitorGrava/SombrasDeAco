using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class CampoDeVisao : MonoBehaviour
{
    public float raioVisao;
    [Range(0, 360)] 
    public float anguloVisao;
    
    public LayerMask layerMaskAlvo;
    public LayerMask layerMaskObstaculo;
    
    [HideInInspector]
    public List<Transform> alvosVisiveis = new List<Transform>();

    void Start()
    {
        StartCoroutine("FOVRoutine");
    }

    public bool AcharAlvosVisiveis()
    {   
        alvosVisiveis.Clear();
        Collider[] AlvosNaVisao = Physics.OverlapSphere(transform.position, raioVisao, layerMaskAlvo);
        for (int i = 0; i < AlvosNaVisao.Length; i++)
        {
            Transform Alvo = AlvosNaVisao[i].transform;
            Vector3 direcaoAlvo = (Alvo.position - transform.position).normalized;
            if (Vector3.Angle(transform.right, direcaoAlvo) < anguloVisao / 2)
            {
                float distanciaAlvo = Vector3.Distance(transform.position, Alvo.position);

                if (!Physics.Raycast(transform.position, direcaoAlvo, distanciaAlvo, layerMaskObstaculo))
                {
                    alvosVisiveis.Add(Alvo);
                    return true;
                }
            }
        }
        return false;
    }
    
     private IEnumerator FOVRoutine()
     {
         float delay = 0.2f;
         WaitForSeconds wait = new WaitForSeconds(delay);

         while (true)
         {
             yield return wait;
             AcharAlvosVisiveis();
         }
     }

    public Vector3 DirecaoDoAngulo(float AnguloEmGraus, bool AnguloEGlobal)
    {   
        
        if (!AnguloEGlobal)
        {
            AnguloEmGraus += transform.eulerAngles.z;
        }

        return new Vector3(Mathf.Cos(AnguloEmGraus * Mathf.Deg2Rad), Mathf.Sin(AnguloEmGraus * Mathf.Deg2Rad), 0);
    }
}