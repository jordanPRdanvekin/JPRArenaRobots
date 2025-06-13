using System;
using TMPro;
using UnityEngine;

public class energySystem : MonoBehaviour
{
    public static energySystem instance;

    [SerializeField]
    private float startingEnergy;

    [SerializeField]
    private float energyPerSecond;
    [SerializeField]
    private float energyShot;

    [SerializeField]
    private float maxEnergy = 100f;

    [SerializeField]
    private float infiniteEnergyDuration;
    [SerializeField]
    private string infiniteEnergyText;

    private float infiniteEnergyCounter = 0f;

    private float currentEnergy;

    private bool infiniteEnergy = false;

    [SerializeField]
    private TextMeshProUGUI energyLabel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        currentEnergy = startingEnergy;
        UpdateEnergyText();
    }

    void Update()
    {
        if (infiniteEnergy)
        {
            EnergyCounter();
        }

        AutomaticEnergyReload();
    }

    void EnergyCounter()
    {
        infiniteEnergyCounter += Time.deltaTime;

        if (infiniteEnergyCounter >= infiniteEnergyDuration)
        {
            infiniteEnergyCounter = 0f;
            infiniteEnergy = false;
            UpdateEnergyText();
        }
    }

    void AutomaticEnergyReload()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += Time.deltaTime * energyPerSecond;
            UpdateEnergyText();
        }
    }

    public void ConsumeShotEnergy()
    {
        if (!infiniteEnergy)
        {
            currentEnergy -= energyShot;
            UpdateEnergyText();
        }
    }

    void UpdateEnergyText()
    {
        if (!infiniteEnergy)
        {
            energyLabel.text = currentEnergy.ToString("energy = "+"00");
        }
    }

    public bool HasEnoughEnergy()
    {
        if (currentEnergy >= energyShot)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void InfiniteEnergyPowerUp()
    {
        infiniteEnergy = true;
        energyLabel.text = infiniteEnergyText;
        currentEnergy = maxEnergy;
    }

    public void FullReloadPowerUp()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyText();
    }
}
