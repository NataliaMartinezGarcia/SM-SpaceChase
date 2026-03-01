using System.Collections;
using UnityEngine;

public class PatrollingState : State
{
    private int currentPatrolIndex; // indice del punto de patrulla al que se dirige actualmente
    private bool isWaiting; // Controla si el guardia está pausado en un punto
    private Coroutine waitCoroutine; // Referencia a la corrutina de espera para poder cancelarla

    public void Enter(AgentBrain brain)
    {
        isWaiting = false; // no empezamos esperando
        brain.Agent.isStopped = false; // aseguramos que el NavMeshAgent pueda moverse
        GoToClosestPatrolPoint(brain); // vamos al punto más cercano para empezar 
    }

    public void Update(AgentBrain brain)
    {

        if (isWaiting) return; // si estamos esperando en un punto, no hacemos nada más en este frame

        // si el guardia ya ha terminado de calcular su ruta y está lo suficientemente 
        // cerca del destino para considerar que ha llegado
        if (!brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.stopAtDistance)
        {
            // Iniciamos la secuencia de espera antes de ir al siguiente punto
            waitCoroutine = brain.StartCoroutine(WaitAtPatrolPoint(brain));
        }
    }

    public void Exit(AgentBrain brain)
    {
        // Si el guardia ve al jugador mientras estaba esperando en un punto debemos 
        // parar la corrutina de espera para que no intente volver a patrullar luego.
        if (waitCoroutine != null)
        {
            brain.StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        isWaiting = false; // Limpiamos el estado de espera
    }
    
    // Corrutina para pausar al guardia en cada punto
    private IEnumerator WaitAtPatrolPoint(AgentBrain brain)
    {
        isWaiting = true; // bloqueamos lo que hace el update 
        brain.Agent.isStopped = true; // detenemos al agente 

        // Esperamos un tiempos (configurado en el inspector del cerebro)
        yield return new WaitForSeconds(brain.patrolWaitingTime);

        // volvemos a patrullar 
        isWaiting = false; 
        brain.Agent.isStopped = false;
        GoToNextPatrolPoint(brain);
    }

    private void GoToNextPatrolPoint(AgentBrain brain)
    {
        // Asignamos la posición del punto actual al NavMeshAgent
        brain.Agent.destination = brain.patrolPoints[currentPatrolIndex].position;

        // incrementamos el índice 
        currentPatrolIndex++; 
        // Si el índice llega a ser igual al total de puntos, vuelve a cero
        if (currentPatrolIndex >= brain.patrolPoints.Length) 
        {
            currentPatrolIndex = 0;
        }
    }

    private void GoToClosestPatrolPoint(AgentBrain brain)
    {
        int closestIndex = 0; // variables de búsqueda (el más cercano es el primero) 
        float closestDistance = float.MaxValue; // establecemos la distancia más corta como el valor max posible 

        for (int i = 0; i < brain.patrolPoints.Length; i++) // recorremos todos los puntos de patrulla 
        {
            // calculamos la distancia entre el guardia y el punto
            float dist = Vector3.Distance(brain.transform.position, brain.patrolPoints[i].position);
            // Si el punto es más cerca que el anterior guardado, acutalizamos
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        brain.Agent.destination = brain.patrolPoints[currentPatrolIndex].position;
    }
}