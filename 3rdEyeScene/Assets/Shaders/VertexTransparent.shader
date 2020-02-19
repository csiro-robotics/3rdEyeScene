Shader "Tes/VertexTransparent"
{
  Properties
  {
    // Tint is for UI highlight. Colour is the primary colour.
    _Color("Main Colour", Color) = (1, 1, 1, 1)
    _Tint("Tint", Color) = (1, 1, 1, 1)
  }

  SubShader
  {
    Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
    LOD 200
    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite Off
    Cull Off

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile __ WITH_COLOURS_UINT WITH_COLOURS_V4

      #include "UnityCG.cginc"

      uniform float4 _Color;
      uniform float4 _Tint;
      StructuredBuffer<float3> _Vertices;
      #ifdef WITH_COLOURS_UINT
      StructuredBuffer<uint> _Colours;
      #endif // WITH_COLOURS_UINT
      #ifdef WITH_COLOURS_V4
      StructuredBuffer<float4> _Colours;
      #endif // WITH_COLOURS_V4

      struct FragmentInput
      {
        float4 vertex : SV_POSITION;
        float4 colour : COLOR;
      };

      FragmentInput vert(uint vid : SV_VertexID)
      {
        FragmentInput o;
        o.vertex = UnityObjectToClipPos(_Vertices[vid]);
        o.colour = _Color * _Tint
            #ifdef WITH_COLOURS_UINT
            * float4((float)((_Colours[vid] >> 24) & 255) / 255.0f,
                     (float)((_Colours[vid] >> 16) & 255) / 255.0f,
                     (float)((_Colours[vid] >> 8) & 255) / 255.0f,
                     (float)(_Colours[vid] & 255) / 255.0f)
            #endif // WITH_COLOURS_UINT
            #ifdef WITH_COLOURS_V4
            * _Colours[vid]
            #endif // WITH_COLOURS_V4
          ;
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
