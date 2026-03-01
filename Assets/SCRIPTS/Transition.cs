using UnityEngine;
using System; // sin esto no va lo de func
/*
Clase que define las reglas para cambiar de un estado a otro
Cada transición guarda tres datos: From, When y To
El AgentBrain recorre una lista de estas transiciones en cada frame para decidir qué debe hacer el guardia
*/
public class Transition
    {
    public Type From {get;} // solo leer no cambiar
    // Func<bool>  no recibe nada y devuelve verdadero o falso (la condición)
    public Func<bool> When {get;}
    // Func<State>  devuelve el nuevo objeto estado al que queremos ir
    public Func<State> To {get;}

    // Constructor que usamos en el cerebro para crear cada regla de los agentes
    public Transition(Type from, Func<bool> when, Func<State> to)
    {
        From = from; // de que estado partimos
        When = when; // condicion para que se de la transicion
        To   = to; // a que estado vamos
    }

    public bool Matches(State currentState) => // ver si entra en la transicion
        currentState.GetType() == From // estamos en el estado de partida
        && When(); // se cumple la regla 
}