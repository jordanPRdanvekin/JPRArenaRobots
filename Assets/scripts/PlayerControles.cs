using UnityEngine;
using LeanTween;

/// <summary>
/// Hace que el objeto flotante persiga a la rueda con un ligero desfase usando LeanTween.
/// </summary>
public class PlayerControles : MonoBehaviour
{
    [SerializeField] private Transform rueda; // Referencia a la rueda
    [SerializeField] private float altura = 1f; // Altura a la que flota el objeto
    [SerializeField] private float suavizado = 0.1f; // Suavidad en el seguimiento

    void Update()
    {
        // Mover suavemente el objeto flotante a la posición de la rueda con LeanTween
        Vector3 posicionObjetivo = rueda.position + Vector3.up * altura;
        LeanTween.move(gameObject, posicionObjetivo, suavizado).setEase(LeanTweenType.easeOutSine);
    }
}


