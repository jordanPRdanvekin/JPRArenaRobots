using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DentedPixel;

public class GameOverController : MonoBehaviour
{
    [Header("Referencias de UI")]
    public RectTransform gameOverTitle;
    public TextMeshProUGUI infoText;
    public RectTransform yesButton;
    public RectTransform noButton;

    [Header("Configuración de la Partida")]
    public string enemyTag = "enemigo";

    [Header("Configuración de Animación")]
    public float randomOffsetStrength = 800f;
    public float canvasScaleDuration = 0.7f;
    public float elementsMoveDuration = 1.2f;

    [Header("Configuración del Mensaje")]
    [TextArea(3, 5)]
    public string formatoMensaje = "Te han quedado {enemigos} enemigos. ¡No te rindas!\nHas sobrevivido durante {tiempo}.";

    private Vector2 originalTitlePosition;
    private Vector2 originalInfoTextPosition;
    private Vector2 originalYesButtonPosition;
    private Vector2 originalNoButtonPosition;

    void Awake()
    {
        if (gameOverTitle != null) originalTitlePosition = gameOverTitle.anchoredPosition;
        if (infoText != null) originalInfoTextPosition = infoText.rectTransform.anchoredPosition;
        if (yesButton != null) originalYesButtonPosition = yesButton.anchoredPosition;
        if (noButton != null) originalNoButtonPosition = noButton.anchoredPosition;
    }

    public void TriggerGameOverSequence(float survivalTime)
    {
        if (gameObject.activeInHierarchy) return;
        gameObject.SetActive(true);

        int enemigosRestantes = GameObject.FindGameObjectsWithTag(enemyTag).Length;
        string tiempoFormateado = FormatearTiempo(survivalTime);

        if (infoText != null)
        {
            string mensajeFinal = formatoMensaje.Replace("{enemigos}", enemigosRestantes.ToString());
            mensajeFinal = mensajeFinal.Replace("{tiempo}", tiempoFormateado);
            infoText.text = mensajeFinal;
        }

        SetupElementsForAnimation();
        PlayGameOverAnimation();
    }

    private void SetupElementsForAnimation()
    {
        if (gameOverTitle != null) gameOverTitle.anchoredPosition = originalTitlePosition + Random.insideUnitCircle.normalized * randomOffsetStrength;
        if (infoText != null) infoText.rectTransform.anchoredPosition = originalInfoTextPosition + Random.insideUnitCircle.normalized * randomOffsetStrength;
        if (yesButton != null) yesButton.anchoredPosition = originalYesButtonPosition + Random.insideUnitCircle.normalized * randomOffsetStrength;
        if (noButton != null) noButton.anchoredPosition = originalNoButtonPosition + Random.insideUnitCircle.normalized * randomOffsetStrength;
        transform.localScale = Vector3.zero;
    }

    private void PlayGameOverAnimation()
    {
        LeanTween.scale(gameObject, Vector3.one, canvasScaleDuration).setEaseOutExpo().setIgnoreTimeScale(true);
        if (gameOverTitle != null) LeanTween.move(gameOverTitle, originalTitlePosition, elementsMoveDuration).setEaseOutElastic().setIgnoreTimeScale(true);
        if (infoText != null) LeanTween.move(infoText.rectTransform, originalInfoTextPosition, elementsMoveDuration).setEaseOutElastic().setIgnoreTimeScale(true).setDelay(0.1f);
        if (yesButton != null) LeanTween.move(yesButton, originalYesButtonPosition, elementsMoveDuration).setEaseOutElastic().setIgnoreTimeScale(true).setDelay(0.2f);
        if (noButton != null) LeanTween.move(noButton, originalNoButtonPosition, elementsMoveDuration).setEaseOutElastic().setIgnoreTimeScale(true).setDelay(0.2f);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private string FormatearTiempo(float tiempoEnSegundos)
    {
        int minutos = (int)tiempoEnSegundos / 60;
        int segundos = (int)tiempoEnSegundos % 60;
        return (minutos > 0) ? $"{minutos}m {segundos}s" : $"{segundos}s";
    }
}
