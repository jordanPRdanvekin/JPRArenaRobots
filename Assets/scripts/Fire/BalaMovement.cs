using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalaMovement : MonoBehaviour
{
    public PoolBala bulletPool;
    [SerializeField] GameObject explosionPrefab;

    float timer = 40f;
    float speed = 10f;

    Vector3 forward;

    public Transform pointer;
    public float rotationSpeed = 0.5f;

    Rigidbody bulletRb;

    float scoreAddAmount = 4f;

    private void Start()
    {
        bulletRb = GetComponent<Rigidbody>();
        pointer = gameObject.transform.GetChild(0);
    }

    private void OnEnable()
    {
        forward = transform.forward;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 40f;
            pointer.GetComponent<Mira>().target = null;
            bulletPool.ReturnToPool(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (pointer.GetComponent<Mira>().target == null)
        {
            bulletRb.velocity = forward * speed;
        }
        else
        {
            bulletRb.velocity = transform.up * -1 * 30;

            transform.rotation = Quaternion.Slerp(transform.rotation, pointer.transform.rotation, rotationSpeed);
            bulletRb.velocity = transform.forward * speed;
        } 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            crearPowerup.instance.SelectRandomPowerUp(collision.transform.position);
            GeneralController.instance.CheckEnemies();
            //ScoreManagerBehaviour.instance.AddScore(scoreAddAmount);
        }
        
        pointer.GetComponent<Mira>().target = null;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        bulletPool.ReturnToPool(gameObject);
    }
}
