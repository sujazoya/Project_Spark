using UnityEngine;
using TMPro;
using System;

public class SystemClock : MonoBehaviour
{
    [Header("Clock Text")]
    public TMP_Text clockText;

    [Header("24 Hour Format")]
    public bool use24HourFormat = false;

    [Header("Show Seconds")]
    public bool showSeconds = true;

    private void Update()
    {
        DateTime localTime = DateTime.Now;

        if (use24HourFormat)
        {
            if (showSeconds)
                clockText.text = localTime.ToString("HH:mm:ss");
            else
                clockText.text = localTime.ToString("HH:mm");
        }
        else
        {
            if (showSeconds)
                clockText.text = localTime.ToString("hh:mm:ss tt");
            else
                clockText.text = localTime.ToString("hh:mm tt");
        }
    }
}