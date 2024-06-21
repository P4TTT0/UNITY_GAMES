using Assets.Scripts;
using Assets.Scripts.Functions;
using Assets.Scripts.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Snake : MonoBehaviour
{
    #region ||==|| PROPERTIES ||==||
    public bool IsAlive { get { return this.isAlive; } }
    #endregion

    #region ||==|| CONSTANTS ||==||
    private const float CIRCLECAST_RADIUS = 1.0f;
    private const float CIRCLECAST_DISTANCE = 3.0F;
    private const float TIME_CLOSE_MOUTH = 0.7f;
    private const float DISTANCE_MOVE = 1f;
    #endregion

    #region ||==|| VARIABLES ||==||
    public Vector2 direction = Vector2.left;
    private Queue<Vector2> directionBuffer = new Queue<Vector2>(); 
    private List<Transform> snakeBody = new List<Transform>();
    private GameManager gameManager;
    private Animator animator;
    private float timeToMove = 0.08f;
    private float elapsedTimeLastMove = 0f;
    private float elapsedTimeLastNonHit = 0f;
    private bool isAlive = false;
    private bool isMouthOpen = false;
    #endregion

    #region ||==|| INSPECTOR VARIABLES ||==||
    [SerializeField] private Transform snakeBodyPrefab;
    [SerializeField] private Transform snakeTailPrefab;
    [SerializeField] private Sprite straightBodySprite;
    [SerializeField] private Sprite curvedBodySprite;
    [SerializeField] private BoxCollider2D mapArea;
    [SerializeField] private LayerMask foodLayer;
    #endregion

    #region ||==|| AUDIO ||==||
    [SerializeField] private AudioClip biteSound;
    [SerializeField] private AudioClip beepSound;
    [SerializeField] private AudioClip impactSound;
    private AudioSource audioSource;
    #endregion

    #region ||==|| START & UPDATE ||==||
    void Start()
    {
        this.InitSnake();
        this.gameManager = FindObjectOfType<GameManager>();
        this.audioSource = this.gameObject.AddComponent<AudioSource>();
        this.animator = this.gameObject.GetComponent<Animator>();
        this.AdjustDifficulty();
    }

    void Update()
    {
        if(this.isAlive)
        {
            this.HandleInput();
            this.HandleRaycast();
        }
    }

    void FixedUpdate()
    {
        this.elapsedTimeLastMove += Time.deltaTime;

        if (this.elapsedTimeLastMove > this.timeToMove && this.isAlive)
        {
            this.elapsedTimeLastMove = 0f;
            this.ProcessBufferedDirection();
            this.Move();
        }
    }
    #endregion

    #region ||==|| BUFFER DIRECTION METHODS ||==||
    private void AddToBuffer(Vector2 newDirection)
    {
        if (this.directionBuffer.Count < 4) this.directionBuffer.Enqueue(newDirection);
    }

    private void ProcessBufferedDirection()
    {
        if (this.directionBuffer.Count <= 0) return;

        var prevDirection = direction;
        this.direction = directionBuffer.Dequeue();
        if (this.direction != prevDirection) this.audioSource.PlayOneShot(this.beepSound);
    }
    #endregion

    #region ||==|| MAIN METHODS ||==||
    private void Move()
    {
        Vector3 prevPosition = this.transform.position;

        //Muevo la cabeza hacia la direccion.
        this.transform.position = new Vector3(
            this.transform.position.x + this.direction.x * DISTANCE_MOVE,
            this.transform.position.y + this.direction.y * DISTANCE_MOVE,
            0f
        );

        //Ajusto la rotacion de la cabeza por la direccion.
        this.AdjustRotationByDirection(this.transform, this.direction);

        //Muevo el resto del cuerpo segmento por segmento desde la cola para adelante. Copiando posicion y rotacion del segmento sucesor
        for (int i = snakeBody.Count - 1; i > 0; i--)
        {
            this.snakeBody[i].position = this.snakeBody[i - 1].position;
            this.snakeBody[i].rotation = this.snakeBody[i - 1].rotation;
        }

        //El elemento en el indice 1 siempre sera sucesor de la cabeza, por lo tanto copiamos su direccion y rotacion.
        if (this.snakeBody.Count > 1)
        {
            this.snakeBody[1].position = prevPosition;
            this.snakeBody[1].rotation = this.transform.rotation;
        }

        //Ajustamos el sprite de todos los elementos que estan entre la cola y la cabeza.
        for (int i = this.snakeBody.Count - 2; i > 0; i--)
        {
            this.AdjustSegmentSprite(i);
        }

        //Ajustamos la rotacion de la cola.
        this.AdjustTailRotation();
    }

    public void InitSnake()
    {
        //Destruyo cada segmento de la serpiente menos la cabeza.
        foreach (var bodySegment in snakeBody)
        {
            if (bodySegment != transform) Destroy(bodySegment.gameObject);
        }

        //Reinicio la posicion y rotacion de la caebeza.
        this.transform.position = new Vector3(0.5f, 0.5f, 0);
        this.transform.rotation = Quaternion.Euler(0, 0, 0);


        //Limpio la lista de segmentos de la serpiente y añado la cabeza como primer elemento.
        this.snakeBody.Clear();
        this.snakeBody.Add(transform);

        // Creo un segmento del cuerpo
        var segmentSnakeBody = Instantiate(this.snakeBodyPrefab);
        var bodyPosition = this.snakeBody[this.snakeBody.Count - 1].position;
        bodyPosition.x += 1;
        segmentSnakeBody.position = bodyPosition;
        this.snakeBody.Add(segmentSnakeBody);

        // Creo la cola
        var segmentSnakeTail = Instantiate(this.snakeTailPrefab);
        var tailPosition = this.snakeBody[this.snakeBody.Count - 1].position;
        tailPosition.x += 1;
        segmentSnakeTail.position = tailPosition;
        this.snakeBody.Add(segmentSnakeTail);

        //Reinicio la direccion.
        this.direction = Vector2.left;

        this.isAlive = true;
    }

    private void Grow()
    {
        this.gameManager.Score++;
        var segmentSnakeBody = Instantiate(this.snakeBodyPrefab);
        segmentSnakeBody.position = this.snakeBody[this.snakeBody.Count - 2].position;
        this.snakeBody.Insert(this.snakeBody.Count - 1, segmentSnakeBody);

        this.audioSource.PlayOneShot(this.biteSound);
    }

    private void Die()
    {
        this.audioSource.PlayOneShot(this.impactSound);
        this.isAlive = false;
        this.gameManager.GameOver();
    }

    private void TeleportToOtherBound(string boundTag)
    {
        var newPosition = this.transform.position;
        switch (boundTag)
        {
            case "BoundTop":
                newPosition.y = MathF.Round(mapArea.bounds.min.y) + 0.5f;
                break;
            case "BoundBottom":
                newPosition.y = MathF.Round(mapArea.bounds.max.y) - 0.5f;
                break;
            case "BoundRight":
                newPosition.x = MathF.Round(mapArea.bounds.min.x) + 0.5f;
                break;
            default:
                newPosition.x = MathF.Round(mapArea.bounds.max.x) - 0.5f;
                break;
        }
        this.transform.position = newPosition;
    }
    #endregion

    #region ||==|| AUX METHODS ||==||
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && direction != Vector2.down) AddToBuffer(Vector2.up);
        else if (Input.GetKeyDown(KeyCode.S) && direction != Vector2.up) AddToBuffer(Vector2.down);
        else if (Input.GetKeyDown(KeyCode.A) && direction != Vector2.right) AddToBuffer(Vector2.left);
        else if (Input.GetKeyDown(KeyCode.D) && direction != Vector2.left) AddToBuffer(Vector2.right);
    }
    private void HandleRaycast()
    {
       var raycastHit = Physics2D.CircleCast(this.transform.position, CIRCLECAST_RADIUS, this.direction, CIRCLECAST_DISTANCE, this.foodLayer);

        if (raycastHit.collider is not null)
        {
            if (!this.isMouthOpen)
            {
                this.animator.SetTrigger("OpenMouth");
                this.isMouthOpen = true;
            }
        }
        else
        {
            this.elapsedTimeLastNonHit += Time.deltaTime;
            if (this.isMouthOpen && this.elapsedTimeLastNonHit > TIME_CLOSE_MOUTH)
            {
                this.elapsedTimeLastNonHit = 0;
                this.animator.SetTrigger("CloseMouth");
                this.isMouthOpen = false;
            }
        }
    }

    private void AdjustCurvedBodyRotationByDirection(Transform transform, Vector2 prevDirection, Vector2 nextDirection)
    {
        if (prevDirection == Vector2.right && nextDirection == Vector2.up ||
            prevDirection == Vector2.down && nextDirection == Vector2.left)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (prevDirection == Vector2.up && nextDirection == Vector2.left ||
                 prevDirection == Vector2.right && nextDirection == Vector2.down)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (prevDirection == Vector2.down && nextDirection == Vector2.right ||
                 prevDirection == Vector2.left && nextDirection == Vector2.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (prevDirection == Vector2.up && nextDirection == Vector2.right ||
                 prevDirection == Vector2.left && nextDirection == Vector2.down)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    private void AdjustRotationByDirection(Transform transfrom, Vector2 direction)
    {
        if (direction == Vector2.up) transfrom.rotation = Quaternion.Euler(0, 0, -90);
        else if (direction == Vector2.down) transfrom.rotation = Quaternion.Euler(0, 0, 90);
        else if (direction == Vector2.left) transfrom.rotation = Quaternion.Euler(0, 0, 0);
        else if (direction == Vector2.right) transfrom.rotation = Quaternion.Euler(0, 0, 180);
    }

    private void AdjustSegmentSprite(int index)
    {
        var currentSegment = snakeBody[index];
        var nextSegment = snakeBody[index - 1];
        var previousSegment = snakeBody[index + 1];

        var nextDirection = (nextSegment.position - currentSegment.position);
        var previousDirection = (currentSegment.position - previousSegment.position);

        if (nextDirection != previousDirection && (VectorUtils.IsNormalized(nextDirection) && VectorUtils.IsNormalized(previousDirection)))
        {
            this.AdjustCurvedBodyRotationByDirection(currentSegment, previousDirection, nextDirection);
            currentSegment.GetComponent<SpriteRenderer>().sprite = this.curvedBodySprite;
        }
        else currentSegment.GetComponent<SpriteRenderer>().sprite = this.straightBodySprite;
    }

    private void AdjustTailRotation()
    {
        var tailSegment = snakeBody[snakeBody.Count - 1];
        var beforeTailSegment = snakeBody[snakeBody.Count - 2];

        var tailDirection = (tailSegment.position - beforeTailSegment.position).normalized;

        if (tailDirection == Vector3.up) tailSegment.rotation = Quaternion.Euler(0, 0, 90);
        else if (tailDirection == Vector3.down) tailSegment.rotation = Quaternion.Euler(0, 0, -90);
        else if (tailDirection == Vector3.left) tailSegment.rotation = Quaternion.Euler(0, 0, 180);
        else if (tailDirection == Vector3.right) tailSegment.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void AdjustDifficulty()
    {
        if (GameSettings.difficultyType == DifficultyType.Hard) this.timeToMove = this.timeToMove / 2;
    }
    #endregion

    #region ||==|| TRIGGER ||==||
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Food") Grow();
        else if (other.tag == "SnakeBody") Die();
        else if (other.tag.StartsWith("Bound"))
        {
            if (GameSettings.difficultyType == DifficultyType.Medium || GameSettings.difficultyType == DifficultyType.Hard) Die();
            else TeleportToOtherBound(other.tag);
        }
    }
    #endregion
}
