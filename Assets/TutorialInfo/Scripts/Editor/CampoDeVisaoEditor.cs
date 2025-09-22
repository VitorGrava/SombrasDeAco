using UnityEditor;
using UnityEngine;
    
    [CustomEditor(typeof(CampoDeVisao))]
    public class CampoDeVisaoEditor : Editor
    {
        private void OnSceneGUI()
        {
            CampoDeVisao cdv = (CampoDeVisao)target;
            Handles.color = Color.white;
            Handles.DrawWireArc(cdv.transform.position, Vector3.forward, Vector3.up, 360, cdv.raioVisao);
            
            Vector3 AnguloDeVisaoA = cdv.DirecaoDoAngulo(-cdv.anguloVisao/2, false);
            Vector3 AnguloDeVisaoB = cdv.DirecaoDoAngulo(cdv.anguloVisao/2, false);
            
            Handles.DrawLine(cdv.transform.position, cdv.transform.position + AnguloDeVisaoA * cdv.raioVisao);
            Handles.DrawLine(cdv.transform.position, cdv.transform.position + AnguloDeVisaoB * cdv.raioVisao);

            Handles.color = Color.red;
            foreach (Transform alvoVisivel in cdv.alvosVisiveis) 
            {
                Handles.DrawLine(cdv.transform.position, alvoVisivel.position);
            }
        }

        
    }
