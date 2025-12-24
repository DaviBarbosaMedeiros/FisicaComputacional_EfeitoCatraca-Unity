using UnityEngine;
using System.Collections.Generic;
using TMPro;


public class CreateParticle : MonoBehaviour
{
    public TextMeshProUGUI velocidadeMediaText;

    public PotentialPlot potentialplot;
    public GameObject BOla;

    private List<GameObject> bolas = new List<GameObject>();

    public float velocidadeMedia = 0f;

    public void ResetParticles()
    {
        foreach (GameObject BOla in bolas)
            Destroy(BOla);

        bolas.Clear();      
    }

    void Update()
    {
        int Quantidade = potentialplot.Quantidade;

        while (bolas.Count < Quantidade)
        {
            float xinitial = Random.Range(0f, 1f);
            float y_initial = -1f * potentialplot.amp * (Mathf.Sin(4f * Mathf.PI * xinitial) +
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

            Debug.Log("Criada partícula #" + bolas.Count + " com x=" + xinitial);
        }

        while (bolas.Count > Quantidade)
        {
            int last = bolas.Count - 1;
            Destroy(bolas[last]);
            bolas.RemoveAt(last);
        }

        List<ParticleMotion> motions = new List<ParticleMotion>();
        foreach (GameObject b in bolas)
            motions.Add(b.GetComponent<ParticleMotion>());

        foreach (ParticleMotion pm in motions)
            pm.allParticles = motions;

        velocidadeMedia = ComputeAverageVelocity();

        velocidadeMediaText.text = velocidadeMedia.ToString("F3") + " m/s";

        Debug.Log("Velocidade média do sistema = " + velocidadeMedia);

    }

    public float ComputeAverageVelocity()
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
}

/* ===============================================================
 *               SCRIPT DA PARTÍCULA COM RUÍDO ESTOCÁSTICO
 * =============================================================== */

public class ParticleMotion : MonoBehaviour
{
    public PotentialPlot potentialplot;
    public List<ParticleMotion> allParticles;
    public int myIndex;

    private float[] x = new float[2];
    private int step = 0;

    public float D = 0.05f;

    public void SetInitialPosition(float x0)
    {
        x[0] = x0;
        x[1] = x0;
    }

    public float GetVelocity()
    {
        if (step < 2) return 0f;

        int i = step % 2;
        int prev = (i + 1) % 2;

        float h = potentialplot.h;

        return (x[i] - x[prev]) / h;
    }

    void Update()
    {
        float beta   = potentialplot.beta;
        float gamma  = potentialplot.gamma;
        float amp    = potentialplot.amp;
        float nB     = potentialplot.nB;
        float h      = potentialplot.h;
        float Cte    = potentialplot.C;
        float D      = potentialplot.D;

        float A = 2f * Mathf.PI * amp / gamma;
        float B = nB / gamma;

        int i = step % 2;

        RungeKuttaStep(i, A, B, beta, h, Cte, gamma, D);

        step++;

        float x_now = x[step % 2];

        Vector2 pos = new Vector2(
            x_now * potentialplot.scaleX - potentialplot.scaleX,
            -1f * amp * (Mathf.Sin(4f * Mathf.PI * x_now) +
                   beta * Mathf.Sin(8f * Mathf.PI * x_now)) * potentialplot.scaleY
        );

        transform.position = pos;
    }

    // =====================================================================
    //                   CORREÇÃO AQUI ↓↓↓↓↓↓↓
    // =====================================================================
    float InteractionForce(float xj, float Cte)
    {
        float Fs = 0f;

        if (allParticles == null) return 0f;

        foreach (ParticleMotion other in allParticles)
        {
            if (other.myIndex == this.myIndex)
                continue;

            // *** CORREÇÃO: usar o step DA OUTRA PARTÍCULA ***
            int idx = other.step % 2;
            float xk = other.x[idx];
            // ---------------------------------------------------------------

            float dx = xj - xk;

            if (Mathf.Abs(dx) > 0.01f)
            {
                Fs += Cte * Mathf.PI * (1f / Mathf.Tan(Mathf.PI * dx));
            }
            else if (dx > 0)
            {
                Fs += Cte * Mathf.PI * (1f / Mathf.Tan(Mathf.PI * 0.01f));
            }
            else
            {
                Fs += Cte * Mathf.PI * (1f / Mathf.Tan(-Mathf.PI * 0.01f));
            }
        }

        return Fs;
    }
    // =====================================================================

    void RungeKuttaStep(int i, float A, float B, float beta, float h, float Cte, float gamma, float D)
    {
        float xi = x[i];

        float Fx(float xval) =>
            A * (Mathf.Cos(4f * Mathf.PI * xval) + 2f * beta * Mathf.Cos(8f * Mathf.PI * xval))
            + InteractionForce(xval, Cte)
            + B * Mathf.Sin(2f * Mathf.PI / 5f * step * h);

        float k1 = Fx(xi);
        float k2 = Fx(xi + h * k1 * 0.5f);
        float k3 = Fx(xi + h * k2 * 0.5f);
        float k4 = Fx(xi + h * k3);

        float deterministic = h * (k1 + 2f * k2 + 2f * k3 + k4) / 6f;

        float stochastic = D * Mathf.Sqrt(h) / gamma * RandomGaussian();

        float xn = xi + deterministic + stochastic;

        x[(i + 1) % 2] = ApplyPeriodicBC(xn);
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
