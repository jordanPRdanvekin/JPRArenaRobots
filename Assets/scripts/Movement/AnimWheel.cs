using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class AnimWheel : MonoBehaviour
{
    [SerializeField] Transform wheel;
    CharacterController _char;
    [SerializeField] float radio;
    [SerializeField] Vector3 dirWheel;
    MovementManager _playerC;
    float velocityWheel;
    private void Start()
    {
        _char = GetComponent<CharacterController>();
        _playerC = GetComponent<MovementManager>();
    }
    void Update()
    {
        AniRotationWheel();
    }
    void AniRotationWheel()
    {
        velocityWheel = _char.velocity.magnitude;

        float CR = 2 * Mathf.PI * radio;
        float revolucionesXSg = velocityWheel / CR;
        float grdXSg = revolucionesXSg * 360f;
        wheel.transform.Rotate(dirWheel * grdXSg * Time.deltaTime);
        float velMax = _playerC.SprintSpeed;
        //normalizamos la velocidad dentro del rango 0 -1
        float velNorm = Mathf.InverseLerp(0, velMax, velocityWheel);

       
    }

}
