// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VertexColour/VertexLitDoubleSided"
{
  Properties
  {
    // Tint is for UI highlight. Colour is the primary colour.
    _Color("Main Colour", Color) = (1, 1, 1, 1)
    _Tint("Tint", Color) = (1, 1, 1, 1)
    _BackColour("Back Face Colour", Color) = (0.5, 0.5, 0.5, 0.5)
    //_MainTex("Diffuse (RGB)", 2D) = "white" {}
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    CGINCLUDE
    #include "UnityCG.cginc"

    uniform float4 _Color;
    uniform float4 _Tint;

    struct VertexInput
    {
      float4 vertex : POSITION;
      float4 normal : NORMAL;
      float4 colour : COLOR;
    };

    struct FragmentInput
    {
      float4 vertex : SV_POSITION;
      float4 colour : COLOR;
    };


    FragmentInput calcVert(VertexInput v, float4 faceColour, float normalScale)
    {
      FragmentInput o;
      o.vertex = UnityObjectToClipPos(v.vertex);
      o.colour = _Tint * v.colour * faceColour * float4(ShadeVertexLights(v.vertex, normalScale * v.normal), 1.0f);
      return o;
    }
    ENDCG

    Pass
    {
      Lighting On

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      FragmentInput vert(VertexInput v)
      {
        return calcVert(v, _Color, 1.0f);
      }

      float4 frag(FragmentInput i) : COLOR
      {
        return i.colour;
      }
      ENDCG
    }


    Pass
    {
      Lighting On
      // Back face rendering.
      Cull Front

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      uniform float4 _BackColour;

      FragmentInput vert(VertexInput v)
      {
        return calcVert(v, _BackColour, -1.0f);
      }

      float4 frag(FragmentInput i) : COLOR
      {
        return i.colour;
      }
      ENDCG
    }
  }
}
