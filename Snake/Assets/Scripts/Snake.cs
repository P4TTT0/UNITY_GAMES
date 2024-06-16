using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public float timeToMove = 0.5f;
    public float distanceToMove = 0.5f;
    public Vector2 direction = Vector2.left;
    private Queue<Vector2> directionBuffer = new Queue<Vector2>(); // Cola para almacenar las direcciones
    private List<Transform> snakeBody = new List<Transform>();
    public Transform snakeBodyPrefab;
    public Transform snakeTailPrefab;
    public Sprite straightBodySprite;
    public Sprite curvedBodySprite;
    private float elapsedTimeLastMove = 0f;
    public BoxCollider2D mapArea;
    private GameManager gameManager;
    private bool isAlive = false;

    public AudioClip biteSound;
    public AudioClip beepSound;
    public AudioClip impactSound;
    private AudioSource audioSource;

    void Start()
    {
        this.InitSnake();
        this.gameManager = FindObjectOfType<GameManager>();

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        elapsedTimeLastMove += Time.deltaTime;

        if (elapsedTimeLastMove > timeToMove && isAlive)
        {
            elapsedTimeLastMove = 0f;
            ProcessBufferedDirection();
            Move();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && direction != Vector2.down)
        {
            AddToBuffer(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) && direction != Vector2.up)
        {
            AddToBuffer(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) && direction != Vector2.right)
        {
            AddToBuffer(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) && direction != Vector2.left)
        {
            AddToBuffer(Vector2.right);
        }
    }

    private void AddToBuffer(Vector2 newDirection)
    {
        if (directionBuffer.Count < 2)
        {
            directionBuffer.Enqueue(newDirection);
            print($"NUEVA DIRECCION: {newDirection}");
        }
    }

    private void ProcessBufferedDirection()
    {
        if (directionBuffer.Count > 0)
        {
            var prevDirection = direction;
            direction = directionBuffer.Dequeue();
            if (direction != prevDirection)
            {
                audioSource.PlayOneShot(beepSound);
            }
        }
    }

    private void Move()
    {
        Vector3 prevPosition = transform.position;

        transform.position = new Vector3(
            transform.position.x + direction.x * distanceToMove,
            transform.position.y + direction.y * distanceToMove,
            0f
        );

        AdjustRotationByDirection(transform, direction);

        for (int i = snakeBody.Count - 1; i > 0; i--)
        {
            snakeBody[i].position = snakeBody[i - 1].position;
            snakeBody[i].rotation = snakeBody[i - 1].rotation;
        }

        if (snakeBody.Count > 1)
        {
            snakeBody[1].position = prevPosition;
            snakeBody[1].rotation = transform.rotation;
        }

        for (int i = snakeBody.Count - 2; i > 0; i--)
        {
            AdjustSegmentSprite(i);
        }

        AdjustTailRotation();
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
        if (direction == Vector2.up)
        {
            transfrom.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (direction == Vector2.down)
        {
            transfrom.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (direction == Vector2.left)
        {
            transfrom.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == Vector2.right)
        {
            transfrom.rotation = Quaternion.Euler(0, 0, 180);
        }
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
            AdjustCurvedBodyRotationByDirection(currentSegment, previousDirection, nextDirection);
            currentSegment.GetComponent<SpriteRenderer>().sprite = curvedBodySprite;
        }
        else
        {
            currentSegment.GetComponent<SpriteRenderer>().sprite = straightBodySprite;
        }
    }

    private void AdjustTailRotation()
    {
        var tailSegment = snakeBody[snakeBody.Count - 1];
        var beforeTailSegment = snakeBody[snakeBody.Count - 2];

        var tailDirection = (tailSegment.position - beforeTailSegment.position).normalized;

        if (tailDirection == Vector3.up)
        {
            tailSegment.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (tailDirection == Vector3.down)
        {
            tailSegment.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (tailDirection == Vector3.left)
        {
            tailSegment.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (tailDirection == Vector3.right)
        {
            tailSegment.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void InitSnake()
    {
        foreach (var bodySegment in snakeBody)
        {
            if (bodySegment != transform) // No destruir la cabeza de la serpiente
            {
                Destroy(bodySegment.gameObject);
            }
        }

        transform.position = new Vector3(0.5f, 0.5f, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        snakeBody.Clear();

        direction = Vector2.left;

        // Añadir la cabeza
        snakeBody.Add(transform);

        // Crear un segmento de cuerpo
        var segmentSnakeBody = Instantiate(snakeBodyPrefab);
        var bodyPosition = snakeBody[snakeBody.Count - 1].position;
        bodyPosition.x += 1;
        segmentSnakeBody.position = bodyPosition;
        snakeBody.Add(segmentSnakeBody);

        // Crear la cola
        var segmentSnakeTail = Instantiate(snakeTailPrefab);
        var tailPosition = snakeBody[snakeBody.Count - 1].position;
        tailPosition.x += 1;
        segmentSnakeTail.position = tailPosition;
        snakeBody.Add(segmentSnakeTail);

        isAlive = true;
    }

    private void Grow()
    {
        gameManager.Score++;
        var segmentSnakeBody = Instantiate(snakeBodyPrefab);
        segmentSnakeBody.position = snakeBody[snakeBody.Count - 2].position;
        snakeBody.Insert(snakeBody.Count - 1, segmentSnakeBody);

        audioSource.PlayOneShot(biteSound);
    }

    private void Die()
    {
        audioSource.PlayOneShot(impactSound);
        isAlive = false;
        gameManager.GameOver();
    }

    private void TeleportToOtherBound(string boundTag)
    {
        var newPosition = transform.position;
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
        transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Food") Grow();
        else if (other.tag.StartsWith("Bound")) TeleportToOtherBound(other.tag);
        else if (other.tag == "SnakeBody") Die();
    }
}
