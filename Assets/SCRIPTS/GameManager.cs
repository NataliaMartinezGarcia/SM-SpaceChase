using UnityEngine;

/*
Guarda el estado de la partida
- SI EL JUGADOR TIENE LA LUNA
- SI HA GANADO
*/

public class GameManager : MonoBehaviour
{      
    // cualquier script puede consultarlo con GameManager.Instance desde cualquier sitio del código
    // para eso ponemos static
    public static GameManager Instance;

    public bool hasMoon = false; // true cuando el jugador roba la luna
    public bool gameWon = false; // true cuando el jugador llega a la nave con la luna

    private void Awake()
    {   
        // solo puede haber uno activo en escena a la vez
       
        if (Instance == null)
            Instance = this; // si no existe ninguno, este pasa a ser el Instance global
        else
            Destroy(gameObject);  // si ya existe un GameManager destruimos este para evitar duplicados
    }
}
