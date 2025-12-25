using UnityEngine;

/// <summary>
/// Escala a seta em X de acordo com a força externa usada no integrador.
/// Atualiza apenas quando GlobalTime.step muda (sincronizado com partículas).
/// </summary>
public class ArrowResize : MonoBehaviour
{
    [Header("Força externa (visual)")]
    public float B = 1f;            // amplitude (se quiser, leia de potentialPlot.nB)
    public float periodo = 5f;      // período (mesma referência que o código da dinâmica)
    public float ganhoEscala = 1f;  // fator que converte |F| em escala X

    [Header("Referências (opcional)")]
    public PotentialPlot potentialPlot; // se preenchido, usaremos potentialPlot.nB como B

    private Vector3 escalaBase;
    private int lastProcessedStep = -1;

    void Start()
    {
        escalaBase = transform.localScale;
    }

    void Update()
    {
        int step = GlobalTime.step;
        if (step == lastProcessedStep) return; // já processado este step
        lastProcessedStep = step;

        float usedB = B;
        if (potentialPlot != null) usedB = potentialPlot.nB;

        float h = GlobalTime.h;
        float Fext = ExternalForce(step, h);

        float escalaX = Mathf.Abs(Fext);

        // Mantém y,z originais; aplica sinal em x
        // Usar Mathf.Abs em escalaX garante valor positivo antes de aplicar sinal
        transform.localScale = new Vector3(
            Mathf.Sign(Fext) * Mathf.Abs(escalaX),
            escalaBase.y,
            escalaBase.z
        );
    }

    float ExternalForce(int step, float h)
    {
        // mesma forma usada na dinâmica: B * sin(2π/5 * step * h)
        return  Mathf.Sin(2f * Mathf.PI / periodo * step * h);
    }
}
