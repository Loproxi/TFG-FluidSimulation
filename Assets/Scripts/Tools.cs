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

    public static float Ver_2_SmoothDerivativeKernel(float radius, float dist)
    {
        float volume = Mathf.PI * Mathf.Pow(radius, 8) / 4;
        float smoothvalue = Mathf.Max(0, radius - dist);

        // Derivada de smoothvalue con respecto a dist
        float derivative_smoothvalue = -3 * smoothvalue * smoothvalue / volume;

        return smoothvalue * smoothvalue * smoothvalue / volume;
    }

    public static float Ver_3_SmoothDensityKernel(float radius, float dist)
    {
        float volume = Mathf.PI * Mathf.Pow(radius, 8) / 4;
        float smoothvalue = Mathf.Max(0, radius * radius - dist * dist);

        return smoothvalue * smoothvalue * smoothvalue / volume;
    }

}
