using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tools
{
    
    public static float Ver_1_SmoothDensityKernel(float radius, float dist)
    {
        return Mathf.Max(0, radius - dist);
    }

    public static float Ver_2_SmoothDensityKernel(float radius, float dist)
    {
        float volume = Mathf.PI * Mathf.Pow(radius, 8) / 4; 
        float smoothvalue = Mathf.Max(0, radius - dist);

        return smoothvalue * smoothvalue * smoothvalue / volume;
    }

    //This function returns the slope of the smooth density Kernel V2 
    public static float Derivative_Ver_2_SmoothDensityKernel(float radius, float dist)
    {
        if (dist >= radius)
            return 0;

        float f = radius - dist;
        float scale = -20 / (Mathf.PI * Mathf.Pow(radius, 8));

        return scale * dist * f * f;
    }

    public static float Ver_3_SmoothDensityKernel(float radius, float dist)
    {
        float volume = Mathf.PI * Mathf.Pow(radius, 8) / 4;
        float smoothvalue = Mathf.Max(0, radius * radius - dist * dist);

        return smoothvalue * smoothvalue * smoothvalue / volume;
    }

}
