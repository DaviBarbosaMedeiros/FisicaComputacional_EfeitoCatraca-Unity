using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Gerencia criação das partículas, cálculo de velocidades (instantânea e média temporal)
/// e interface com a UI.
/// Também contém a classe ParticleMotion (anexa dinamicamente ao prefab BOla).
/// </summary>
public class CreateParticle : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI velocidadeMediaText;            // v_med(t) instantânea
    public TextMeshProUGUI velocidadeMediaTemporalText;    // v_med_total (média temporal)
    public TextMeshProUGUI sentidoVelocidadeText;          // texto do sentido

    [Header("Referências")]
    public PotentialPlot potentialplot;
    public GameObject BOla; // prefab da partícula

    private List<GameObject> bolas = new List<GameObject>();

    // ---------- observáveis ----------
    public float velocidadeMedia = 0f;            // instantânea (⟨v⟩(t))
    public float velocidadeMediaTemporal = 0f;    // média temporal (⟨v⟩_T)

    // alias público em inglês (compatibilidade com outros scripts)
    public float meanVelocityTotal => velocidadeMediaTemporal;
    // também expõe com nome exato se algum script espera 'meanVelocityTotal' como campo (ler apenas)
    public float meanVelocityTotal_alias { get { return velocidadeMediaTemporal; } }

    // ---------- acumuladores ----------
    private double somaVelocidades = 0.0; // double para reduzir acumulação de erro
    private int stepsContados = 0;
    private int lastProcessedStep = -1;

    // ----------------- API pública -----------------
    public void ResetParticles()
    {
        foreach (GameObject g in bolas)
            Destroy(g);

        bolas.Clear();

        ResetVelocityAverages();
    }

    void Update()
    {
        if (potentialplot == null)
            return; // evita NRE

        int Quantidade = potentialplot.Quantidade;

        // criar partículas até alcançar Quantidade
        while (bolas.Count < Quantidade)
        {
            float xinitial = Random.Range(0f, 1f);
            float y_initial = -1f * potentialplot.amp *
                (Mathf.Sin(4f * Mathf.PI * xinitial) +
                 potentialplot.beta * Mathf.Sin(8f * Mathf.PI * xinitial));

            Vector2 pos = new Vector2(
                xinitial * potentialplot.scaleX - potentialplot.scaleX,
                y_initial * potentialplot.scaleY
            );

            GameObject nova = Instantiate(BOla, pos, Quaternion.identity);
            ParticleMotion pm = nova.AddComponent<ParticleMotion>();

            pm.potentialplot = potentialplot;
            pm.myIndex = bolas.Count;
            pm.SetInitialPosition(xinitial);

            bolas.Add(nova);
        }

        // destruir partículas extras
        while (bolas.Count > Quantidade)
        {
            int last = bolas.Count - 1;
            Destroy(bolas[last]);
            bolas.RemoveAt(last);
        }

        // atualizar referências de allParticles
        List<ParticleMotion> motions = new List<ParticleMotion>();
        foreach (GameObject b in bolas)
            motions.Add(b.GetComponent<ParticleMotion>());

        foreach (ParticleMotion pm in motions)
            pm.allParticles = motions;

        // ---------- cálculo das médias ----------
        ComputeVelocities();
        UpdateUI();
    }

    // ================= VELOCIDADES =================

    // calcula velocity instantânea e acumula média temporal (1 cálculo por GlobalTime.step)
    void ComputeVelocities()
    {
        int step = GlobalTime.step;
        if (step == lastProcessedStep) return;
        lastProcessedStep = step;

        velocidadeMedia = ComputeAverageVelocityInstant();

        somaVelocidades += velocidadeMedia;
        stepsContados++;

        velocidadeMediaTemporal = (float)(somaVelocidades / stepsContados);
    }

    // média instantânea sobre partículas no instante atual
    float ComputeAverageVelocityInstant()
    {
        if (bolas.Count == 0)
            return 0f;

        float soma = 0f;
        foreach (GameObject b in bolas)
        {
            ParticleMotion pm = b.GetComponent<ParticleMotion>();
            soma += pm.GetVelocity();
        }

        return soma / bolas.Count;
    }

    // ================= UI =================

    void UpdateUI()
    {
        // Evitar caracteres Unicode que podem não existir na fonte TMP usada
        if (velocidadeMediaText != null)
            velocidadeMediaText.text = velocidadeMedia.ToString("F5") + " m/s";

        if (sentidoVelocidadeText != null)
            sentidoVelocidadeText.text = GetVelocityDirection(velocidadeMediaTemporal);
    }

    string GetVelocityDirection(float v)
    {
        const float eps = 1e-6f; // menor limiar para detectar transporte
        if (v > eps) return "Sentido: Direita (+x)";
        if (v < -eps) return "Sentido: Esquerda (-x)";
        return "Sentido: Sem transporte";
    }

    // ================= RESET =================

    public void ResetVelocityAverages()
    {
        somaVelocidades = 0.0;
        stepsContados = 0;
        velocidadeMedia = 0f;
        velocidadeMediaTemporal = 0f;
        lastProcessedStep = -1;
    }
}


// --------------------- ParticleMotion ---------------------
// classe responsável pela dinâmica de cada partícula.
// Atualizações importantes:
// - mantém posição reduzida x (0..1) usada para forças/potencial
// - mantém posição desenrolada X (unwrapped) para cálculo de velocidade física contínua
public class ParticleMotion : MonoBehaviour
{
    public PotentialPlot potentialplot;
    public List<ParticleMotion> allParticles;
    public int myIndex;

