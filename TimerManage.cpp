using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Tooltip("Passo de tempo (deve ser igual a potentialPlot.h se desejar consistência)")]
    public float h = 0.01f;

    [Tooltip("Use FixedUpdate para atualizar o passo (recomendado para integrações numéricas)")]
    public bool useFixedUpdate = true;

    [Header("Optional")]
    public PotentialPlot potentialPlot; // arraste se quiser propagar 'h' automaticamente

    void Start()
    {
        GlobalTime.h = h;
        GlobalTime.step = 0;

        if (potentialPlot != null)
        {
            potentialPlot.h = h;
            potentialPlot.DrawPotential();
        }
    }

    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            GlobalTime.step++;
        }
    }

    void Update()
    {
        if (!useFixedUpdate)
        {
            GlobalTime.step++;
        }
    }
}
