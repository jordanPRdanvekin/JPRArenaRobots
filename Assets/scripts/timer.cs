using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class timer : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float maxTimer;
    public float currentTimer;

    float minutes;
    float seconds;

    void Start()
    {
        currentTimer = maxTimer;
    }

    void Update()
    {
        currentTimer -= Time.deltaTime;
        minutes = (int)((currentTimer % 3600) / 60);
        seconds = (int)(currentTimer % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (currentTimer <= 0f)
        {
            ReiniciarJuego.instance.LostGame();
        }
    }
}
