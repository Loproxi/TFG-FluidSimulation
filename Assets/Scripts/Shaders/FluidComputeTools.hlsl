const float PI = 3.14159265359f;

int2 GetCellFromPosition(float2 position, float smoothingRadius)
{
    int2 cellCoord;
    cellCoord.x = int(floor(position.x / smoothingRadius));
    cellCoord.y = int(floor(position.y / smoothingRadius));

    return cellCoord;
}

uint HashingCell(int2 cell)
{
    uint cellHashed = 0;

    uint p1 = (uint) cell.x * 73856093; // Prime Numbers
    uint p2 = (uint) cell.y * 19349663; // Prime Numbers

    cellHashed = p1 ^ p2;

    return cellHashed;
}

uint GetKeyFromHashedCell(uint cellHashed,uint numOfParticles)
{
    uint key = 0;

    key = cellHashed % numOfParticles;

    return key;
}

void SelectSurroundingCells(float2 particlePosition, float smoothingRadius, out int2 nearCells[9])
{

    int2 centerCell = GetCellFromPosition(particlePosition, smoothingRadius);

    //nearCells[0] -> contains the particle position cell AKA -> the center one
    //nearCells[1-8] -> the near ones
    nearCells[0] = centerCell; // Centro
    nearCells[1] = centerCell + int2(1, 0); //Right
    nearCells[2] = centerCell + int2(1, -1);
    nearCells[3] = centerCell + int2(0, -1); // Bottom
    nearCells[4] = centerCell + int2(-1, -1);
    nearCells[5] = centerCell + int2(-1, 0); // Left
    nearCells[6] = centerCell + int2(-1, 1);
    nearCells[7] = centerCell + int2(0, 1); // Up
    nearCells[8] = centerCell + int2(1, 1);
    
    //Once we have all the keys we can use it to go to the secondary list of indices and iterate for each particle if it is inside of the smoothing radius
}

float Ver_1_SmoothNearDensityKernel(float radius, float dist)
{
    if (dist < radius)
    {
        float volume = 10 / (PI * pow(radius, 5));
        return (radius - dist) * (radius - dist) * (radius - dist) * volume;
    }
    return 0.0f;
}
    
float Derivative_Ver_1_SmoothNearDensityKernel(float radius, float dist)
{
    if (dist <= radius)
    {

        float volume = 30 / (pow(radius, 5) * PI);
        return -(radius - dist) * (radius - dist) * volume;

    }
    return 0;
}
    
float Ver_2_SmoothDensityKernel(float radius, float dist)
{
    if (dist < radius)
    {

        float volume = 6 / (PI * pow(radius, 4));
        return (radius - dist) * (radius - dist) * volume;

    }
    return 0;
}
   //This function returns the slope of the smooth density Kernel V2 
    
float Derivative_Ver_2_SmoothDensityKernel(float radius, float dist)
{
    if (dist <= radius)
    {

        float volume = 12 / (pow(radius, 4) * PI);
        return -(radius - dist) * volume;

    }
    return 0;
}

float Ver_3_SmoothDensityKernel(float radius, float dist)
{
    float volume = PI * pow(radius, 8) / 4;
    float smoothvalue = max(0, radius * radius - dist * dist);

    return smoothvalue * smoothvalue * smoothvalue / volume;
}

float Dot(float2 a,float2 b)
{
    float dotProduct = 0.0f;
    dotProduct = a.x * b.x;
    dotProduct += a.y * b.y;
    return dotProduct;
}

float ConvertDensityIntoPressure(float density,float restDensity,float gasConstant)
{
        //If the rest density is achieved particle won't generate pressure
    float pressure = (density - restDensity) * gasConstant;
    return pressure;
}

float ConvertNearDensityIntoPressure(float density, float nearDensityMult)
{
    float pressure = density * nearDensityMult;
    return pressure;
}