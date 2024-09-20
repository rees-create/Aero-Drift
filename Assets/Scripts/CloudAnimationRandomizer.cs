using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudAnimationRandomizer : MonoBehaviour
{
    [SerializeField] Vector2 range;
    [SerializeField] float speed;
    [SerializeField] float speedDeviation;

    float speedDiff = 0;
    float direction = 0;
    void start(float direction) 
    {
        transform.position = new Vector3(0, transform.position.y, 0) + new Vector3(-range.x * direction, 0, 0);
    }
    float restart(float direction) 
    {
        float rand = Random.Range(-100, 100) / 100.0f;
        transform.position = new Vector3(0, rand * range.y, 0) + new Vector3(-range.x * direction, 0, 0);
        return rand * speedDeviation;
    }
    // Start is called before the first frame update
    void Start()
    {
        // ei boi just move it in the code wai
        direction = speed / Mathf.Abs(speed);
        start(direction);

    }

    // Update is called once per frame
    void Update()
    {
        bool atEndOfRange = transform.position.x * direction >= range.x;
        if (!atEndOfRange)
        {
            transform.position += new Vector3(Time.deltaTime, 0, 0) * (speed + speedDiff);
        }
        else
        {
            speedDiff = restart(direction);
        }

    }
}
