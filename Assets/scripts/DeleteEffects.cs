using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteEffects : MonoBehaviour
{
    float timer;
    bool scaling = false;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f && scaling == false)
        {
            scaling = true;
            LeanTween.scale(gameObject, Vector3.zero, 1f);
        }
        if (timer >= 3f)
        {
            Destroy(gameObject);
        }
    }
}
