using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TMP_Text timerText;
    private float currentTime = 0f;

    public CreateParticle createParticle;

    void Update()
    {
        currentTime += Time.deltaTime;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {

        if (timerText == null)
        {
            Debug.LogError("");
            return;
        }

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        int centes = Mathf.FloorToInt((currentTime * 100f) % 100f);

        timerText.text =
            minutes.ToString("00") + ":" +
            seconds.ToString("00") + ":" +
            centes.ToString("00");
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateTimerUI();
    }

    public void ResetSimulation()
    {
        ResetTimer();
        createParticle.ResetParticles();
    }

    [ContextMenu("Reset Timer (Inspector)")]
    private void ResetTimerContextMenu()
    {
        ResetSimulation();
    }
}
