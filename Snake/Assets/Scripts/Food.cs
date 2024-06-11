using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Food : MonoBehaviour
{
    public BoxCollider2D mapArea;
    private const int BOUND_OFFSET = 1;
    private const float CENTER_OFFSET = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        this.RandomizePosition();
    }

    public void RandomizePosition()
    {
        //Agarro los limites del mapa
        var bounds = this.mapArea.bounds;

        //Tomo un valor al azar del minimo y el maximo del eje x 
        var x = Mathf.Round(Random.Range(bounds.min.x + BOUND_OFFSET, bounds.max.x - BOUND_OFFSET)) + CENTER_OFFSET;
        //Tomo un valor al azar del minimo y el maximo del eje y
        var y = Mathf.Round(Random.Range(bounds.min.y + BOUND_OFFSET, bounds.max.y - BOUND_OFFSET)) + CENTER_OFFSET;

        //Muevo la manzana a la posicion al azar
        this.transform.position = new Vector3(x, y, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Si la serpiente choca con la manzana vuelvo a cambiar la posicion
        if (other.tag == "SnakeHead" || other.tag == "SnakeBody") this.RandomizePosition();
    }
}
