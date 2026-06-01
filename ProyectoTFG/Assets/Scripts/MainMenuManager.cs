using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    [Tooltip("Asigna aquí el Panel de Ajustes desde el Inspector")]
    public GameObject panelAjustes;
    
    [Tooltip("Asigna aquí el Panel de Puzles desde el Inspector")]
    public GameObject panelPuzles;

    [Header("UI Partida Guardada")]
    [Tooltip("Asigna aquí el botón de Continuar para poder desactivarlo si no hay partida guardada")]
    public Button botonContinuar;

    [Header("Controles")]
    [Tooltip("Asigna aquí el Scrollbar que controla el volumen")]
    public Scrollbar scrollbarSonido;

    void Start()
    {
        // Asegurarnos de que los paneles estén desactivados al iniciar la escena
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (panelPuzles != null) panelPuzles.SetActive(false);

        // Inicializar el volumen si el scrollbar está asignado
        if (scrollbarSonido != null)
        {
            // Cargar el volumen guardado o ponerlo al máximo (1f) por defecto
            float volumenGuardado = PlayerPrefs.GetFloat("VolumenAudio", 1f);
            scrollbarSonido.value = volumenGuardado;
            AudioListener.volume = volumenGuardado;
            
            // Añadimos el listener para que cada vez que se mueva el scrollbar, se llame al método CambiarVolumen
            scrollbarSonido.onValueChanged.AddListener(CambiarVolumen);
        }

        // Desactivar el botón Continuar si no hay partida guardada
        if (botonContinuar != null)
        {
            botonContinuar.interactable = SaveManager.HasSavedGame();
        }
    }

    // =========== MÉTODOS PARA LOS BOTONES PRINCIPALES ===========

    // Función a asignar al "Botón de jugar"
    public void Jugar()
    {
        // Empezamos una partida nueva
        PlayerPrefs.SetInt("CargarPartidaGuardada", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Función a asignar al "Botón de continuar"
    public void ContinuarPartida()
    {
        if (SaveManager.HasSavedGame())
        {
            PlayerPrefs.SetInt("CargarPartidaGuardada", 1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Función a asignar al "Botón de ajustes"
    public void AbrirPanelAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(true);
    }

    // Función a asignar al "Botón de volver" (dentro de ajustes o puzles)
    public void CerrarPanelAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(false);
    }

    // Función a asignar al "Botón de puzles"
    public void AbrirPanelPuzles()
    {
        if (panelPuzles != null) panelPuzles.SetActive(true);
    }

    public void CerrarPanelPuzles()
    {
        if (panelPuzles != null) panelPuzles.SetActive(false);
    }

    // Función a asignar al "Botón de salir"
    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // =========== MÉTODOS PARA FUNCIONES ESPECÍFICAS ===========

    // Función para el Scrollbar del sonido
    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        // Guardamos el volumen para que se mantenga al cambiar de escena o reiniciar el juego
        PlayerPrefs.SetFloat("VolumenAudio", valor);
        PlayerPrefs.Save();
    }

    // Función a asignar a los botones individuales de cada puzle
    // Si pasas el nombre de la escena como parámetro en el OnClick() del botón
    public void CargarPuzleEspecifico(string nombreEscenaPuzle)
    {
        SceneManager.LoadScene(nombreEscenaPuzle);
    }
}
