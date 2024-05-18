using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STATES
{
    SOLID,
    LIQUID,
    MAX
}

public class SP_Tile 
{
    public Vector2 position { get; private set; }
    public float width = 2.0f;
    public float height = 2.0f;
    public STATES state { get; private set;}

    public void UpdatePosition(Vector2 position)
    {
        this.position = position; 
    }

    public void UpdateState(STATES newState)
    {
        state = newState;
    }
}
