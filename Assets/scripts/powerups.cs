using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerups : MonoBehaviour
{
    [SerializeField]
    private LeanTweenType animType;
    private float animDuration = 1f;
    private float animScale = 0.5f;

    private void Start()
    {
        LeanTween.moveLocalY(gameObject, transform.position.y + animScale, animDuration).setEase(animType).setLoopPingPong();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (gameObject.name.Contains("Infinita"))
            {
                energySystem.instance.InfiniteEnergyPowerUp();
            }
            else if (gameObject.name.Contains("Recargar"))
            {
                energySystem.instance.FullReloadPowerUp();
            }

            Destroy(gameObject);
        }
    }
}
