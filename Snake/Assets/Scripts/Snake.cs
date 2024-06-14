using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public float timeToMove = 0.5f;
    public float distanceToMove = 0.5f;
    // Direccion en la que se está moviendo la serpiente
    public Vector2 direction = Vector2.left;
    // Lista de prefabs que componen el cuerpo de la serpiente
    private List<Transform> snakeBody = new();
    // Prefab del cuerpo de la serpiente
    public Transform snakeBodyPrefab;
    public Transform snakeTailPrefab;
    public Sprite straightBodySprite;
    public Sprite curvedBodySprite;
    private float elapsedTimeLastMove = 0f;
    public BoxCollider2D mapArea;
    private GameManager gameManager;
    private bool isAlive = false;
    private bool canChangeDirection = false;

    // Start is called before the first frame update
    void Start()
    {
        this.InitSnake();
        this.gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.canChangeDirection)
        {
            if (Input.GetKeyDown(KeyCode.W) && this.direction != Vector2.down)
            {
                this.direction = Vector2.up;
                this.canChangeDirection = false;
            }
            else if (Input.GetKeyDown(KeyCode.S) && this.direction != Vector2.up)
            {
                this.direction = Vector2.down;
                this.canChangeDirection = false;
            }
            else if (Input.GetKeyDown(KeyCode.A) && this.direction != Vector2.right)
            {
                this.direction = Vector2.left;
                this.canChangeDirection = false;
            }
            else if (Input.GetKeyDown(KeyCode.D) && this.direction != Vector2.left)
            {
                this.direction = Vector2.right;
                this.canChangeDirection = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Tomo el tiempo (en segundos) que transcurrió desde el último frame hasta el actual
        // Lo sumo a la variable de control de movimiento
        this.elapsedTimeLastMove += Time.deltaTime;

        if (this.elapsedTimeLastMove > timeToMove && this.isAlive)
        {
            this.Move();
            this.elapsedTimeLastMove = 0f;
            this.canChangeDirection = true;
        }
    }

    private void Move()
    {
        // Almacena la posición anterior y la dirección de la cabeza
        Vector3 prevPosition = this.transform.position;

        // Muevo la cabeza de la serpiente hacia la dirección seleccionada
        this.transform.position = new Vector3(
            this.transform.position.x + this.direction.x * this.distanceToMove,
            this.transform.position.y + this.direction.y * this.distanceToMove,
            0f
        );


        this.AdjustRotationByDirection(this.transform, this.direction);


        // Recorro el cuerpo de la serpiente de la cola a la cabeza, excepto el último segmento (la cola)
        for (int i = this.snakeBody.Count - 1; i > 0; i--)
        {
            // Coloco cada segmento del cuerpo en la posición del segmento que le sigue
            this.snakeBody[i].position = this.snakeBody[i - 1].position;
            this.snakeBody[i].rotation = this.snakeBody[i - 1].rotation;
        }


        // Ajusta la posición y la rotación del primer segmento del cuerpo
        if (this.snakeBody.Count > 1)
        {
            this.snakeBody[1].position = prevPosition;
            this.snakeBody[1].rotation = this.transform.rotation;
        }

        for (int i = this.snakeBody.Count - 2; i > 0; i--)
        {
            this.AdjustSegmentSprite(i);
        }
    }

    private void AdjustCurvedBodyRotationByDirection(Transform transform, Vector2 prevDirection, Vector2 nextDirection)
    {
        //De abajo hacia arriba y de izquierda a derecha 
        //De derecha a izquierda y de arriba a abajo
        if (prevDirection == Vector2.right && nextDirection == Vector2.up ||
            prevDirection == Vector2.down && nextDirection == Vector2.left)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        //De abajo a arriba y de derecha a izquierda 
        //De izquierda a derecha y de arriba a abajo
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

        var nextDirection = (nextSegment.position - currentSegment.position).normalized;
        var previousDirection = (currentSegment.position - previousSegment.position).normalized;

        if (nextDirection != previousDirection)
        { 
            this.AdjustCurvedBodyRotationByDirection(currentSegment, previousDirection, nextDirection);
            currentSegment.GetComponent<SpriteRenderer>().sprite = curvedBodySprite;
        }
        else
        {
            currentSegment.GetComponent<SpriteRenderer>().sprite = straightBodySprite;
        }
    }

    public void InitSnake()
    {
        foreach (var bodySegment in this.snakeBody)
        {
            if (bodySegment != this.transform) // No destruir la cabeza de la serpiente
            {
                Destroy(bodySegment.gameObject);
            }
        }

        this.snakeBody.Clear();

        this.direction = Vector2.left;

        this.isAlive = true;

        // Añadir la cabeza
        this.snakeBody.Add(this.transform);

        // Crear un segmento de cuerpo
        var segmentSnakeBody = Instantiate(this.snakeBodyPrefab);
        var bodyPosition = this.snakeBody[this.snakeBody.Count - 1].position;
        bodyPosition.x += 1;
        segmentSnakeBody.position = bodyPosition;
        this.snakeBody.Add(segmentSnakeBody);

        // Crear la cola
        var segmentSnakeTail = Instantiate(this.snakeTailPrefab);
        var tailPosition = this.snakeBody[this.snakeBody.Count - 1].position;
        tailPosition.x += 1;
        segmentSnakeTail.position = tailPosition;
        this.snakeBody.Add(segmentSnakeTail);
    }

    private void Grow()
    {
        this.gameManager.Score++;
        // Instancio un nuevo segmento del cuerpo de la serpiente.
        var segmentSnakeBody = Instantiate(this.snakeBodyPrefab);
        // Lo coloco en la posición del segmento antes de la cola
        segmentSnakeBody.position = this.snakeBody[this.snakeBody.Count - 2].position;
        // Lo añado al cuerpo de la serpiente justo antes de la cola
        this.snakeBody.Insert(this.snakeBody.Count - 1, segmentSnakeBody);
    }

    private void Die()
    {
        this.isAlive = false;
        this.gameManager.GameOver();
    }

    private void TeleportToOtherBound(string boundTag)
    {
        var newPosition = this.transform.position;
        switch (boundTag)
        {
            case "BoundTop":
                newPosition.y = MathF.Round(this.mapArea.bounds.min.y) + 0.5f;
                break;
            case "BoundBottom":
                newPosition.y = MathF.Round(this.mapArea.bounds.max.y) - 0.5f;
                break;
            case "BoundRight":
                newPosition.x = MathF.Round(this.mapArea.bounds.min.x) + 0.5f;
                break;
            default:
                newPosition.x = MathF.Round(this.mapArea.bounds.max.x) - 0.5f;
                break;
        }
        this.transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Food") Grow();
        else if (other.tag.StartsWith("Bound")) TeleportToOtherBound(other.tag);
        else if (other.tag == "SnakeBody") Die();
    }
}
