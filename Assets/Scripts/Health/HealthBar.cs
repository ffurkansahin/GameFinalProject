using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image maxHealthBar;
    [SerializeField] private Image currentHealthBar;

    void Start()
    {
        currentHealthBar.fillAmount = playerHealth.currentHealth / 10;
    }
    void Update()
    {
        currentHealthBar.fillAmount = playerHealth.currentHealth / 10;
    }
}
