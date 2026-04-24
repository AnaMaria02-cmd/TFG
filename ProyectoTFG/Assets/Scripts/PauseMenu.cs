using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    [Tooltip("Arrastra aquí el panel principal de pausa")]
    public GameObject pausePanel;
    
    [Tooltip("Arrastra aquí el panel de instrucciones")]
    public GameObject instructionsPanel;

    [Header("Configuración")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "MainMenu";

    // Opcional: Para controlar si el juego está pausado con la tecla Escape
    private bool isPaused = false;

    private void Start()
    {
        // Nos aseguramos de que los paneles empiecen desactivados al iniciar el juego
        if (pausePanel != null) pausePanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    private void Update()
    {
        // Opcional: Pausar o reanudar al pulsar la tecla Escape (o 'P')
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // ── MÉTODOS PARA LOS BOTONES ──

    // Asignar al botón de "Pausa" en el Canvas (si tienes uno)
    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        
        Time.timeScale = 0f; // Detener el tiempo
        isPaused = true;
        
        // Mostrar el ratón por si estaba oculto
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Asignar al botón "Continuar jugando"
    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        
        Time.timeScale = 1f; // Reanudar el tiempo
        isPaused = false;
    }

    // Asignar al botón "Instrucciones" del panel de pausa
    public void OpenInstructions()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    // Asignar al botón "Volver/Cerrar" dentro del panel de instrucciones
    public void CloseInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true); // Volver al panel de pausa
    }

    // Asignar al botón "Salir al menú"
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Asegurarse de que el tiempo fluye de nuevo
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
