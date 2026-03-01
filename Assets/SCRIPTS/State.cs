using UnityEngine;
/*
Interfaz que define la estructura de cualquier comportamiento del guardia
*/
public interface State
{
    // Se ejecuta una sola vez justo cuando el guardia entra en este estado
    void Enter(AgentBrain brain);
    // Se ejecuta en cada frame mientras el estado esté activo 
    void Update(AgentBrain brain);
    // Se ejecuta una sola vez justo antes de que el guardia cambie a un estado diferente
    void Exit(AgentBrain brain);
}