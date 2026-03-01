using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
Bailecito de victoria cuando el jugador llega a la salida con la luna robada
Baile, va a las escaleras, lo abduce el ovni, desaparece, texto de victoria, reinicia la escena

VA PEGADO AL TRIGGER DE SALIDA DEL ESCENARIO
*/
public class ExitWinSequence : MonoBehaviour
{
    // REFERENCIAS DEL JUGADOR
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Rigidbody playerRb;

    // PUNTOS DE LA SECUENCIA 
    [Header("Puntos")]
    [SerializeField] private Transform stairsTarget; // arriba de escaleras
    [SerializeField] private Transform vanishPoint; // donde desaparece 
    [SerializeField] private Transform shipTarget; // dentro de la nave (ya invisible)

    // TEXTO QUE SALE EN PANTLALA AL GANAR 
    [Header("UI")]
    [SerializeField] private GameObject youWonText;  

    [Header("Animator")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string danceStateName = "Dance";

    [Header("Time")] // cuanto estamos con cada animacion
    [SerializeField] private float danceSeconds = 1f;
    [SerializeField] private float walkSeconds = 3.0f;
    [SerializeField] private float abductUpSeconds = 1.2f;
    [SerializeField] private float toVanishSeconds = 0.35f;
    [SerializeField] private float abductToShipSeconds = 1.0f;
    
    [Header("Abduction")]
    [SerializeField] private float abductUpHeight = 2.5f; // cuánto sube el jugador antes de desaparecer

    [Header("Restart")]
    [SerializeField] private bool freezeTimeOnWin = true;
    [SerializeField] private float restartDelaySeconds = 2.0f; // espera antes de reiniciar

    private bool running = false; // evita que la secuencia se ejecute varias veces

    private void Awake()
    {
        // si no se asignamos el player en el inspector, lo buscamos por tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // si tenemos jugador, intentamos coger sus componentes
        if (player != null)
        {
            if (playerMovement == null) playerMovement = player.GetComponent<PlayerMovement>();
            if (playerAnimator == null) playerAnimator = player.GetComponentInChildren<Animator>();
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody>();
        }
    }

    // cuando el jugador entra en el trigger de salida empezamos la secuencia
    private void OnTriggerEnter(Collider other)
    {
        if (running) return; // si ya está en ejecución, no repetir
        if (!other.transform.root.CompareTag("Player")) return; // solo el jugador 

        // solo permite ganar si tiene la luna
        if (GameManager.Instance == null || !GameManager.Instance.hasMoon) return;

        running = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        Debug.Log("WIN Inicio secuencia");
        GameManager.Instance.gameWon = true; // marcamos estado global (que hemos ganao)

        // desactivamos el control del jugador para que no se pueda mover
        if (playerMovement != null) playerMovement.enabled = false;

        // congelamos físicas por si acaso para que no caiga ni se mueva por inercia
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }

        // baile
        if (playerAnimator != null) // comprobamos que el jugador tenga Animator asignado
        {
            if (!string.IsNullOrEmpty(danceStateName))
                playerAnimator.Play(danceStateName, 0, 0f); // nombreEstado, capa, normalizedTime (el tiempo desde el inicio de la animacion)
        }

        yield return new WaitForSeconds(danceSeconds);

        // subir a escaleras
        // activa animación de correr 
        yield return MoveToPosition(stairsTarget.position, walkSeconds);
        SetSpeed(0f); // paramos la animación de movimiento al llegar

        // lo abduce el ovni
        // el jugador sube en el aire antes de desaparecer
        Vector3 upTarget = player.position + Vector3.up * abductUpHeight;
        yield return MoveToPosition(upTarget, abductUpSeconds);

        // el jugador se mueve al punto de desvanecimiento y se oculta
        if (vanishPoint != null)
            yield return MoveToPosition(vanishPoint.position, toVanishSeconds);
        HidePlayerVisual(); // desactivamos los renderers para que sea invisible
        yield return MoveToPosition(shipTarget.position, abductToShipSeconds);

        // textito de win
        if (youWonText != null)
            youWonText.SetActive(true);

        Debug.Log("¡HAS GANADO!");

        // congelar tiempo del juego
        if (freezeTimeOnWin)
            Time.timeScale = 0f;

        // esperar unos segundos (en tiempo real, en el juego esta congelado) antes de reiniciar
        yield return new WaitForSecondsRealtime(restartDelaySeconds);

        // restauramos el tiempo y reiniciamos la escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // cambia el parámetro speed del animator para controlar la animación de movimiento
    private void SetSpeed(float v)
    {
        if (playerAnimator != null && !string.IsNullOrEmpty(speedParam))
            playerAnimator.SetFloat(speedParam, v);
    }

    // desactiva todos los renderers del jugador para hacerlo invisible (y que parezca que esta dentro de la nave)
    private void HidePlayerVisual()
    {
        if (player == null) return;

        Renderer[] rends = player.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
            rends[i].enabled = false;
    }

    // mueve al jugador suavemente desde su posición actual hasta el target en un tiempo x
    // es una corrutina, o sea que se ejecuta a lo largo de varios frames en lugar de hacerlo todo de golpe
    private IEnumerator MoveToPosition(Vector3 target, float seconds)
    {
        if (player == null) yield break;

        Vector3 start = player.position; // guardamos la posición inicial para el lerp
        float t = 0f; // contador de tiempo que ha transcurrido
 
        while (t < seconds)
        {   
            // alpha va de 0 a 1 según cuánto tiempo ha pasado respecto al total
            float alpha = Mathf.Clamp01(t / seconds);
            // lerp es linear interpolation
            // calcula la posición intermedia entre start y target según alpha
            player.position = Vector3.Lerp(start, target, alpha);

            // calculamos la dirección hacia el target para rotar al jugador
            Vector3 dir = target - player.position;
            dir.y = 0f; // ignoramos el eje y para que no se incline hacia arriba o abajo
            // solo rotamos si la dirección es suficientemente grande para que no ande haciendo giros raros
            if (dir.sqrMagnitude > 0.001f)
                // slerp rota suavemente hacia la dirección de movimiento frame a frame
                player.forward = Vector3.Slerp(player.forward, dir.normalized, 10f * Time.deltaTime);

            t += Time.deltaTime;
            yield return null;
        }

        player.position = target; // aseguramos que el jugador llegue exactamente al destino
    }
}
