using UnityEngine;

public class ParticleRendering : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] Mesh mesh;
    [SerializeField] Shader particleInstancingShader;
    [SerializeField] float scale;
    [SerializeField] Color color;

    Material material;
    ComputeBuffer meshInstanceBuffer;
    Bounds bounds;

    public void SendDataToParticleInstancing(FluidSimulation2 fluidSimulation,FluidInitializer fluidInitializer)
    {
        //OJO
        bounds.max = new Vector2(5000, 5000);
        bounds.min = new Vector2(-5000,-5000);

        material = new Material(particleInstancingShader);
        material.SetBuffer("Particles", fluidSimulation.particles);

        uint[] meshInstanceData = new uint[5] { 0, 0, 0, 0, 0 };

        // Argumentos para el dibujo indirecto: mesh vertex count, instance count, start vertex, start instance
        meshInstanceData[0] = mesh.GetIndexCount(0);
        meshInstanceData[1] = (uint)fluidSimulation.particles.count;
        meshInstanceData[2] = mesh.GetIndexStart(0);
        meshInstanceData[3] = mesh.GetBaseVertex(0);
        meshInstanceBuffer = new ComputeBuffer(1, meshInstanceData.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        meshInstanceBuffer.SetData(meshInstanceData);

    }

    void Update()
    {
        material.SetFloat("_Scale",scale);
        material.SetColor("_Color", color);
        //GraphicsBufferHandle id = material.GetBuffer("Particles");
        //0 => subset of the mesh but since there is only one material
        //This bounds is needed for the shader knowing the bounding volume surrounding the instances you intend to draw.
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, meshInstanceBuffer);
    }

    void OnDestroy()
    {
        meshInstanceBuffer.Release();
    }

}
