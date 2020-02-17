Shader "Tes/LeftHanded/Primitives"
{
  Properties
  {
    // Tint is for UI highlight. Colour is the primary colour.
    _Color("Colour", Color) = (1, 1, 1, 1)
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      Lighting On
      Cull Back

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing
      #include "UnityCG.cginc"

      struct VertexInput
      {
        float4 vertex : POSITION;
        float4 normal : NORMAL;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct FragmentInput
      {
        float4 vertex : SV_POSITION;
        float4 colour : COLOR;
      };

      UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
      UNITY_INSTANCING_BUFFER_END(Props)

      FragmentInput vert(VertexInput v)
      {
        FragmentInput o;
        UNITY_SETUP_INSTANCE_ID(v);
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.colour = UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * float4(ShadeVertexLights(v.vertex, v.normal), 1.0f);
        return o;
      }

      float4 frag(FragmentInput i) : COLOR
      {
        return i.colour;
      }
      ENDCG
    }
  }
}
