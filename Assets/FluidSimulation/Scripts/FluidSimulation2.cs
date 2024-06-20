using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 36)]
public struct FluidParticleData
{
    public Vector2 position; // 8
    public Vector2 nextPosition; // 8
    public Vector2 velocity; // 8
    public float mass; // 4
    public float density; // 4
    public float nearDensity; // 4

    public FluidParticleData(Vector2 pos, Vector2 velocity, float mass = 1.0f, float density = 0.0f, float nearDensity = 0.0f)
    {
        this.position = pos;
        this.nextPosition = pos;
        this.velocity = velocity;
        this.mass = mass;
        this.density = density;
        this.nearDensity = nearDensity;
    }
}

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 44)]
public struct FluidColliderData
{
    public Vector2 center; //8
    public float radius;   //4
    public Vector2 size;   //8
    public Vector3 rotation; //12
    public Vector2 scale; //8
    public int type; //4
}

public class FluidSimulation2 : MonoBehaviour
{
    [Header("Particle Related")]
    //private FluidParticle[] _particles;
    private FluidParticleData[] _particlesDataArray;
    private FluidInitializer _fluidInitializer;

    private float particleRadius = 0.0f;
    private float deltaTime = 0.0f;

    //public GameObject sphere;
    private List<IFluidCollider> colliders;
    private FluidColliderData[] _colliderDataArray;
    int numOfColliders;

    [Header("SPH Simulation Related")]
    [Range(0.0f, 5.0f)]
    public float smoothingDensityRadius = 1.0f;
    public float restDensity = 1.0f;
    public float fluidConstant = 2.0f;
    public float nearDensityConst = 5.0f;
    [Range(0.0f, 1.0f)]
    public float collisionDamping = 1.0f;
    public float gravity = -9.81f;
    public float viscosity = 0.0f;

    private ParticleRendering particleRendering;

    [Header("Compute Shader Related")]
    public ComputeShader compute;
    public ComputeBuffer particlesBuffer;
    private ComputeBuffer spatialHashingInfo; // Vector x = particleIndex Vector Y = cellkey
    private ComputeBuffer spatialHashingIndices;
    private ComputeBuffer collidersBuffer;

    //ID REFS TO FUNCTIONS IN COMPUTE
    int updateNextPositionKernel;
    int updateSpatialHashingInfoKernel;
    int sortSpatialHashingInfoKernel;
    int updateSpatialHashingIndicesKernel;
    int computeDensityKernel;
    int computePressureKernel;
    int computeViscosityKernel;
    int externalForcesKernel;


    void Start()
    {
        //Fill my particles array with data
        //FixSpawnParticles ON BIG NUMBERS
        InitializeSimulation();
        //Get Kernels ID
        FindKernelsInCompute();
        //Setting buffers to each kernel
        SetBufferOnKernels(particlesBuffer,"Particles",updateNextPositionKernel,updateSpatialHashingInfoKernel,computeDensityKernel,computePressureKernel,computeViscosityKernel,externalForcesKernel);
        SetBufferOnKernels(spatialHashingInfo, "SpatialHashingInfo", updateSpatialHashingInfoKernel,sortSpatialHashingInfoKernel, updateSpatialHashingIndicesKernel, computeDensityKernel, computePressureKernel, computeViscosityKernel);
        SetBufferOnKernels(spatialHashingIndices, "SpatialHashingIndices", updateSpatialHashingInfoKernel,updateSpatialHashingIndicesKernel, computeDensityKernel, computePressureKernel, computeViscosityKernel);
        if(collidersBuffer != null)
        {
            SetBufferOnKernels(collidersBuffer, "Colliders", externalForcesKernel);
        }

        particleRendering = gameObject.GetComponent<ParticleRendering>();
        particleRendering.SendDataToParticleInstancing(particlesBuffer);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateSimulation(Time.fixedDeltaTime);
    }

