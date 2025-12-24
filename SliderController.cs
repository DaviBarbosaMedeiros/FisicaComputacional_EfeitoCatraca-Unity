using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderController : MonoBehaviour
{
    [Header("Referências")]
    public PotentialPlot potentialplot; // arraste seu PotentialPlot aqui

    [Header("Sliders")]
    public Slider slider_beta;
    public Slider slider_amp;
    public Slider slider_C;
    public Slider slider_Quantidade; 
    public Slider slider_nB;   
    public Slider slider_D;    

    [Header("Value labels (TMP)")]
    public TMP_Text value_beta;
    public TMP_Text value_amp;
    public TMP_Text value_C;
    public TMP_Text value_Quantidade;
    public TMP_Text value_nB;
    public TMP_Text value_D; 

    void Start()
    {
        if (potentialplot == null) Debug.LogWarning("SliderController: potentialplot não atribuído.");

        // Inicializa textos com valores dos sliders
        if (slider_beta != null) SetBeta(slider_beta.value);
        if (slider_amp != null) SetAmp(slider_amp.value);
        if (slider_C != null) SetC(slider_C.value);
        if (slider_Quantidade != null) SetQuantidade(slider_Quantidade.value);
        if (slider_nB != null) SetNB(slider_nB.value); // <-- NOVO
        if (slider_D != null) SetD(slider_D.value); // <-- NOVO

        // Listeners
        if (slider_beta != null) slider_beta.onValueChanged.AddListener(SetBeta);
        if (slider_amp != null) slider_amp.onValueChanged.AddListener(SetAmp);
        if (slider_C != null) slider_C.onValueChanged.AddListener(SetC);
        if (slider_Quantidade != null) slider_Quantidade.onValueChanged.AddListener(SetQuantidade);
        if (slider_nB != null) slider_nB.onValueChanged.AddListener(SetNB); // <-- NOVO
        if (slider_D != null) slider_D.onValueChanged.AddListener(SetD); // <-- NOVO
    }

    // ---------- Setters ----------
    public void SetBeta(float v)
    {
        if (potentialplot != null) potentialplot.beta = v;
        if (value_beta != null) value_beta.text = v.ToString("0.00");
        if (potentialplot != null) potentialplot.DrawPotential();
    }

    public void SetAmp(float v)
    {
        if (potentialplot != null) potentialplot.amp = v;
        if (value_amp != null) value_amp.text = v.ToString("0.000");
    }

    public void SetC(float v)
    {
        if (potentialplot != null) potentialplot.C = v;
        if (value_C != null) value_C.text = v.ToString("0.000");
    }

    public void SetQuantidade(float v)
    {
        int q = Mathf.RoundToInt(v);
        if (potentialplot != null) potentialplot.Quantidade = q;
        if (value_Quantidade != null) value_Quantidade.text = q.ToString();
    }

    // ---------- NOVO: nB ----------
    public void SetNB(float v)
    {
        if (potentialplot != null) potentialplot.nB = v;
        if (value_nB != null) value_nB.text = v.ToString("0.00");
    }

    // ---------- NOVO: D ----------
    public void SetD(float v)
    {
        if (potentialplot != null) potentialplot.D = v;
        if (value_D != null) value_D.text = v.ToString("0.000");
    }
    void OnDestroy()
    {
        if (slider_beta != null) slider_beta.onValueChanged.RemoveListener(SetBeta);
        if (slider_amp != null) slider_amp.onValueChanged.RemoveListener(SetAmp);
        if (slider_C != null) slider_C.onValueChanged.RemoveListener(SetC);
        if (slider_Quantidade != null) slider_Quantidade.onValueChanged.RemoveListener(SetQuantidade);
        if (slider_nB != null) slider_nB.onValueChanged.RemoveListener(SetNB);
        if (slider_D != null) slider_D.onValueChanged.RemoveListener(SetD); 
    }
}
