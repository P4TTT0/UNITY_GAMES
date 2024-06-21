using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBlink : MonoBehaviour
{
    private Animator animator; 
    private const float MIN_BLINK_TIME = 3.0f;
    private const float MAX_BLINK_TIME = 5.0f; 

    void Start()
    {
        this.animator = this.GetComponent<Animator>();
        this.ScheduleNextBlink();
    }

    void ScheduleNextBlink()
    {
        float blinkTime = Random.Range(MIN_BLINK_TIME, MAX_BLINK_TIME);
        this.Invoke("Blink", blinkTime);
    }

    void Blink()
    {
        this.animator.SetTrigger("Blink");
        this.ScheduleNextBlink();
    }
}