    void InitializeSimulation()
    {
        deltaTime = 1 / 60.0f;
        Time.fixedDeltaTime = deltaTime;

        _fluidInitializer = gameObject.GetComponent<FluidInitializer>();

        if (_fluidInitializer != null)
        {
            //Create Particle Buffer
            particlesBuffer = new ComputeBuffer(_fluidInitializer.numParticles, 36);
            spatialHashingInfo = new ComputeBuffer(_fluidInitializer.numParticles, 8);
            spatialHashingIndices = new ComputeBuffer(_fluidInitializer.numParticles, 4);

            _fluidInitializer.InitializeParticles();
            SpawnParticles();
            colliders = new List<IFluidCollider>();
            colliders.AddRange(FindObjectsOfType<FluidCircleCollider>(false));
            colliders.AddRange(FindObjectsOfType<FluidQuadCollider>(false));
            numOfColliders = colliders.Count;
            if (numOfColliders > 0)
            {
                collidersBuffer = new ComputeBuffer(numOfColliders, 44);
                _colliderDataArray = new FluidColliderData[numOfColliders];
                SetCollidersData();
            }
            else
            {
                // Create an empty buffer if no colliders are present
                collidersBuffer = new ComputeBuffer(1, 44);
                _colliderDataArray = new FluidColliderData[1];
                _colliderDataArray[0] = new FluidColliderData(); // Initialize with default values
                _colliderDataArray[0].type = 0;
                _colliderDataArray[0].radius = 0.0f;
                _colliderDataArray[0].center = new Vector2(0.0f, 0.0f);
                collidersBuffer.SetData(_colliderDataArray);
            }
        }
        else
        {
            Debug.Log("This simulation doesn't have the fluid initializer component");
        }
    }

    void SpawnParticles()
    {
        _particlesDataArray = new FluidParticleData[_fluidInitializer.numParticles];

        for (int i = 0; i < _fluidInitializer.numParticles; i++)
        {
            _particlesDataArray[i] = new FluidParticleData(_fluidInitializer.positions[i], Vector2.zero);
        }

        particlesBuffer.SetData(_particlesDataArray);
    }

    void UpdateSimulation(float dt)
    {

        UpdateComputeVariables(dt);

        OnDispatchComputeShader(_fluidInitializer.numParticles, updateNextPositionKernel);
        OnDispatchComputeShader(_fluidInitializer.numParticles, updateSpatialHashingInfoKernel);
        OnDispatchComputeShader(spatialHashingInfo.count/2, sortSpatialHashingInfoKernel);
        SortSpatialHashing(); 
        OnDispatchComputeShader(_fluidInitializer.numParticles, updateSpatialHashingIndicesKernel);
        OnDispatchComputeShader(_fluidInitializer.numParticles, computeDensityKernel);
        OnDispatchComputeShader(_fluidInitializer.numParticles, computePressureKernel);
        OnDispatchComputeShader(_fluidInitializer.numParticles, computeViscosityKernel);
        OnDispatchComputeShader(_fluidInitializer.numParticles, externalForcesKernel);

    }

    private void UpdateComputeVariables(float dt)
    {
        particleRadius = _fluidInitializer.particleScale/2;
        //Update the simulation Variables each frame
        compute.SetFloat("smoothingDensityRadius", smoothingDensityRadius);
        compute.SetFloat("collisionDamping", collisionDamping);
        compute.SetFloat("gasConstant", fluidConstant);
        compute.SetFloat("nearDensityConstant", nearDensityConst);
        compute.SetFloat("restDensity", restDensity);
        compute.SetFloat("gravity", gravity);
        compute.SetFloat("deltaTime", dt);
        compute.SetFloat("viscosity",viscosity);
        compute.SetVector("bounds",new Vector4(_fluidInitializer.minBounds.x, _fluidInitializer.minBounds.y, _fluidInitializer.maxBounds.x, _fluidInitializer.maxBounds.y));
        compute.SetFloat("particleRadius", particleRadius);
        compute.SetInt("numOfParticles", _fluidInitializer.numParticles);
        compute.SetInt("numOfColliders", numOfColliders);
        //I do this here because if this line is done on the .hlsl file density calculations are infinity
        compute.SetFloat("volumeSmoothDensity1", 10.0f / (Mathf.PI * Mathf.Pow(smoothingDensityRadius, 5)));
        compute.SetFloat("volumeSmoothDensity2", 6.0f / (Mathf.PI * Mathf.Pow(smoothingDensityRadius, 4)));
        compute.SetFloat("volumeSmoothNearPressure1", 30.0f / (Mathf.Pow(smoothingDensityRadius, 5.0f) * Mathf.PI));
        compute.SetFloat("volumeSmoothPressure2", 12.0f / (Mathf.Pow(smoothingDensityRadius, 4.0f) * Mathf.PI));
        compute.SetFloat("volumeSmoothViscosity3", 12.0f / Mathf.PI * Mathf.Pow(smoothingDensityRadius, 8.0f) / 4.0f);
        compute.SetInt("numEntries",spatialHashingInfo.count);

        SetCollidersData();
    }

