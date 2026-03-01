using UnityEngine;

/*
Sensor para detectar al jugador si pasa cerca
Basicamente compara la distancia entre el agente y el jugador con la suma de los radios de cada uno
El del agente es hearingRadius y el del jugador es PlayerSound.SoundRadius (del archivo PlayerSound)

Si el jugador entra en rango, avisa al brain con OnPlayerHeard()
Si el jugador sale del rango, avisa con OnPlayerOutOfHearing()
*/
public class SoundSensor : MonoBehaviour
{
    [Header("Sound settings")]
    // aqui mejor valores cualquiera y ya lo cambiamos en el editor que es mas facil para la prueba y error
    [SerializeField] private float hearingRadius = 50f; // radio de escucha del agente

    [Header("Gizmos")]
    [SerializeField] private bool showHearingRadius = true; // casilla que podemos quitar en el editor pero mejor enseñarla
    [SerializeField] private Color hearingColor = new Color(0f, 0.5f, 1f, 0.2f);

    private Transform player;
    private AgentBrain brain;

    // guardamos si el jugador fue escuchado el frame anterior para detectar cambios
    private bool wasHeardLastFrame = false;

    void Start()
    {
        brain = GetComponent<AgentBrain>();
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    void Update()
    {
        bool canHear = CanHearPlayer();

        // solo avisamos al brain cuando hay un cambio de estado (entró o salió del rango)
        if (canHear && !wasHeardLastFrame) // Si el jugador entra en rango
            brain.OnPlayerHeard();
        else if (!canHear && wasHeardLastFrame) // Si el jugador sale del rango
            brain.OnPlayerOutOfHearing();

        wasHeardLastFrame = canHear;
    }

    public bool CanHearPlayer()
    {
        if (player == null) return false;

        // obtenemos el radio de emisión del jugador
        float playerSoundRadius = 0f;
        PlayerSound playerSound = player.GetComponent<PlayerSound>();
        if (playerSound != null)
            playerSoundRadius = playerSound.SoundRadius;

        // sumamos el radio de escucha del agente y el radio de sonido del jugador
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= hearingRadius + playerSoundRadius;
    }
    private void OnDrawGizmos()
    {   // para dibujar el radio
        if (!showHearingRadius) return;
        Gizmos.color = hearingColor;
        Gizmos.DrawSphere(transform.position, hearingRadius);
    }
}