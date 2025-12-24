using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PotentialPlot : MonoBehaviour
{
    public float scaleX = 5f;   // Escala horizontal
    public float scaleY = 1f;    // Escala vertical
    public int numPoints = 500;   // Número de pontos
    public float nB = 1f;
    public float gamma = 0.25f;
    public float amp = 0.01f; 
    public float beta = 1f;
    public float C = 1f;
    public float D = 0.05f; // Coeficiente de difusão
    public float h = 0.1f; // passo de tempo
    public int Quantidade = 1;
    private LineRenderer line;

    private int step = 0;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = numPoints;
        line.useWorldSpace = true;
        line.widthMultiplier = 0.05f;
        line.material.color = Color.orange;
        DrawPotential();
    }

    // Potencial conforme seu código Python
    float Potential(float x)
    {
        return -1.0f * amp * (Mathf.Sin(4f * Mathf.PI * x) + beta*Mathf.Sin(8f * Mathf.PI * x));
    }
    float ExternalForce(int step, float B)
    {
        return B * Mathf.Sin(2f * Mathf.PI / 5f * step * h);
    }

    public void DrawPotential()
    {
        for (int i = 0; i < numPoints; i++)
        {
            float xVal = (float)i / (numPoints - 1); // intervalo [0,1]
            float yVal = Potential(xVal);

            // Converte para coordenadas no mundo 2D (ajuste à sua cena)
            Vector2 pos = new Vector2(xVal * scaleX - scaleX , yVal * scaleY);
            line.SetPosition(i, pos);
        }
    }

    // Se mudar parâmetros no runtime
    void Update()
    {
        // Atualiza o gráfico se A ou gamma forem alterados em tempo real
        step++;
        ExternalForce(step, nB);
        DrawPotential();
    }
}


