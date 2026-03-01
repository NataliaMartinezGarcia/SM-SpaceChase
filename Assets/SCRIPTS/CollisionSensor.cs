using UnityEngine;
/*
Sensor de contacto físico (sistema de colisiones de Unity)
Detecta cuando el jugador toca al guardia

Utiliza OnCollisionEnter y OnCollisionExit para detectar el inicio y el fin del contacto.
Filtra las colisiones para asegurarse de que solo reacciona ante el Jugador
Notifica al AgentBrain mediante OnPlayerCollisionEnter() y OnPlayerCollisionExit().
*/
public class CollisionSensor : MonoBehaviour
{
    private Transform player;
    private AgentBrain brain;

    void Start()
    {
        brain = GetComponent<AgentBrain>();
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    // Evento de Unity que se dispara cuando algo choca físicamente con el colisionador de este objeto
    private void OnCollisionEnter(Collision collision)
    {
        // verificamos que el jugador está asignado y luego si el objeto con el que chocamos es el jugador
        if (player != null && collision.gameObject.transform == player)
        {
            brain.OnPlayerCollisionEnter(); // notificamos al cerebro, actualiza el hecho
        }
    }

    // Evento de Unity que se dispara cuando un objeto deja de estar en contacto físico con este colisionador
    private void OnCollisionExit(Collision collision)
    {
        // Verificamos si el objeto que se acaba de separar de nosotros es el jugador
        if (player != null && collision.gameObject.transform == player)
        {
            brain.OnPlayerCollisionExit(); // notificamos al cerebro, actualiza el hecho
        }
    }
}