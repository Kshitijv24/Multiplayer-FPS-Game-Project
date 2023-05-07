using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public TMP_Text overheatedMessage;
    public Slider weaponTemperatureSlider;

    private void Awake()
    {
        Instance = this;
    }
}
