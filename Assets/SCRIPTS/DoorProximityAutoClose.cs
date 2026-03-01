using System.Collections;
using UnityEngine;

public class DoorProximityAutoClose : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Esto es un minimo por si se nos olvida poner algo dentro de allowedtags en el editor por lo menos va a poder pasar el jugador siempre
    [SerializeField] private string playerTag = "Player";

    [SerializeField] private string[] allowedTags = { "Player", "Guard" }; // tags de personajes que pueden abrir la puerta

    [Header("Animator Triggers")]
    [SerializeField] private string openTrigger = "Open"; // trigger del Animator que activa la animación de abrir la puerta
    [SerializeField] private string closeTrigger = "Close"; // trigger del Animator que activa la animación de cerrar la puerta

    [Header("Auto Close")]
    [SerializeField] private float closeDelaySeconds = 0.3f; // tiempo antes de cerrar de que la puerta se cierre automáticamente
    [SerializeField] private bool keepOpenWhilePlayerInside = true;

    [Header("Vision Layers")]
    [SerializeField] private string openDoorLayer = "Ignore Raycast"; // Layer cuando está abierta (NO bloquea la vista del agente)
    [SerializeField] private string closedDoorLayer = "Door"; // Layer cuando está cerrada (bloquea la vista del agente)

    [Header("Lock")]
    [SerializeField] private bool startLocked = false; // si quieres que empiece bloqueada (normalmente false)

    [Header("Require Moon")]
    [SerializeField] private bool requireMoonToOpen = false;
    [SerializeField] private bool logWhenNoMoon = true;
    [SerializeField] private string noMoonMessage = "Tienes que robar la luna para poder salir.";

    private Coroutine closeRoutine; // Referencia a la coroutine de cierre (para poder cancelarla)
    private int allowedInside = 0; // Contador de cuantos personajes de los permitidos están dentro del trigger

    private bool locked = false; // Bloqueada o no 
    private bool lockedByMoon = false; // Bloqueada por no tener la luna 

    
    private void Reset()
    {
        // Si no hay animator asignado, busca uno en los objetos hijos
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    // se ejecuta cuando el objeto se inicializa
    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();

        locked = startLocked;
        if (requireMoonToOpen && !HasMoon())
        {
            locked = true;
            lockedByMoon = true;
            ForceCloseNow();
        }
        else if (locked)
        {
            ForceCloseNow();
        }
    }

    private void Update()
    {
        // Si estaba bloqueada por la luna y ahora ya la tenemos: desbloquea
        if (lockedByMoon && HasMoon())
        {
            locked = false;
            lockedByMoon = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si es la puerta de salida y aún no hay luna, NO ABRE y avisa
        if (requireMoonToOpen && other.CompareTag(playerTag) && !HasMoon())
        {
            if (logWhenNoMoon) Debug.Log(noMoonMessage);
            ForceCloseNow();
            return;
        }

        if (locked) return;
        if (!IsAllowed(other)) return;

        allowedInside++;
        OpenDoor();
        CancelClose();
    }

    // se ejecuta cuando otro collider sale del trigger de la puerta
    private void OnTriggerExit(Collider other)
    {
        if (locked) return;   // si está bloqueada, ignoramos
        if (!IsAllowed(other)) return; // Si el objeto que sale no tiene un tag permitido, no hace nada

        allowedInside = Mathf.Max(0, allowedInside - 1); // reduce el contador (pero nunca se hace menor que 0)
        if (allowedInside == 0) ScheduleClose(); // Si no quedan personajes permitidos dentro, la puerta se cierra
    }

    // comprueba si el collider que entró/salió es de un personaje permitido
    private bool IsAllowed(Collider other)
    {
        // si no ponemos los personajes permitidos en allowedTags, sigue usando playerTag
        if (allowedTags == null || allowedTags.Length == 0)
            return other.CompareTag(playerTag);

        // mira si el tag que estamos comprobando esta dentro de la lista de permitidos
        for (int i = 0; i < allowedTags.Length; i++)
        {
            if (other.CompareTag(allowedTags[i])) return true;
        }
        return false;
    }

    private void OpenDoor()
    {
        if (locked) return; // doble seguridad

        animator.ResetTrigger(closeTrigger);
        animator.SetTrigger(openTrigger);

        // Cambia el layer de la puerta para que los raycasts la ignoren
        // Asi el vision sensor del agente puede ver a traves de una puerta abierta
        gameObject.layer = LayerMask.NameToLayer(openDoorLayer);
    }

    private void CloseDoor()
    {
        if (locked)
        {
            // Si está bloqueada, SIEMPRE cerramos (aunque hubiera alguien dentro)
            ForceCloseNow();
            return;
        }

        if (keepOpenWhilePlayerInside && allowedInside > 0) return;

        animator.ResetTrigger(openTrigger);
        animator.SetTrigger(closeTrigger);

        // Cambia el layer de la puerta para que los raycasts choquen con ella
        // Asi el vision sensor del guardia no puede ver a traves de una puerta cerrada
        gameObject.layer = LayerMask.NameToLayer(closedDoorLayer);
    }

    // fuerza cierre inmediato (y limpia contador/coroutine)
    private void ForceCloseNow()
    {
        CancelClose();
        allowedInside = 0;

        animator.ResetTrigger(openTrigger);
        animator.SetTrigger(closeTrigger);

        gameObject.layer = LayerMask.NameToLayer(closedDoorLayer);
    }

    // programa el cierre automático dela puerta para después de un tiempo
    private void ScheduleClose()
    {
        CancelClose();
        closeRoutine = StartCoroutine(CloseAfterDelay());
    }

    // cancela el cierre automático programado de la puerta
    private void CancelClose()
    {
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null; //Limpia la referencia
        }
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelaySeconds);
        CloseDoor();
        closeRoutine = null;
    }

    public void LockDoorPermanently(bool closeImmediately = true)
    {
        locked = true;

        if (closeImmediately)
            ForceCloseNow();
    }

    // (Opcional) por si en algún momento quieres desbloquear desde otro script
    public void UnlockDoor()
    {
        locked = false;
    }

    private bool HasMoon()
    {
        return (GameManager.Instance != null && GameManager.Instance.hasMoon);
    }
}