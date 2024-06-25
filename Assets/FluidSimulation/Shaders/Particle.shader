Shader "Custom/Particle"
{
    Properties
    {
    }
    SubShader
    {
        ZWrite Off
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct FluidParticleData
            {
                float2 position; // 8
                float2 nextPosition; // 8
                float2 velocity; // 8
                float mass; // 4
                float density; // 4
                float nearDensity; // 4
            };

            StructuredBuffer<FluidParticleData> Particles;
            float _Scale;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                InitIndirectDrawArgs(0);
                uint cmdID = GetCommandID(0);
                uint indirectInstanceID = GetIndirectInstanceID(instanceID);

                // Obtener la posición de la partícula usando el ID de la instancia
                float2 particlePosition = Particles[indirectInstanceID].position;
                float3 worldPosition = float3(particlePosition, 0.0);

                // Escalar la partícula
                float3 scaledVertex = v.vertex.xyz * _Scale;

                // World To clip space
                o.vertex = UnityObjectToClipPos(float4(worldPosition + scaledVertex, 1.0));

                // Pasar los datos al fragment shader
                o.uv = v.uv;
                o.color = _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
