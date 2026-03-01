using System.Collections;
using UnityEngine;

public class SearchingState : State
{
    private Vector3 lastKnownPosition; // Última posición conocida del jugador
    private Coroutine lookCoroutine; // Corrutina que controla el movimiento de mirar a los lados
    private const float arrivedThreshold = 1.2f;  // Distancia extra para considerar que el agente ha llegado al destino

    // Parámetros que definen el comportamiento de búsqueda
    private const float searchRotationSpeed = 260f; 
    private const float searchLookAngle = 90f;   
    private const float searchPause = 0.20f;  

    // Constructor q recibe la última posición conocida desde el brain
    public SearchingState(Vector3 lastKnownPos)
    {
        lastKnownPosition = lastKnownPos;
    }

    public void Enter(AgentBrain brain)
    {
        lookCoroutine = null; // Nos aseguramos de que no haya ninguna corrutina de búsqueda anterior activa

        brain.Agent.isStopped = false;  // Reactivamos el movimiento del agente por si venía de un estado en el que estaba detenido
        brain.Agent.updateRotation = true; 
        brain.Agent.destination = lastKnownPosition; // Asignamos como destino la última posición conocida del jugador
    }

    // Se ejecuta cada frame mientras esté en Searching
    public void Update(AgentBrain brain)
    {
        if (lookCoroutine != null) return; // Si ya estamos mirando a los lados no hacemos nada más

        // Cuando el agente ha llegado al destino
        if (!brain.Agent.pathPending &&
            brain.Agent.remainingDistance <= brain.Agent.stoppingDistance + arrivedThreshold)
        {
            brain.Agent.isStopped = true; // Detenemos el movimiento
            brain.Agent.updateRotation = false; // Desactivamos la rotación automática del NavMesh
            lookCoroutine = brain.StartCoroutine(LookAroundRoutine(brain)); // Iniciamos la corrutina de mirar a los lados
        }
    }

    // Se ejecuta al salir del estado Searching
    public void Exit(AgentBrain brain)
    {
        // Si la corrutina estaba activa, la detenemos
        if (lookCoroutine != null)
        {
            brain.StopCoroutine(lookCoroutine);
            lookCoroutine = null;
        }

        // Restauramos comportamiento normal del agente
        brain.Agent.updateRotation = true;
        brain.Agent.isStopped = false;
    }

    private IEnumerator LookAroundRoutine(AgentBrain brain)
    {
        Quaternion baseRotation = brain.transform.rotation; // Guardamos la rotación inicial para volver al centro

        // Mirar a la derecha
        yield return RotateTo(brain, baseRotation * Quaternion.Euler(0f,  searchLookAngle, 0f), searchRotationSpeed);
        yield return new WaitForSeconds(searchPause);

        // Mirar a la izquierda
        yield return RotateTo(brain, baseRotation * Quaternion.Euler(0f, -searchLookAngle, 0f), searchRotationSpeed);
        yield return new WaitForSeconds(searchPause);

        yield return RotateTo(brain, baseRotation, searchRotationSpeed); // Volver al centro

        lookCoroutine = null;
        brain.OnReachedLastKnownPosition(); // Notificamos al cerebro que la búsqueda ha terminado
    }

    private IEnumerator RotateTo(AgentBrain brain, Quaternion target, float rotSpeed)
    {
        // mientras el angulo entre la rotacion actual y la deseada sea > 1
        while (Quaternion.Angle(brain.transform.rotation, target) > 1f)
        {
            // Movemos la rotación un pequeño paso proporcional al tiempo transcurrido
            brain.transform.rotation = Quaternion.RotateTowards(
                brain.transform.rotation,
                target,
                rotSpeed * Time.deltaTime
            );
            yield return null; // esperamos al siguiente frame 
        }
    }
}