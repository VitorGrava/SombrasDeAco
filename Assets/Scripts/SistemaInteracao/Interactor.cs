using System;
using UnityEngine;

interface IInteractable
{
    public void Interact();
}
public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    public float InteractRange;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {   
            Vector3 direcao = InteractorSource.right;
            if 
            Ray r = new Ray(InteractorSource.position, direcao);
            if (Physics.Raycast(r, out RaycastHit hitInfo, InteractRange))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                {
                    interactObj.Interact();
                }
            }
        }
    }
    
}    
