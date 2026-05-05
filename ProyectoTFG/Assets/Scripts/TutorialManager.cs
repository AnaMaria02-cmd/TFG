using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Paneles del Tutorial")]
    [Tooltip("Arrastra aquí el Panel 1 (la primera parte de la historia)")]
    public GameObject panel1;
    
    [Tooltip("Arrastra aquí el Panel 2 (la segunda parte de la historia)")]
    public GameObject panel2;

    void Start()
    {
        // Iniciamos el tutorial con una pequeña pausa para asegurarnos 
        // de que otros scripts (como TimerAndShopManager) ya han terminado de cargar
        StartCoroutine(IniciarTutorialRutina());
    }

    private IEnumerator IniciarTutorialRutina()
    {
        // Esperamos un frame para que cualquier script que ponga Time.timeScale = 1 en Start() se ejecute primero
        yield return null;

        // Pausamos el tiempo para que el juego (cronómetro, físicas, etc.) no avance
        Time.timeScale = 0f;

        // Mostramos el Panel 1 y ocultamos el Panel 2
        if (panel1 != null) panel1.SetActive(true);
        if (panel2 != null) panel2.SetActive(false);
    }

    // Este método se debe asignar al evento OnClick del botón "Next" del Panel 1
    public void MostrarPanel2()
    {
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(true);
    }

    // Este método se debe asignar al evento OnClick del botón "Next" del Panel 2
    public void EmpezarJuego()
    {
        if (panel2 != null) panel2.SetActive(false);
        
        // Reanudamos el tiempo para que comience el juego
        Time.timeScale = 1f;
    }
}
