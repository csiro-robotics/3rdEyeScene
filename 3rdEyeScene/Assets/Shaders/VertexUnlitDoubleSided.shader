// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VertexColour/VertexUnlitDoubleSided"
{
  Properties
  {
    // Tint is for UI highlight. Colour is the primary colour.
    _Color("Main Colour", Color) = (1, 1, 1, 1)
    _Tint("Tint", Color) = (1, 1, 1, 1)
    _BackColour("Back Face Colour", Color) = (0.5, 0.5, 0.5, 0.5)
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    CGINCLUDE
    #include "UnityCG.cginc"

    uniform float4 _Color;
    uniform float4 _Tint;

    // Note: we do lighting in the vertex shader because it is uniform across the point quad.
    struct VertexInput
    {
      float4 vertex : POSITION;
      float4 colour : COLOR;
    };

    struct FragmentInput
    {
      float4 vertex : SV_POSITION;
      float4 colour : COLOR;
    };
    ENDCG

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      FragmentInput vert(VertexInput v)
      {
        FragmentInput o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.colour = _Color * _Tint * v.colour;
        return o;
      }

      float4 frag(FragmentInput i) : COLOR
      {
        return i.colour;
      }
      ENDCG
    }

    Pass
    {
      // Back face rendering.
      Cull Front

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      uniform float4 _BackColour;

      FragmentInput vert(VertexInput v)
      {
        FragmentInput o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.colour = _BackColour * _Tint * v.colour;
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
