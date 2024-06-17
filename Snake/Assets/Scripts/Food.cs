using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Food : MonoBehaviour
{
    #region ||==|| CONSTANTS ||==|| 
    private const int BOUND_OFFSET = 1;
    private const float CENTER_OFFSET = 0.5f;
    #endregion
    #region ||==|| INSPECTOR VARIABLES ||==||
    [SerializeField] private BoxCollider2D mapArea;
    #endregion

    #region ||==|| START AND UPDATE ||==||
    void Start()
    {
        this.RandomizePosition();
    }
    #endregion

    #region ||==|| AUX METHODS ||==||
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
    #endregion

    #region ||==|| TRIGGER ||==||
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Si la serpiente choca con la manzana vuelvo a cambiar la posicion
        if (other.tag == "SnakeHead" || other.tag == "SnakeBody") this.RandomizePosition();
    }
    #endregion
}
