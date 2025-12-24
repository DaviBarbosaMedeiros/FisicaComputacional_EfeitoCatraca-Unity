using UnityEngine;
using UnityEngine.UI;

public class UISliders : MonoBehaviour
{
    public PotentialPlot potentialPlot;

    // Arraste os sliders no Inspector
    public Slider betaSlider;
    public Slider ampSlider;
    public Slider quantidadeSlider;
    public Slider nBSlider;    // numerador de B
    public Slider CSlider;     // constante de repulsão
    public Slider DSlider;     // coeficiente de difusão

    void Start()
    {
        // Atualiza os valores no início
        OnBetaChange();
        OnAmpChange();
        OnQuantidadeChange();
        OnNBChange();
        OnCChange();
        OnDChange();
    }

    public void OnBetaChange()
    {
        potentialPlot.beta = betaSlider.value;
    }

    public void OnAmpChange()
    {
        potentialPlot.amp = ampSlider.value;
    }

    public void OnQuantidadeChange()
    {
        potentialPlot.Quantidade = Mathf.RoundToInt(quantidadeSlider.value);
    }

    public void OnNBChange()
    {
        potentialPlot.nB = nBSlider.value;
    }

    public void OnCChange()
    {
        potentialPlot.C = CSlider.value;
    }
    public void OnDChange()
    {
        potentialPlot.D = DSlider.value;
    }
}