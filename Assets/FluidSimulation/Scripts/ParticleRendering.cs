using UnityEngine;

public class ParticleRendering : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] Mesh mesh;
    [SerializeField] Shader particleInstancingShader;
    [Range(0.1f, 30.0f)]
    [SerializeField] float scale;
    [SerializeField] Color color;

    Material material;
    GraphicsBuffer commandBuffer;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    const int commandCount = 1;  // Number of commands
    Bounds bounds;

    public void SendDataToParticleInstancing(ComputeBuffer particles)
    {
        bounds.max = new Vector2(5000, 5000);
        bounds.min = new Vector2(-5000, -5000);

        material = new Material(particleInstancingShader);
        material.SetBuffer("Particles", particles);

        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];

        // Initialize the indirect command data
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)particles.count;
        commandData[0].startIndex = 0;
        commandData[0].baseVertexIndex = 0;
        commandData[0].startInstance = 0;

        // Set the data to the command buffer
        commandBuffer.SetData(commandData);
    }

    void Update()
    {
        material.SetFloat("_Scale", scale);
        material.SetColor("_Color", color);

        RenderParams rp = new RenderParams(material);
        rp.worldBounds = bounds; // use tighter bounds for better FOV culling

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, commandBuffer);
    }

    void OnDestroy()
    {
        commandBuffer?.Release();
        commandBuffer = null;
    }
}