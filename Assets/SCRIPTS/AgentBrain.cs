using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AgentBrain : MonoBehaviour
{
    [SerializeField] private Transform player; // Referencia al objrto del jugdor 

    // Configuración de estados
    [Header("Patrol")]
    [SerializeField] public Transform[] patrolPoints;
    [SerializeField] public float patrolWaitingTime = 2f;
    [SerializeField] public float stopAtDistance = 0.5f;
    [SerializeField] public float speed = 15f;

    [Header("Chase")]
    [SerializeField] public float followPlayerTime = 0.6f;

    [Header("Investigate")]
    [SerializeField] public float investigateWaitTime = 1.5f;  // tiempo que espera en cada punto
    [SerializeField] public float investigateSearchRadius = 6f; // radio alrededor del sonido
    [SerializeField] public int investigatePointCount = 3;      // cuántos puntos investigar

    // Sensores y actuador
    private VisionSensor visionSensor;
    private CollisionSensor collisionSensor;
    private CaptureActuator captureActuator;
    private SoundSensor soundSensor;

    // Accesos que usamos para los estados para los estados
    public NavMeshAgent Agent { get; private set; }
    public Transform Player => player;
    public CaptureActuator CaptureActuator => captureActuator;

    // Hechos 
    public bool PlayerVisible { get; private set; }
    public bool PlayerColliding { get; private set; }
    public bool ReachedLastKnownPosition { get; private set;}
    public bool PlayerHeard { get; private set; }
    public bool InvestigationComplete { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; private set;}

    // lógica de los estados
    private State currentState;
    private float followingStateStartTime;
    private List<Transition> transitions;

    void Start()
    {
        // Inicialización del agente
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = speed;

        // Obtenemos componentes
        visionSensor = GetComponent<VisionSensor>();
        collisionSensor = GetComponent<CollisionSensor>();
        captureActuator = GetComponent<CaptureActuator>();
        soundSensor = GetComponent<SoundSensor>();

        // COnfiguramos sensores
        visionSensor.SetTarget(player);
        collisionSensor.SetTarget(player);
        captureActuator.SetTarget(player);
        soundSensor.SetTarget(player);
        
        // Guardamos la última posición conocida del jugador
        if (player != null) 
            LastKnownPlayerPosition = player.position;

        // Definimos las transiciones de la máquina de estados finita 
        transitions = new List<Transition>
        {
            // Si veo o colisiono con el jugador, persigo
            new Transition(typeof(PatrollingState), () => PlayerVisible || PlayerColliding, () => new FollowingState()),
            new Transition(typeof(SearchingState),  () => PlayerVisible || PlayerColliding, () => new FollowingState()),

            // Si lo pierdo mientras lo sigo, busco
            new Transition(typeof(FollowingState), () => !PlayerVisible && !PlayerColliding, () => new SearchingState(LastKnownPlayerPosition)),

            // Si termino de buscar, vuelvo a patrullar
            new Transition(typeof(SearchingState), () => ReachedLastKnownPosition, () => new PatrollingState()),

            // Si estoy investigando y vuelvo a ver o colisionar con el jugador, lo persigo
            new Transition(typeof(InvestigatingState), () => PlayerVisible || PlayerColliding, () => new FollowingState()),

            // Si estoy patrullando y escucho al jugador, investigo el sonido   
            new Transition(typeof(PatrollingState), () => PlayerHeard, () => new InvestigatingState(player.position, investigateSearchRadius, investigatePointCount)),
            
            // Si termino la investigación sin encontrar al jugador, vuelvo a patrullar
            new Transition(typeof(InvestigatingState), 
                () => InvestigationComplete, 
                () => {
                    InvestigationComplete = false; // reiniciamos la bandera para futuras investigaciones
                    return new PatrollingState();
                }),
        };

        ChangeState(new PatrollingState()); // Empezamos siempre patrullando
    }

    void Update()
    {
        // actualizamos la posición mientras lo vemos
        if (PlayerVisible && player != null)
            LastKnownPlayerPosition = player.position;

        currentState?.Update(this); // Ejecutamos la lógica del frame del estado actual (si existe)
    }

    // Hace el cambio de estado de la máquina de estados
    public void ChangeState(State newState)
    {
        // Si ya estamos en un estado del mismo tipo, no hacemos nada
        if (currentState != null && currentState.GetType() == newState.GetType())
            return;

        // Si existe un estado activo, ejecutamos su método Exit() antes de cambiar al nuevo estado
        if (currentState != null)
        {
            currentState.Exit(this);
        }
        currentState = newState; // El estado actual es el nuevo estado 

        // reset del hecho de búsqueda
        // Este flag solo tiene sentido en el estado de búsqueda, entonces al cambiar de estado hay que dejarlo en false 
        ReachedLastKnownPosition = false;

        currentState.Enter(this); // Ejecutamos la entrada del nuevo estado 

        // Si el nuevo estado es perseguir guardamos el instante en el que comienza la persecución.
        if (currentState is FollowingState)
            followingStateStartTime = Time.time;
    }

    // aqui decidimos en que transicion entramos
    // cuando llega una señal, entra en la primera transicion que coincide segun los hechos y estado actual
    private void CheckTransitions()
    {
        foreach (Transition transition in transitions) // mira la lista en orden
        {
            if (transition.Matches(currentState))
            {
                ChangeState(transition.To()); // cambiamos al estado que corresponda en esta transicion
                return; // si ya entramos en una transicion no seguimos
            }
        }
    }

    // MÉTODOS LLAMADOS POR LOS SENSORES 
    
    // Cuando un sensor detecta un cambio llama a alguna de estas funciones
    // que basicamente actualizan un hecho y comprueba si ahora coincide en una transicion
    public void OnPlayerSpotted()
    {
        PlayerVisible = true; // si ve al jugador ponemos el hecho a true
        if (player != null) LastKnownPlayerPosition = player.position; // Guardamos la última posición conocida del jugador 
        CheckTransitions();
    }

    public void OnPlayerOutOfSight()
    {
        PlayerVisible = false; // El sensor de visión deja de detectar al jugador 
        CheckTransitions();
    }

    public void OnPlayerCollisionEnter()
    {
        PlayerColliding = true; // El sensor de colisión indica contacto con el jugador.
        if (player != null) LastKnownPlayerPosition = player.position; // tb actualizamos la ultima posición conocida del jugador 
        CheckTransitions();

        // El guardia solo puede capturar al jugador si está en modo persecución y lleva persiguiéndolo durante un tiempo mínimo
        if (currentState is FollowingState)
        {
            if (Time.time - followingStateStartTime >= followPlayerTime)
                captureActuator.CapturePlayer();
        }
    }

    public void OnPlayerCollisionExit()
    {
        PlayerColliding = false; // Se ha perdido la colisión con el jugador.
        CheckTransitions();
    }
    
    public void OnReachedLastKnownPosition()
    {
        ReachedLastKnownPosition = true; // Indicamos que la búsqueda ha finalizado.
        CheckTransitions();
    }

    public void OnPlayerHeard()
    {
        Debug.Log("he escuchado");
        PlayerHeard = true;
        CheckTransitions();
    }

    public void OnPlayerOutOfHearing()
    {
        PlayerHeard = false;
        // no hace falta CheckTransitions aquí porque dejar de oír no dispara ninguna transición
    }
    // Lo llama InvestigatingState cuando termina de recorrer todos los puntos
    public void OnInvestigationComplete()
    {
        PlayerHeard = false;
        InvestigationComplete = true;
        CheckTransitions();
    }
}