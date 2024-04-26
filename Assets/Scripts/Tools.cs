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
        if (dist >= radius)
            return 0;

        float volume = Mathf.PI * Mathf.Pow(radius, 4) / 6; 

        return (radius - dist) * (radius - dist) / volume;
    }

    //This function returns the slope of the smooth density Kernel V2 
    public static float Derivative_Ver_2_SmoothDensityKernel(float radius, float dist)
    {
        if (dist >= radius)
            return 0;

        float scale = 12 / (Mathf.Pow(radius, 4) * Mathf.PI);

        return (dist - radius) * scale;
    }

    public static float Ver_3_SmoothDensityKernel(float radius, float dist)
    {
        float volume = Mathf.PI * Mathf.Pow(radius, 8) / 4;
        float smoothvalue = Mathf.Max(0, radius * radius - dist * dist);

        return smoothvalue * smoothvalue * smoothvalue / volume;
    }

    public static float Derivative_Ver_3_SmoothDensityKernel(float radius, float dist)
    {
        if (dist >= radius)return 0;

        float f = radius * radius - dist * dist;
        float scale = -24 / (Mathf.PI * Mathf.Pow(radius, 8));

        return scale * dist * f * f;
    }

}
