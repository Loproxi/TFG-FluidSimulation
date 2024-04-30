using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SP_Tile : MonoBehaviour
{
    public Vector2 position;
    public float width, height;

    private void Start()
    {
        SetValues();
    }

    private void SetValues()
    {
        position = gameObject.transform.position;
        gameObject.transform.localScale = new Vector2(width, height);
    }
}
