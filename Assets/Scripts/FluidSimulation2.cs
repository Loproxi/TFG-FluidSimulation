using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulation2 : MonoBehaviour
{
    [Header("Particle Related")]
    public FluidParticle fluidParticle;
    private FluidParticle[] _particles;
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

    [Header("Compute Shader Related")]
    public ComputeShader compute;
    private ComputeBuffer particles;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
