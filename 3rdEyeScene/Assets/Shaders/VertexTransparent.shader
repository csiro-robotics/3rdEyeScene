Shader "VertexColour/VertexTransparent"
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

    Pass
    {
      CGPROGRAM
#pragma vertex vert
#pragma fragment frag

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

      FragmentInput vert(VertexInput v)
      {
        FragmentInput o;
        o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
        o.colour = _Color * _Tint * v.colour;
        return o;
      }

      float4 frag(FragmentInput i) : COLOR
      {
        return i.colour;
        //return float4(0, 1, 1, 0.5f);
      }
      ENDCG
    }
  }
}
