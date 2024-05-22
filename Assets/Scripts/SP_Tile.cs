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
    public float width = 0.8f;
    public float height = 0.8f;
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
