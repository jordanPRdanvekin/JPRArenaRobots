using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Logo con efecto de Latido Suave")]
    public RectTransform logo;                 // Referencia al logo principal
    public float beatScale = 1.05f;            // Escala m�xima del efecto de latido
    public float beatDuration = 1.5f;          // Duraci�n del latido

    [Header("Panel del Submen� de Opciones")]
    public RectTransform optionsPanel;         // Panel que contiene el submen�
    public Vector3 hiddenPosition = new Vector3(0, 614.2499f, 0);  // Posici�n fuera de vista
    public Vector3 visiblePosition = new Vector3(0, 23.62506f, 0); // Posici�n visible
    public float optionsTweenDuration = 1f;    // Duraci�n de la animaci�n de desplazamiento

    [Header("Botones del Men�")]
    public Button startButton;     // Bot�n para empezar el juego
    public Button optionsButton;   // Bot�n para abrir el submen� de opciones
    public Button backButton;      // Bot�n para volver del submen� al men� principal
    public Button exitButton;      // Bot�n para salir del juego

    private void Start()
    {
        // Inicia el efecto de latido en el logo
        if (logo != null)
        {
            StartBeatLoop();
        }

        // Asignaci�n de funciones a los botones
        if (startButton != null)
            startButton.onClick.AddListener(LoadStartScene);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptionsMenu);

        if (backButton != null)
            backButton.onClick.AddListener(CloseOptionsMenu);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // Asegura que el submen� de opciones est� oculto al iniciar
        if (optionsPanel != null)
            optionsPanel.localPosition = hiddenPosition;
    }

    // Efecto de latido constante en el logo
    private void StartBeatLoop()
    {
        LeanTween.scale(logo.gameObject, Vector3.one * beatScale, beatDuration)
                 .setEase(LeanTweenType.easeInOutSine)
                 .setLoopPingPong();
    }

    // Carga la escena principal del juego
    public void LoadStartScene()
    {
        SceneManager.LoadScene("laberinto");
    }

    // Abre el submen� de opciones con animaci�n de rebote
    public void OpenOptionsMenu()
    {
        if (optionsPanel != null)
        {
            LeanTween.moveLocal(optionsPanel.gameObject, visiblePosition, optionsTweenDuration)
                     .setEase(LeanTweenType.easeOutBounce);
        }
    }

    // Cierra el submen� y lo devuelve a su posici�n original
    public void CloseOptionsMenu()
    {
        if (optionsPanel != null)
        {
            LeanTween.moveLocal(optionsPanel.gameObject, hiddenPosition, optionsTweenDuration)
                     .setEase(LeanTweenType.easeInBack);
        }
    }

    // Sale del juego (tambi�n funciona en el editor de Unity)
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
