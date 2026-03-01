using UnityEngine;

public class FollowingState : State
{
    public void Enter(AgentBrain brain)
    {
        brain.Agent.isStopped = false; // reactivamos el movimiento del NavMeshAgent
        // Los contadores TimeInFollowingState y TimeSinceLostPlayer
        // los gestiona el cerebro, no hace falta resetearlos aquí
    }

    public void Update(AgentBrain brain)
    {
        // Aquí solo ejecutamos el comportamiento: seguir al jugador
        brain.Agent.destination = brain.Player.position;
        // El cerebro se encarga de contar el tiempo y decidir cuándo parar
    }

    public void Exit(AgentBrain brain) {}
}