using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tools
{
    
    public static float Ver_1_SmoothNearDensityKernel(float radius, float dist)
    {
        if(dist < radius)
        {
            float volume = 10 / (Mathf.PI * Mathf.Pow(radius, 5));
            return (radius - dist) * (radius - dist) * (radius - dist) * volume;
        }
        return 0.0f;
    }

    public static float Ver_2_SmoothDensityKernel(float radius, float dist)
    {
        if (dist < radius)
        {

            float volume = 6 / (Mathf.PI * Mathf.Pow(radius, 4));
            return (radius - dist) * (radius - dist) * volume;

        }
        return 0;
    }

    //This function returns the slope of the smooth density Kernel V2 
    public static float Derivative_Ver_2_SmoothDensityKernel(float radius, float dist)
    {
        if (dist <= radius)
        {

            float volume = 12 / (Mathf.Pow(radius, 4) * Mathf.PI);
            return -(radius - dist) * volume;

        }
        return 0;
    }

    public static float Ver_3_SmoothDensityKernel(float radius, float dist)
    {
        float volume = Mathf.PI * Mathf.Pow(radius, 8) / 4;
        float smoothvalue = Mathf.Max(0, radius * radius - dist * dist);

        return smoothvalue * smoothvalue * smoothvalue / volume;
    }

    public static float Derivative_Ver_3_SmoothNearDensityKernel(float radius, float dist)
    {
        if (dist <= radius)
        {

            float volume = 30 / (Mathf.Pow(radius, 5) * Mathf.PI);
            return -(radius - dist) * (radius - dist) * volume;

        }
        return 0;
    }

}
