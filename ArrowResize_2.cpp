using UnityEngine;

/// <summary>
/// Escala a seta em X de acordo com a velocidade média temporal do sistema.
/// Atualiza apenas quando GlobalTime.step muda (sincronizado com partículas).
/// </summary>
public class ArrowResizeVel : MonoBehaviour
{
    [Header("Escala visual")]
    public float ganhoEscala = 5f;   // converte |<v>| em tamanho da seta

    [Header("Referências")]
    public CreateParticle createParticle; // script que calcula <v>_total

    private Vector3 escalaBase;
    private int lastProcessedStep = -1;

    void Start()
    {
        escalaBase = transform.localScale;
    }

    void Update()
    {
        int step = GlobalTime.step;
        if (step == lastProcessedStep) return;
        lastProcessedStep = step;

        if (createParticle == null) return;

        float vMean = createParticle.velocidadeMediaTemporal;

        float escalaX = ganhoEscala * Mathf.Abs(vMean);

        // Evita seta invisível
        if (escalaX < 1e-4f)
            escalaX = 1e-4f;

        transform.localScale = new Vector3(
            Mathf.Sign(vMean),
            escalaBase.y,
            escalaBase.z
        );
    }
}
