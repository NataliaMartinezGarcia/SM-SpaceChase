using UnityEngine;
/*
Sensor de visión encargado de detectar al jugador (triple chequeo):
- Distancia: Comprueba si el jugador está dentro 
- Ángulo: Comprueba si el jugador está dentro del cono de visión 
- Obstáculos: Lanza un Raycast para verificar que no haya paredes o puertas bloqueando la vista

Si el jugador entra en el campo de visión, avisa al brain con OnPlayerSpotted().
Si el jugador sale o se esconde tras un muro, avisa al brain con nPlayerLost().

También hay representación visual del cono en el editor de Unity mediante Gizmos*/
public class VisionSensor : MonoBehaviour
{
    [Header("View settings")]
    [SerializeField] public float detectionRange = 20f; // Distancia máxima a la que el guardia puede ver
    [SerializeField] public float viewAngle = 90; // Apertura del cono de visión en grados
    [SerializeField] private LayerMask obstacleLayerMask; // Capas que bloquean la vista (paredes, puertas cerradas)

    [Header("Field of view visualization")]
    [SerializeField] private bool showVisionCone = true; // para ver el cono en el editor
    [SerializeField] private Color visionConeColor = new Color(1f, 1f, 0f, 0.2f); //  amarillo (reposo)
    [SerializeField] private Color detectedColor = new Color(1f, 0f, 0f, 0.3f); //  rojo (cuando detecta al jugador)
    [SerializeField] private int coneResolution = 30; // Cantidad de líneas para dibujar el arco del cono

    private Transform player; // Referencia a la posición del jugador
    private AgentBrain brain; // Referencia al cerebro del agente para mandarle datos

    private bool wasSeenLastFrame = false; // para saber si el jugador era visto en el frame anterior 

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
        bool canSee = CanSeePlayer(); // comprobamos si vemos al jugador

        // Solo notificamos cuando hay un CAMBIO de estado
        if (canSee && !wasSeenLastFrame) // Si antes lo veía y ahora no,  avisamos
        {
            brain.OnPlayerSpotted();
        }
        else if (!canSee && wasSeenLastFrame) // Si antes no lo veía y ahora sí, avisamos
        {
            brain.OnPlayerOutOfSight();
        }
        // Si no ha cambiado nada, no hacemos nada
        wasSeenLastFrame = canSee;  // Actualizamos el estado del frame anterior para la siguiente comparación
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false; // Si no hay jugador, no podemos ver nada

        // DIstancia entre el guardia y jugador
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); 

        // Si el jugador está más lejos del rango configurado, no se sigue calculando
        if (distanceToPlayer > detectionRange) return false; 
        // Si el juegador no está en el cono, no lo vemos
        if (!IsFacingPlayer()) return false;

        return HasClearPathToPlayer();
    }

    // Comprobación del cono de visión 
    private bool IsFacingPlayer()
    {
        // Calcula el vector dirección normalizado (longitud 1) hacia el jugador        
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        // Calcula el ángulo en grados entre el frente del guardia (forward) y la dirección al jugador
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        // Compara si el ángulo es menor o igual a la mitad del ángulo total (si el cono es 90, mira 45 a cada lado)
        return angle <= viewAngle / 2;
    }

    // Comprobación de colisión física (Raycast)
    private bool HasClearPathToPlayer()
    {
        // Vector dirección sin normalizar para obtener la magnitud 
        Vector3 dirToPlayer = player.position - transform.position;

        // Lanza un rayo desde la posición del guardia hacia el jugador
        // Se usa la magnitud de dirToPlayer para que el rayo no viaje infinitamente, solo hasta el jugador
        // obstacleLayerMask permite que el rayo solo choque con las paredes y puertas (obstáculos)
        if (Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, dirToPlayer.magnitude, obstacleLayerMask))
        {
            // Si el rayo choca con algo antes de llegar al jugador, hay un obstáculo
            return false;
        }
        // Si el rayo llega al final sin chocar con ningún obstauclo, el camino está despejado
        return true;
    }

    // Visualización en el editos de unity
    private void OnDrawGizmos()
    {
        if (!showVisionCone) return; // Si el checkbox en el inspector está apagado no dibujamos nada

        // Cambia el color del Gizmo (rojo - amarillo)
        Color currentColor;
        if (CanSeePlayer())
        {
            currentColor = detectedColor; 
        }
        else 
        {
            currentColor = visionConeColor;
        }
        Gizmos.color = currentColor;

        Vector3 startPos = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = viewAngle / 2f;
        Vector3 previousPoint = Vector3.zero;

        // Bucle para crear la malla de líneas que forman el cono
        for (int i = 0; i <= coneResolution; i++)
        {
            float angle = -halfAngle + (viewAngle * i / coneResolution); // calculamos el ángulo actual del cono 
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward; //Rota el vector forward según el ángulo calculado

            Vector3 targetPoint; // punto final del radio 
            // Lanza un rayo desde startPos en la dirección calculada y comprueba si choca con algo 
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, detectionRange, obstacleLayerMask))
                targetPoint = hit.point; // Si el rayo choca un obstáculo, el punto final será el punto exacto del impacto
            else
                targetPoint = startPos + direction * detectionRange; // Si no choca nada, el rayo llega hasta la distancia máxima del cono

            Gizmos.DrawLine(startPos, targetPoint); // dibujamos la  linea 
            if (i > 0) Gizmos.DrawLine(previousPoint, targetPoint); // Une el punto actual con el anterior
            previousPoint = targetPoint; // guardamos el punto actual para que en la siguiente iteración pueda conectarse
        }
    }

}