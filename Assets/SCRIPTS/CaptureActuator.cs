using System.Collections;
using UnityEngine;
using UnityEngine.UI; // para los textos por pantalla
using UnityEngine.SceneManagement; // para reinicia el juego 

/*
Actuador que se encarga de gestionar lo que pasa cuando el jugador es atrapado
*/
public class CaptureActuator : MonoBehaviour
{
    private Transform player;
    private bool hasCapture = false; 
    [SerializeField] private GameObject gameOverText;

    public void SetTarget(Transform target)
    {
        player = target;
    }

    public void CapturePlayer()
    {
        // Si ya capturó, no hacer nada
        if (hasCapture) return;
        
        hasCapture = true; // Marcamos que la captura ha ocurrido
        // Si no hacemos esto sigue saliendo el mensaje de atrapado hasta que se reinicie el juego
        // No hay problema en que se quede true para siempre porque al reinciar se vuelve a poner a false
        
        Debug.Log("ATRAPADO!");
        gameOverText.SetActive(true);
        
        // Detenemos el movimiento del guardia para que no siga caminando tras capturarnos
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        Time.timeScale = 0f; // Congelamos el tiempo del juego
        
        // Iniciamos una corrutina para esperar un momento antes de reiniciar
        StartCoroutine(RestartLevel());
    }

    // Corrutina que gestiona el reinicio de la partida
    private IEnumerator RestartLevel()
    {
        yield return new WaitForSecondsRealtime(1f); // Esperamos 1 segundo 
        Time.timeScale = 1f;
        // Recargamos la escena actual desde el principio
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}