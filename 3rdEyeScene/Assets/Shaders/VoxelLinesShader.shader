// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Points/VoxeLines"
{
  Properties
  {
    _Color("Main Colour", Color) = (1, 1, 1, 1)
    _Tint("Tint", Color) = (1, 1, 1, 1)
  }

  SubShader
  {
    Pass
    {
      Tags{ "Queue" = "Opaque" "RenderType" = "Opaque" }
      LOD 200

      CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma geometry geom
#include "UnityCG.cginc"

      // **************************************************************
      // Data structures                        *
      // **************************************************************
      struct VertexInput
      {
        float4 vertex : POSITION;
        float4 halfExtents : NORMAL;
        float4 colour : COLOR;
      };

      struct GeometryInput
      {
        float4 pos : POSITION;
        float3 halfExtents : NORMAL;
        float4 colour : COLOR;
      };

      struct FragmentInput
      {
        float4 pos : POSITION;
        float4 colour : COLOR;
      };

      // User properites
      uniform float4 _Color;
      uniform float4 _Tint;

      // **************************************************************
      // Shader Programs                        *
      // **************************************************************

      // Vertex Shader ------------------------------------------------
      GeometryInput vert(VertexInput v)
      {
        GeometryInput o;
        o.pos = mul(unity_ObjectToWorld, v.vertex);
        o.halfExtents = v.halfExtents;
        o.colour = v.colour * _Color * _Tint;
        return o;
      }

      // Geometry Shader -----------------------------------------------------
      // Generates a cube
#define VERTCOUNT 18
      [maxvertexcount(VERTCOUNT)]
      void geom(point GeometryInput p[1], inout LineStream<FragmentInput> outStream)
      {
        FragmentInput fin;
        const float3 voxelCentre = p[0].pos;
        //const half3 halfExt = half3(0.05f, 0.05f, 0.05f);// p[0].halfExtents;
        const half3 halfExt = p[0].halfExtents;
        float3 vert;

        fin.colour = p[0].colour;

        // Voxel base.
        vert = voxelCentre + half3(-halfExt.x, -halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);
        vert = voxelCentre + half3( halfExt.x, -halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        vert = voxelCentre + half3( halfExt.x, -halfExt.y,  halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        vert = voxelCentre + half3(-halfExt.x, -halfExt.y,  halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        vert = voxelCentre + half3(-halfExt.x, -halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        outStream.RestartStrip();

        // Voxel top.
        vert = voxelCentre + half3(-halfExt.x,  halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        vert = voxelCentre + half3( halfExt.x,  halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        vert = voxelCentre + half3( halfExt.x, halfExt.y,  halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        vert = voxelCentre + half3(-halfExt.x, halfExt.y,  halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);
        vert = voxelCentre + half3(-halfExt.x, halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        outStream.RestartStrip();

        // Connections.
        vert = voxelCentre + half3(-halfExt.x, -halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);
        vert = voxelCentre + half3(-halfExt.x,  halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        outStream.RestartStrip();

        vert = voxelCentre + half3( halfExt.x, -halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);
        vert = voxelCentre + half3( halfExt.x, halfExt.y, -halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        outStream.RestartStrip();

        vert = voxelCentre + half3( halfExt.x, -halfExt.y,  halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);
        vert = voxelCentre + half3( halfExt.x, halfExt.y,  halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);

        outStream.RestartStrip();

        vert = voxelCentre + half3(-halfExt.x, -halfExt.y, halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);
        vert = voxelCentre + half3(-halfExt.x, halfExt.y, halfExt.z);
        fin.pos = mul(UNITY_MATRIX_VP, float4(vert, 1));
        outStream.Append(fin);
      }

      // Fragment Shader -----------------------------------------------
      float4 frag(FragmentInput input) : COLOR
      {
        if (input.colour.a == 0)
        {
          discard;
        }
        return input.colour;
      }

        // Fragment Shader -----------------------------------------------
      float4 frag_depth(FragmentInput input) : COLOR
      {
        if (input.colour.a == 0)
        {
          discard;
        }
      return input.colour;
      }

      ENDCG
    }
  }
}
