using UnityEngine;
using UnityEngine.InputSystem;

public class tutotestrollball : MonoBehaviour
{
    private Rigidbody BolaRB;
    [SerializeField] private float moverX;
    [SerializeField] private float moverY;
    [SerializeField] public float vel = 10;
    void Start()
    {
        BolaRB = GetComponent<Rigidbody>();
    }
     public void OnMove(InputValue movementValue)
    {
        Vector2 vectormover = movementValue.Get<Vector2>();
        moverX = vectormover.x;
        moverY = vectormover.y;
    }
    private void FixedUpdate()
    {
        Vector3 moviendo = new Vector3(moverX, 0.0f, moverY);
        BolaRB.AddForce(moviendo * vel);

    }


}
