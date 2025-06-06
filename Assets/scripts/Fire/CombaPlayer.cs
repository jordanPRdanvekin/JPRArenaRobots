using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombaPlayer : MonoBehaviour
{
    StarterAssetsInputs input;
    [SerializeField] GameObject ball;
    [SerializeField] Transform[] spawnBall;
    bool fire;
    private void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
    }
    private void Update()
    {
        fire = input.Fire;
        if (fire)
        {
            
            InstanceBall();
            input.Fire = false; 
        }
    }
    void InstanceBall()
    {
        GameObject ballpre1 = Instantiate(ball, spawnBall[0].transform.position,Quaternion.identity);
        GameObject ballpre2 = Instantiate(ball, spawnBall[1].transform.position, Quaternion.identity);
        ballpre1.GetComponent<BallMovement>().dire = spawnBall[0].forward;
        ballpre2.GetComponent<BallMovement>().dire = spawnBall[1].forward;
    }
}
