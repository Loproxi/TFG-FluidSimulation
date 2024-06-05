using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
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
public class FluidSimulation2 : MonoBehaviour
{
    [Header("Particle Related")]
    //private FluidParticle[] _particles;
    private FluidParticleData[] _particlesDataArray;
    private FluidInitializer _fluidInitializer;

    private SP_Tile tile;
    private float deltaTime = 0.0f;
    private float predictiondeltaTime = 0.0f;

    //public GameObject sphere;
    private List<IFluidCollider> colliders;

    [Header("SPH Simulation Related")]
    [Range(0.0f, 1.0f)]
    public float smoothDensityRadius = 1.0f;
    public float restDensity = 1.0f;
    public float gasConstant = 2.0f;
    private float nearDensityMult = 5.0f;
    [Range(0.0f, 1.0f)]
    public float collisionDamping = 1.0f;
    public float gravity = -9.81f;
    public float viscosity = 0.0f;
    private CompactHashing compactHashing;

    public ParticleRendering particleRendering;

    [Header("Compute Shader Related")]
    public ComputeShader compute;
    public ComputeBuffer particles;
    private ComputeBuffer spatialHashingInfo; // Vector x = particleIndex Vector Y = cellkey
    private ComputeBuffer spatialHashingIndices;

    //ID REFS TO FUNCTIONS IN COMPUTE
    int updateNextPositionKernel;
    int spatialHashingKernel;
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
        SetBufferOnKernels(particles,"Particles",updateNextPositionKernel,spatialHashingKernel,computeDensityKernel,computePressureKernel,computeViscosityKernel,externalForcesKernel);

        particleRendering.SendDataToParticleInstancing(this, _fluidInitializer);
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
            particles = new ComputeBuffer(_fluidInitializer.numParticles, 36);

            _fluidInitializer.InitializeParticles();
            SpawnParticles();
            tile = new SP_Tile();
            compactHashing = new CompactHashing(_fluidInitializer.numParticles, tile.width, tile.height);
            colliders = new List<IFluidCollider>();
            colliders.AddRange(FindObjectsByType<FluidCircleCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
            colliders.AddRange(FindObjectsByType<FluidQuadCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
            colliders.AddRange(FindObjectsByType<FluidCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

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

        particles.SetData(_particlesDataArray);
    }

    void UpdateSimulation(float dt)
    {
        //Update the simulation Variables each frame
        compute.SetFloat("smoothingDensityRadius", smoothDensityRadius);
        compute.SetFloat("collisionDamping",collisionDamping);
        compute.SetFloat("gasConstant", gasConstant);
        compute.SetFloat("restDensity", restDensity);
        compute.SetFloat("deltaTime", dt);
    }

    private void OnDestroy()
    {
        ReleaseBuffers(particles,spatialHashingInfo,spatialHashingIndices);
    }

    #region ComputeShader

    void FindKernelsInCompute()
    {
        updateNextPositionKernel = compute.FindKernel("UpdateNextPositions");
        spatialHashingKernel = compute.FindKernel("UpdateSpatialHashing");
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
        // How big are thread groups
        uint x,y,z;
        compute.GetKernelThreadGroupSizes(kernelID,out x,out y,out z);
        Vector3Int threadSizes = new Vector3Int((int)x,(int)y,(int)z);

        //Calculate number of threads depending on how many particles
        int numGroupsX = Mathf.CeilToInt(numParticlesX / (float)threadSizes.x);
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
