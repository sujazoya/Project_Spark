using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TigerAttack : MonoBehaviour
{
   public float erodeRate=0.03f;
   public float erodeRefreshRate=0.01f;
   public float erodeDelay=1.25f;
   public Renderer tigerRenderer;

    void Start()
    {
        StartCoroutine(ErodeTiger());
        
    }
    IEnumerator ErodeTiger()
    {
        yield return new WaitForSeconds(erodeDelay);
        float erodeAmount = 0f;
        Material mat = tigerRenderer.material;
        while (erodeAmount < 1f)
        {
            erodeAmount += erodeRate;
            mat.SetFloat("_Erode", erodeAmount);
            yield return new WaitForSeconds(erodeRefreshRate);
        }
        
    }
}
