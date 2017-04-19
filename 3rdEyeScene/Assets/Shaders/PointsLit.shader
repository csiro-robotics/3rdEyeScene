// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Points/PointsLit"
{
  Properties
  {
    _Color("Main Colour", Color) = (1, 1, 1, 1)
    _Tint("Tint", Color) = (1, 1, 1, 1)
    _PointSize("Point Size", Range(1, 64)) = 2
    _PointHighlighting("Point Highlighting", Range(0, 1)) = 1
  }

  CGINCLUDE
#include "UnityCG.cginc"

  // **************************************************************
  // Data structures                        *
  // **************************************************************
  struct VertexInput
  {
    float4 vertex : POSITION;
    float4 normal : NORMAL;
    float4 colour : COLOR;
  };

  struct GeometryInput
  {
    float4 pos : POSITION;
    float3 normal : NORMAL;
    float4 colour : COLOR;
  };

  struct FragmentInput
  {
    float4 pos : POSITION;
    float2 tex0 : TEXCOORD0;
    float4 colour : COLOR;
  };

  // User properties.
  uniform float _PointSize;
  uniform float4 _Color;
  uniform float4 _Tint;
  uniform int _PointHighlighting;

  // **************************************************************
  // Shader Programs                        *
  // **************************************************************

  // Vertex Shader ------------------------------------------------
  GeometryInput vert(VertexInput v)
  {
    GeometryInput o;
    o.pos = mul(unity_ObjectToWorld, v.vertex);
    o.normal = mul(UNITY_MATRIX_MV, float4(v.normal, 0.0f));
    o.colour = v.colour;
    return o;
  }

  // Geometry Shader -----------------------------------------------------
  [maxvertexcount(4)]
  void geom(point GeometryInput p[1], inout TriangleStream<FragmentInput> triStream)
  {
    // Minimum point size. Greater than one due to potential floating point error.
    // Will generally equate to not overflowing into adjacent pixels anyway.
    const float MinScale = 1.5f;
    FragmentInput fin;

    // _ScreenParams:
    // x is the current render target width in pixels
    // y is the current render target height in pixels
    // z is 1.0 + 1.0/width
    // w is 1.0 + 1.0/height.
    const float4 ppos = mul(UNITY_MATRIX_VP, float4(p[0].pos.xyz, 1));
    const float depth = ppos.w;
    const float size = 0.5f * max(0.5f + _PointSize * (1 + depth), MinScale * ppos.w) * (_ScreenParams.w - 1.0f);
    const float3 right = mul(UNITY_MATRIX_VP, UNITY_MATRIX_V[0].xyz * size);
    const float3 up = mul(UNITY_MATRIX_VP, UNITY_MATRIX_V[1].xyz * size);

    fin.pos = ppos - float4((right + up), 0);
    fin.colour = p[0].colour;
    fin.tex0 = float2(0, 0);
    triStream.Append(fin);

    fin.pos = ppos + float4((right - up), 0);
    fin.tex0 = float2(1, 0);
    triStream.Append(fin);

    fin.pos = ppos - float4((right - up), 0);
    fin.tex0 = float2(0, 1);
    triStream.Append(fin);

    fin.pos = ppos + float4((right + up), 0);
    fin.tex0 = float2(1, 1);
    triStream.Append(fin);
  }

  // Fragment Shader -----------------------------------------------
  float4 frag(FragmentInput input) : COLOR
  {
    const float2 uvoffset = input.tex0 - float2(0.5f, 0.5f);
    const float uvdist2 = uvoffset.x * uvoffset.x + uvoffset.y * uvoffset.y;
    // Drop points outside the point size radius.
    // Use 0.25 because point radius has ended up being 0.5 (from uvoffset)
    // and we need radius squared (0.25)
    if (uvdist2 > 0.25f || input.colour.a == 0)
    {
      discard;
    }
    // Colour scale is set by 1 - pow(pixel radius, 2) / pow(max radius, 2);
    // The max radius is 0.5, which yields a division by 0.25, which is the same
    // as a multiplication by 4. uvdist2 is already pow(pixel radius, 2)
    const float scale = (_PointHighlighting) ? 1.0f - uvdist2 * 4.0f : 1.0f;
    return scale * _Color * _Tint * input.colour;
  }

  // Fragment Shader -----------------------------------------------
  float4 frag_depth(FragmentInput input) : COLOR
  {
    const float2 uvoffset = input.tex0 - float2(0.5f, 0.5f);
    const float uvdist2 = uvoffset.x * uvoffset.x + uvoffset.y * uvoffset.y;
    // Drop points outside the point size radius.
    // Use 0.25 because point radius has ended up being 0.5 (from uvoffset)
    // and we need radius squared (0.25)
    if (uvdist2 > 0.25f || input.colour.a == 0)
    {
      discard;
    }
    return input.colour;
  }

  ENDCG

  SubShader
  {
//    // FIXME: Avoid using geometry shader twice just to get EDL working. Help with render to texture?
//    Pass
//    {
//      Tags{ "RenderType" = "Opaque" "LightMode" = "ShadowCaster" "Queue" = "Geometry" }
//      LOD 200
//
//      CGPROGRAM
//#pragma vertex vert
//#pragma fragment frag_depth
//#pragma geometry geom
//      ENDCG
//    }

    Pass
    {
      //Tags {"RenderType" = "Opaque" "LightMode" = "ShadowCaster" }
      Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry" }
      LOD 200

      CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma geometry geom
      ENDCG
    }
  }
}
