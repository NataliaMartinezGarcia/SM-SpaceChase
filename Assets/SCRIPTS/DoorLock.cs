using UnityEngine;

/*
Va en un trigger invisible que cuando el jugador lo pisa,
manda cerrar y bloquear la puerta asignada de forma permanente a través de DoorProximityAutoClose

ES PARA DESACTIVAR LA PUERTA DE ENTRADA (UNA VEZ PASA NO NOS PODEMOS ESCONDER EN ESE PASILLO OTRA VEZ)
*/
public class DoorLock : MonoBehaviour
{
    [SerializeField] private DoorProximityAutoClose door; // la puerta que queremos bloquear
    [SerializeField] private string playerTag = "Player"; // esto es para filtrar que solo ocurra si es el jugador
    [SerializeField] private bool disableThisTriggerAfterLock = true; // di esto es true, el trigger se desactiva tras usarse una vez

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return; // solo reaccionamos si quien entra al trigger es el jugador

        // le decimos a la puerta que se cierre y se bloquee para siempre
        if (door != null)
            door.LockDoorPermanently(closeImmediately: true);

        // desactivamos este GameObject para que el trigger no vuelva a dispararse
        if (disableThisTriggerAfterLock)
            gameObject.SetActive(false);
    }
}
