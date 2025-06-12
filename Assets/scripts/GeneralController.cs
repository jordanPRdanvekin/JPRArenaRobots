using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GeneralController : MonoBehaviour
{
    public static GeneralController instance;
    
    [SerializeField] GameObject enemiesParent;
    int enemiesAmount;
    int totalEnemiesAmount;

    [SerializeField] TextMeshProUGUI enemiesText;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        enemiesAmount = enemiesParent.transform.childCount;
        totalEnemiesAmount = enemiesParent.transform.childCount;
        enemiesText.text = enemiesAmount.ToString("00") + " / " + totalEnemiesAmount.ToString("00");
    }

    public void CheckEnemies()
    {
        enemiesAmount--;
        enemiesText.text = enemiesAmount.ToString("00") + " / " + totalEnemiesAmount.ToString("00");
        if (enemiesAmount <= 0)
        {
            ReiniciarJuego.instance.WonGame();
        }
    }
}
