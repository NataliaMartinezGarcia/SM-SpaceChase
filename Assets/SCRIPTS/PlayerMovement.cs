using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // SerializeField para que podamos editar desde unity aunque sea privada
    [SerializeField] private float speed = 5f;
    private Rigidbody rb; // para que pueda chocar con las paredes
    private Animator anim; 
    private Vector3 moveDirection;  // dirección en la que nos movemos

    // se ejecuta UNA SOLA VEZ cuando el juego inicia
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        
        // Para no atravesar paredes 
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;
    }

    // Update se ejecuta CADA FRAME
    // para capturar input del jugador porque necesita respuesta inmediata
    void Update()
    {
        // teclas A y D o flechas izq/dcha
        float horizontalMovement = Input.GetAxis("Horizontal");
        // teclas W y S o flechas arriba/abajo
        float verticalMovement = Input.GetAxis("Vertical");

        // horizontalMovement: movimiento en X (izquierda/derecha)
        // 0f en Y porque no queremos movimiento vertical
        // verticalMovement: movimiento en Z (adelante/atrás)
        // normalized para que el personaje no se mueva mas rapido si va en diagonal
        moveDirection = new Vector3(horizontalMovement, 0f, verticalMovement).normalized;

        float movementAmount = moveDirection.magnitude;
        anim.SetFloat("Speed", movementAmount);
    }

    // Se ejecuta en intervalos fijos 
    void FixedUpdate()
    {
        if (moveDirection != Vector3.zero)
        {
            // Rotar hacia donde se mueve
            transform.rotation = Quaternion.LookRotation(moveDirection);
            
            // Usar velocity en lugar de MovePosition
            Vector3 targetVelocity = moveDirection * speed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            // Detener el movimiento horizontal cuando no hay input
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
}