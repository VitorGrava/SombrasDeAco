using UnityEditor.Timeline.Actions;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    public LayerMask camdachao;
    
    public float velocidadeMoviumento = 5f;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * velocidadeMoviumento * Time.deltaTime;  
    }

    
}
