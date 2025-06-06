using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [SerializeField] float velocity = 2;
    public Vector3 dire;
    private void Start()
    {
        Destroy(gameObject,6);
    }
    private void Update()
    {
        MovementBall();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Debug.Log("Destroy");
            Destroy(gameObject);
            
        }
    }
    void MovementBall()
    {
        transform.position += dire * velocity * Time.deltaTime;
    }
}
