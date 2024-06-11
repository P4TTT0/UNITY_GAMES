using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public float timeToMove = 0.5f;
    public float distanceToMove = 0.5f;
    //Direccion en la que se esta moviendo la serpiente
    public Vector2 direction = Vector2.left;
    //Lista de prefabs que componen el cuerpo de la serpiente
    private List<Transform> snakeBody = new();
    //Prefab del cuerpo de la serpiente
    public Transform snakeBodyPrefab;
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
        //Tomo el tiempo (en segundos) que transcurrio desde el ultimo frame hasta el actual
        //Lo sumo a la variable de control de movimiento
        this.elapsedTimeLastMove += Time.deltaTime;

        if(this.elapsedTimeLastMove > timeToMove && this.isAlive)
        {
            this.Move();
            this.elapsedTimeLastMove = 0f;
            this.canChangeDirection = true;
        }
    }

    private void Move()
    {

        //Recorro el cuerpo de la serpiente de la cola a la cabeza
        for (int i = this.snakeBody.Count - 1; i > 0; i--)
        {
            //Coloco cada segmento del cuerpo en la posicion del segmento que le sigue
            this.snakeBody[i].position = this.snakeBody[i - 1].position;
        }

        //Muevo la cabeza de la serpiente hacia la direccion seleccionada
        this.transform.position = new Vector3(
            this.transform.position.x + this.direction.x * this.distanceToMove,
            this.transform.position.y + this.direction.y * this.distanceToMove,
            0f
        );
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

        this.isAlive = true;

        //Añado la cabeza
        this.snakeBody.Add(this.transform);

        //Creo la cola 
        var segmentSnakeBody = Instantiate(this.snakeBodyPrefab);
        var tailPosition = this.snakeBody[this.snakeBody.Count - 1].position;
        tailPosition.x += 1;
        segmentSnakeBody.position = tailPosition;
        this.snakeBody.Add(segmentSnakeBody);
    }

    private void Grow()
    {
        this.gameManager.Score++;
        //Instancio un nuevo segmento del cuerpo de la serpiente.
        var segmentSnakeBody = Instantiate(this.snakeBodyPrefab);
        //Lo coloco en la posicion de la cola
        segmentSnakeBody.position = this.snakeBody[this.snakeBody.Count - 1].position;
        //Lo añado al cuerpo de la serpiente
        this.snakeBody.Add(segmentSnakeBody);
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
