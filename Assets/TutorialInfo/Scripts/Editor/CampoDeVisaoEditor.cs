using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CampoDeVisao))]
public class CampoDeVisaoEditor : Editor
{
    private void OnSceneGUI()
    {
        CampoDeVisao cdv = (CampoDeVisao)target;

        // 🔹 Cor de preenchimento do cone
        Handles.color = new Color(1f, 1f, 1f, 0.1f); // branco transparente
        Vector3 direcaoCentro = cdv.DirecaoDoAngulo(0, false);
        Handles.DrawSolidArc(
            cdv.transform.position,          // centro
            Vector3.forward,                 // normal do plano (Z)
            direcaoCentro,                   // direção central
            cdv.anguloVisao / 2f,            // metade do ângulo para um lado
            cdv.raioVisao                    // raio
        );
        Handles.DrawSolidArc(
            cdv.transform.position,
            Vector3.forward,
            direcaoCentro,
            -cdv.anguloVisao / 2f,           // metade do ângulo para o outro lado
            cdv.raioVisao
        );

        // 🔹 Contorno do raio (círculo de alcance)
        Handles.color = Color.white;
        Handles.DrawWireArc(cdv.transform.position, Vector3.forward, Vector3.up, 360, cdv.raioVisao);

        // 🔹 Linhas limite do cone
        Vector3 anguloA = cdv.DirecaoDoAngulo(-cdv.anguloVisao / 2, false);
        Vector3 anguloB = cdv.DirecaoDoAngulo(cdv.anguloVisao / 2, false);

        Handles.DrawLine(cdv.transform.position, cdv.transform.position + anguloA * cdv.raioVisao);
        Handles.DrawLine(cdv.transform.position, cdv.transform.position + anguloB * cdv.raioVisao);

        // 🔹 Linhas até os alvos visíveis
        Handles.color = Color.red;
        foreach (Transform alvoVisivel in cdv.alvosVisiveis)
        {
            Handles.DrawLine(cdv.transform.position, alvoVisivel.position);
        }
    }
}