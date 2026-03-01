using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class InvestigatingState : State
{
    private Vector3 soundOrigin;  // punto donde se escuchó al jugador
    private Vector3[] investigatePoints; // puntos aleatorios alrededor de ese origen
    private int currentPointIndex; // Índice del punto al que se dirige ahora mismo
    private bool isWaiting;
    private Coroutine waitCoroutine; //  para poder detener la espera si cambia de estado

    private const float arrivedThreshold = 0.5f;

    public InvestigatingState(Vector3 soundOrigin, float searchRadius, int pointCount)
    {
        this.soundOrigin = soundOrigin;
        // Genera la lista de puntos de inspección nada más crearse el estado
        investigatePoints = GeneratePoints(soundOrigin, searchRadius, pointCount);
    }

    public void Enter(AgentBrain brain)
    {
        isWaiting = false; // no empezamos parados
        currentPointIndex = 0; // empezamos la ruta en el primer punto del array 
        brain.Agent.isStopped = false; // Asegura que el NavMeshAgent pueda moverse
        GoToCurrentPoint(brain); // ir al origen del ruido 
    }

    public void Update(AgentBrain brain)
    {
        if (isWaiting) return; // si estamos mirando, no hacemos nada 

        // si llegamos al punto actual del recorrido 
        if (!brain.Agent.pathPending && brain.Agent.remainingDistance <= arrivedThreshold)
        {
            // iniciamos la corrutina para esperar y luego pasar al siguiente punto
            waitCoroutine = brain.StartCoroutine(WaitAndAdvance(brain));
        }
    }

    public void Exit(AgentBrain brain)
    {
        // detenemos la corrutina si vemos al jugador y salimos de este estado
        if (waitCoroutine != null) 
        {
            brain.StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        isWaiting = false;
    }

    private IEnumerator WaitAndAdvance(AgentBrain brain)
    {
        isWaiting = true;
        brain.Agent.isStopped = true;

        // esperamos el tiempo de investigación (se define en el cerebro) 
        yield return new WaitForSeconds(brain.investigateWaitTime);

        brain.Agent.isStopped = false;
        isWaiting = false;

        currentPointIndex++; // pasamos al siguiente punto 

        if (currentPointIndex >= investigatePoints.Length) // si ya hemos visitado todos los puntos generados
        {
            // Avisamos al cerebro que hemos terminado de buscar y no hemos encontrado nada
            brain.OnInvestigationComplete();
        }
        else
        {
            GoToCurrentPoint(brain); // si quedan puntos vamos al siguiente
        }
    }

    private void GoToCurrentPoint(AgentBrain brain)
    {
        // destino al punto actual 
        brain.Agent.destination = investigatePoints[currentPointIndex];
    }

    // Genera puntos aleatorios en el NavMesh alrededor del origen del sonido
    private Vector3[] GeneratePoints(Vector3 origin, float radius, int count)
    {
        Vector3[] points = new Vector3[count];

        // El punto 0 es el origen del sonido (con un margen de 1 unidad por si es inaccesible)
        points[0] = SampleNavMesh(origin, 1f);

        // El resto de puntos son aleatorios dentro del radio de búsqueda
        for (int i = 1; i < count; i++)
        {
            points[i] = SampleNavMesh(origin, radius);
        }
        return points;
    }

    // Busca punto válido dentro del NavMesh
    private Vector3 SampleNavMesh(Vector3 origin, float radius)
    {
        // Intentamos varias veces encontrar un punto válido en el NavMesh
        for (int attempt = 0; attempt < 10; attempt++)
        {
            // Elegimos una dirección aleatoria en un círculo alrededor del sonido
            Vector2 rand2D = Random.insideUnitCircle * radius;
            // Creamos la posición candidata
            Vector3 candidate = origin + new Vector3(rand2D.x, 0f, rand2D.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius, NavMesh.AllAreas))
                return hit.position;
        }
        // Si no encontramos nada válido, devolvemos el propio origen
        return origin;
    }
}