    private void OnDestroy()
    {
        ReleaseBuffers(particlesBuffer,spatialHashingInfo,spatialHashingIndices,collidersBuffer);
    }

    private void SetCollidersData()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            _colliderDataArray[i] = colliders[i].GetColliderData();
        }

        if(collidersBuffer!=null)
        {
            collidersBuffer.SetData(_colliderDataArray);
        }
    }

    //BitonicSort from Sebastian Lague
    private void SortSpatialHashing()
    {

        int numStages = (int)Mathf.Log(Mathf.NextPowerOfTwo(spatialHashingInfo.count), 2);

        for (int stageIndex = 0; stageIndex < numStages; stageIndex++)
        {
            for (int stepIndex = 0; stepIndex < stageIndex + 1; stepIndex++)
            {
                // Calculate some pattern stuff
                int groupWidth = 1 << (stageIndex - stepIndex);
                int groupHeight = 2 * groupWidth - 1;
                compute.SetInt("groupWidth", groupWidth);
                compute.SetInt("groupHeight", groupHeight);
                compute.SetInt("stepIndex", stepIndex);
                // Run the sorting step on the GPU
                OnDispatchComputeShader(Mathf.NextPowerOfTwo(spatialHashingInfo.count) / 2, sortSpatialHashingInfoKernel);

            }
        }
    }

    #region ComputeShader

    void FindKernelsInCompute()
    {
        updateNextPositionKernel = compute.FindKernel("UpdateNextPositions");
        updateSpatialHashingInfoKernel = compute.FindKernel("UpdateSpatialHashingInfo");
        sortSpatialHashingInfoKernel = compute.FindKernel("SortSpatialHashingInfo");
        updateSpatialHashingIndicesKernel = compute.FindKernel("UpdateSpatialHashingIndices");
        computeDensityKernel = compute.FindKernel("ComputeDensity");
        computePressureKernel = compute.FindKernel("ComputePressure");
        computeViscosityKernel = compute.FindKernel("ComputeViscosity");
        externalForcesKernel = compute.FindKernel("ApplyForcesAndCollisions");
    }

    void SetBufferOnKernels(ComputeBuffer buffer,string nameID,params int[] kernels)
    {
        for (int i = 0; i < kernels.Length; i++)
        {
            compute.SetBuffer(kernels[i], nameID, buffer);
        }
    }

    void OnDispatchComputeShader(int numParticlesX, int kernelID = 0)
    {
        // How big are WORK GROUPS
        uint x,y,z;
        compute.GetKernelThreadGroupSizes(kernelID,out x,out y,out z);
        Vector3Int threadSizes = new Vector3Int((int)x,(int)y,(int)z);

        //Calculate number of threads depending on how many particles
        int numGroupsX = Mathf.CeilToInt(numParticlesX / (float)threadSizes.x);
        // HOW MANY WORK GROUPS DO WE NEED
        compute.Dispatch(kernelID, numGroupsX, 1, 1);

    }

    void ReleaseBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {

            if (buffers[i] != null)
            {
                buffers[i].Release();
            }
        }
    }

#endregion
}
