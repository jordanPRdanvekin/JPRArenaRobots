using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DentedPixel;

public class VictoryScreenController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform victoryText;
    public RectTransform restartButton;
    public TextMeshProUGUI timeDisplayText;

    [Header("Animation Settings")]
    public float randomOffsetStrength = 500f;
    public float canvasScaleDuration = 0.7f;
    public float elementsMoveDuration = 1.2f;

    private Vector2 originalTextPosition;
    private Vector2 originalButtonPosition;

    void Awake()
    {
        if (victoryText != null) originalTextPosition = victoryText.anchoredPosition;
        if (restartButton != null) originalButtonPosition = restartButton.anchoredPosition;
    }

    public void TriggerVictorySequence(float finalTime)
    {
        if (gameObject.activeInHierarchy) return;
        gameObject.SetActive(true);

        FormatTimeText(finalTime);
        SetupElementsForAnimation();
        PlayVictorySequence();
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void FormatTimeText(float finalTime)
    {
        if (timeDisplayText == null) return;

        int minutes = (int)finalTime / 60;
        int seconds = (int)finalTime % 60;
        string timeString = (minutes > 0) ? $"{minutes}m {seconds}s" : $"{seconds}s";
        timeDisplayText.text = $"Has tardado {timeString} en finalizar. ¡ENHORABUENA!";
    }

    private void SetupElementsForAnimation()
    {
        if (victoryText != null) victoryText.anchoredPosition = originalTextPosition + Random.insideUnitCircle.normalized * randomOffsetStrength;
        if (restartButton != null) restartButton.anchoredPosition = originalButtonPosition + Random.insideUnitCircle.normalized * randomOffsetStrength;
        transform.localScale = Vector3.zero;
    }

    private void PlayVictorySequence()
    {
        LeanTween.scale(gameObject, Vector3.one, canvasScaleDuration).setEaseOutExpo().setIgnoreTimeScale(true);

        if (victoryText != null)
        {
            LeanTween.move(victoryText, originalTextPosition, elementsMoveDuration).setEaseOutElastic().setIgnoreTimeScale(true);
        }

        if (restartButton != null)
        {
            LeanTween.move(restartButton, originalButtonPosition, elementsMoveDuration).setEaseOutElastic().setIgnoreTimeScale(true);
        }
    }
}
