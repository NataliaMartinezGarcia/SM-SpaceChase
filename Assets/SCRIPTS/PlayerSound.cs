using UnityEngine;

/*
Va pegado al jugador y dice el radio en el que los agentes lo escuchan
Lo dibujamos en el editor para que sea más fácil ver cómo funcionan las cosas
*/

public class PlayerSound : MonoBehaviour
{   
    [SerializeField] private float soundRadius = 50f; // radio de sonido que emite el jugador

    // solo lectura para que el sensor de sonido pueda saber si escucha al jugador
    public float SoundRadius => soundRadius; 

    private void OnDrawGizmos()
    {   
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f); // blanco (por ejemplo luego ya lo cambiamos en el editor)
        Gizmos.DrawSphere(transform.position, soundRadius);
    }
}