    // posições reduzidas (buffer duplo)
    private float[] x = new float[2];

    // posições desenroladas (sem BC) correspondentes ao mesmo buffer
    private float[] X = new float[2];

    public float D = 0.05f;

    // Para garantir que cada step seja processado apenas uma vez
    private int lastProcessedStep = -1;

    // ---------- inicialização ----------
    public void SetInitialPosition(float x0)
    {
        x[0] = x0;
        x[1] = x0;

        X[0] = x0;
        X[1] = x0;
    }

    // retorna velocidade usando posições desenroladas (X) para evitar saltos artificiais
    public float GetVelocity()
    {
        int step = GlobalTime.step;
        if (step < 1) return 0f;

        int i = step % 2;
        int prev = (i + 1) % 2;

        float h = potentialplot.h;

        return (X[i] - X[prev]) / h;
    }

    void Update()
    {
        if (potentialplot == null) return;

        int step = GlobalTime.step;
        if (step == lastProcessedStep) return; // já processado este step
        lastProcessedStep = step;

        // ler parâmetros do potentialplot
        float beta = potentialplot.beta;
        float gamma = potentialplot.gamma;
        float amp = potentialplot.amp;
        float nB = potentialplot.nB;
        float h = potentialplot.h;
        float Cte = potentialplot.C;
        float Dlocal = potentialplot.D; // evitar sombra com D da classe

        float A = 2f * Mathf.PI * amp / gamma;
        float B = nB / gamma;

        int i = step % 2;

        // faça o passo de Runge-Kutta passando o step atual
        RungeKuttaStep(i, A, B, beta, h, Cte, gamma, Dlocal, step);

        // x atualizado está em (i+1)%2
        float x_now = x[(i + 1) % 2];

        Vector2 pos = new Vector2(
            x_now * potentialplot.scaleX - potentialplot.scaleX,
            -1f * amp * (Mathf.Sin(4f * Mathf.PI * x_now) +
                   beta * Mathf.Sin(8f * Mathf.PI * x_now)) * potentialplot.scaleY
        );

        transform.position = pos;
    }

    // InteractionForce usa o instante 'step' para ler todos no mesmo buffer
    float InteractionForce(int step, float xj, float Cte)
    {
        float Fs = 0f;

        if (allParticles == null) return 0f;

        int idx = step % 2; // instante sincronizado para todas as partículas

        foreach (ParticleMotion other in allParticles)
        {
            if (other.myIndex == this.myIndex)
                continue;

            float xk = other.x[idx];

            float dx = xj - xk;

            if (Mathf.Abs(dx) > 0.01f)
            {
                Fs += Cte * Mathf.PI / Mathf.Tan(Mathf.PI * dx);
            }
            else if (dx > 0)
            {
                Fs += Cte * Mathf.PI / Mathf.Tan(Mathf.PI * 0.01f);
            }
            else
            {
                Fs += Cte * Mathf.PI / Mathf.Tan(-Mathf.PI * 0.01f);
            }
        }

        return Fs;
    }

    float ExternalForce(int step, float Bval, float h)
    {
        return Bval * Mathf.Sin(2f * Mathf.PI / 5f * step * h);
    }

    // Runge-Kutta que atualiza tanto x (com BC) quanto X (desenrolada)
    void RungeKuttaStep(int i, float A, float B, float beta, float h, float Cte, float gamma, float D, int step)
    {
        float xi = x[i];

        float Fx(float xval) =>
            A * (Mathf.Cos(4f * Mathf.PI * xval) + 2f * beta * Mathf.Cos(8f * Mathf.PI * xval))
            + InteractionForce(step, xval, Cte)
            + ExternalForce(step, B, GlobalTime.h);

        float k1 = Fx(xi);
        float k2 = Fx(xi + h * k1 * 0.5f);
        float k3 = Fx(xi + h * k2 * 0.5f);
        float k4 = Fx(xi + h * k3);

        float deterministic = h * (k1 + 2f * k2 + 2f * k3 + k4) / 6f;

        float stochastic = D * Mathf.Sqrt(h) / gamma * RandomGaussian();

        float xn = xi + deterministic + stochastic;

        // aplica condição periódica para x (reduzido)
        float xWrapped = ApplyPeriodicBC(xn);
        x[(i + 1) % 2] = xWrapped;

        // --- Atualiza X (posição desenrolada) para evitar saltos artificiais ---
        // dx reduzido entre novo e antigo (no espaço 0..1)
        float dxReduced = xWrapped - x[i];

        // corrigir salto de fronteira: se |dxReduced| > 0.5 assume-se atravessou a fronteira oposta
        if (dxReduced > 0.5f)
            dxReduced -= 1f;
        else if (dxReduced < -0.5f)
            dxReduced += 1f;

        // acumula em X (posição desenrolada)
        X[(i + 1) % 2] = X[i] + dxReduced;
    }

    float RandomGaussian()
    {
        float u1 = 1f - Random.value;
        float u2 = 1f - Random.value;
        return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
    }

    float ApplyPeriodicBC(float v)
    {
        if (v > 1f) return v - Mathf.FloorToInt(v);
        if (v < 0f) return v - Mathf.FloorToInt(v);
        return v;
    }
}
