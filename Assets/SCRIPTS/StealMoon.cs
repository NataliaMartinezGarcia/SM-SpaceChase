using UnityEngine;
/*
VA PEGADO AL JUGADOR

Cuando toca la luna, la recoge, se destruye y avisa al GameManager para que sepa que ya tiene la luna
*/

public class StealMoon : MonoBehaviour
{
    // private bool hasMoon = false;
    // public bool HasMoon() => hasMoon;

    // esto se ejecuta automáticamente cuando el jugador entra en el trigger de la luna 
    private void OnTriggerEnter(Collider other)
    {
        // solo reaccionamos si tocamos el objeto con tag Moon
        if (other.CompareTag("Moon"))
        {
            // hasMoon = true; // Marcamos que ahora tenemos la luna

            // actualizamos el game manager
            if (GameManager.Instance != null)
                GameManager.Instance.hasMoon = true;

            Debug.Log("Luna robada"); // Mostramos mensaje en consola 
            Destroy(other.gameObject); // Destruimos el objeto Luna de la escena (simula que la hemos recogido)
        }
    }
}