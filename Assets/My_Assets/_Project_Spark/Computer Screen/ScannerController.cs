using System;
using UnityEngine;
using UnityEngine.UI;

public class ScannerController : MonoBehaviour
{
    [Tooltip("Scanner Material")]
    [SerializeField] Material material;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Image image;

    

    private void Awake()
    {
       
        material = Instantiate(image.material);
        image.material = material;
        //Debug.Log(image.material.shader.name);
    }
    private void Update()
    {
        float p = Mathf.PingPong(Time.time, 1f);

        material.SetFloat("_SweepPosition", p);
    }

    private void OnEnable()
    {
        material.SetFloat("_ScanEnabled", 1);

        material.SetFloat("_Scan", 2);

        material.SetFloat("_Sweep", 3);

        material.SetFloat("_SweepPosition", 0);
        Debug.Log(material.GetFloat("_SweepPosition"));
    }

  

    private void OnDisable()
    {
        material.SetFloat("_ScanEnabled", 0);

        material.SetFloat("_Scan", 0);

        material.SetFloat("_Sweep", 0);

        material.SetFloat("_Flash", 1);
    }
}
