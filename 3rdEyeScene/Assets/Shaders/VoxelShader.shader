// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Tes/Voxel"
{
  Properties
  {
    _Color("Main Colour", Color) = (1, 1, 1, 1)
    _Tint("Tint", Color) = (1, 1, 1, 1)
    _LineWidth("Line Width", Range(1, 16)) = 3.5
  }

  SubShader
  {
    Pass
    {
      Tags{ "Queue" = "Opaque" "RenderType" = "Opaque" }
      LOD 200
      Cull Off

      CGPROGRAM
#pragma target 4.0
#pragma vertex vert
#pragma fragment frag
#pragma geometry cubeGeom
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
        float4 edge : COLOR1;     // Edge coordinates. 1 or -1 on each face. Two must be 1/-1 for an edge.
        float2 edgeThreshold : TEXCOORD0; // Edge threshold factor.
      };

      // User properites
      uniform float4 _Color;
      uniform float4 _Tint;
      uniform float _LineWidth;
      StructuredBuffer<float3> _Vertices;
      // Normals used as half-extents. Named normals for input consistency.
      StructuredBuffer<float3> _Normals;
      #ifdef WITH_COLOURS
      StructuredBuffer<float4> _Colours;
      #endif // WITH_COLOURS

      // **************************************************************
      // Shader Programs                        *
      // **************************************************************

      // Vertex Shader ------------------------------------------------
      GeometryInput vert(uint vid : SV_VertexID)
      {
        GeometryInput o;
        o.pos = mul(unity_ObjectToWorld, float4(_Vertices[vid], 1));
        o.halfExtents = _Normals[vid];
        o.colour = _Color * _Tint
          #ifdef WITH_COLOURS
            * _Colours[vid]
          #endif // WITH_COLOURS
          ;
        return o;
      }

      // Geometry Shader -----------------------------------------------------
      // Generates a cube
      [maxvertexcount(24)]
      void cubeGeom(point GeometryInput p[1], inout TriangleStream<FragmentInput> outStream)
      {
        FragmentInput fin;
        const float3 voxelCentre = p[0].pos;
        // Minimum point size. Greater than one due to potential floating point error.
        // Will generally equate to not overflowing into adjacent pixels anyway.
        const float MinScale = 1.5f;
        const float ScreenHeight = (_ScreenParams.w - 1.0f);
        // const half3 halfExt = half3(0.05f, 0.05f, 0.05f);
        const half3 halfExt = p[0].halfExtents;
        float4 verts[8];
        float edgeThreshold[8];

        fin.colour = p[0].colour;
        fin.edge = float4(1, 1, 1, 1);
        fin.edgeThreshold = float2(1, 1);

        // Voxel base.
        verts[0] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3(-halfExt.x, -halfExt.y, -halfExt.z), 1));
        verts[1] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3( halfExt.x, -halfExt.y, -halfExt.z), 1));
        verts[2] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3( halfExt.x, -halfExt.y,  halfExt.z), 1));
        verts[3] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3(-halfExt.x, -halfExt.y,  halfExt.z), 1));

        verts[4] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3(-halfExt.x,  halfExt.y, -halfExt.z), 1));
        verts[5] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3( halfExt.x,  halfExt.y, -halfExt.z), 1));
        verts[6] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3( halfExt.x,  halfExt.y,  halfExt.z), 1));
        verts[7] = mul(UNITY_MATRIX_VP, float4(voxelCentre + half3(-halfExt.x,  halfExt.y,  halfExt.z), 1));

        // Technically the edgeThreshold factor should reflect the area of each face, not use just x/y.
        for (int i = 0; i < 8; ++i)
        {
          // edgeThreshold[i] = 2 * (1 - 0.5f * LineWidth * (1 + verts[i].w) * (_ScreenParams.w - 1.0f) / (0.5f * halfExt.x));
          edgeThreshold[i] = 2 * (1 - 0.5f * _LineWidth * (1 + verts[i].w) * (_ScreenParams.w - 1.0f) / (2 * halfExt.x));
        }

        // Voxel bottom.
        fin.pos = verts[0];
        fin.edge = float4(-1, -1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[0], 1);
        outStream.Append(fin);
        fin.pos = verts[1];
        fin.edge = float4(1, -1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[1], 1);
        outStream.Append(fin);
        fin.pos = verts[3];
        fin.edge = float4(-1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[3], 1);
        outStream.Append(fin);
        fin.pos = verts[2];
        fin.edge = float4(1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[2], 1);
        outStream.Append(fin);

        outStream.RestartStrip();

        // Voxel left.
        fin.pos = verts[0];
        fin.edge = float4(-1, -1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[0], 1);
        outStream.Append(fin);
        fin.pos = verts[3];
        fin.edge = float4(-1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[3], 1);
        outStream.Append(fin);
        fin.pos = verts[4];
        fin.edge = float4(-1, -1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[4], 1);
        outStream.Append(fin);
        fin.pos = verts[7];
        fin.edge = float4(-1, 1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[7], 1);
        outStream.Append(fin);

        outStream.RestartStrip();

        // Voxel back.
        fin.pos = verts[2];
        fin.edge = float4(1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[2], 1);
        outStream.Append(fin);
        fin.pos = verts[6];
        fin.edge = float4(1, 1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[6], 1);
        outStream.Append(fin);
        fin.pos = verts[3];
        fin.edge = float4(-1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[3], 1);
        outStream.Append(fin);
        fin.pos = verts[7];
        fin.edge = float4(-1, 1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[7], 1);
        outStream.Append(fin);

        outStream.RestartStrip();

        // Voxel right.
        fin.pos = verts[1];
        fin.edge = float4(1, -1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[1], 1);
        outStream.Append(fin);
        fin.pos = verts[5];
        fin.edge = float4(1, -1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[5], 1);
        outStream.Append(fin);
        fin.pos = verts[2];
        fin.edge = float4(1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[2], 1);
        outStream.Append(fin);
        fin.pos = verts[6];
        fin.edge = float4(1, 1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[6], 1);
        outStream.Append(fin);

        outStream.RestartStrip();

        // Voxel front.
        fin.pos = verts[0];
        fin.edge = float4(-1, -1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[0], 1);
        outStream.Append(fin);
        fin.pos = verts[4];
        fin.edge = float4(-1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[4], 1);
        outStream.Append(fin);
        fin.pos = verts[1];
        fin.edge = float4(1, -1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[1], 1);
        outStream.Append(fin);
        fin.pos = verts[5];
        fin.edge = float4(1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[5], 1);
        outStream.Append(fin);

        outStream.RestartStrip();

        // Voxel top.
        fin.pos = verts[5];
        fin.edge = float4(1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[5], 1);
        outStream.Append(fin);
        fin.pos = verts[4];
        fin.edge = float4(-1, 1, -1, 0);
        fin.edgeThreshold = float2(edgeThreshold[4], 1);
        outStream.Append(fin);
        fin.pos = verts[6];
        fin.edge = float4(1, 1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[6], 1);
        outStream.Append(fin);
        fin.pos = verts[7];
        fin.edge = float4(-1, 1, 1, 0);
        fin.edgeThreshold = float2(edgeThreshold[7], 1);
        outStream.Append(fin);

        outStream.RestartStrip();
      }

      [maxvertexcount(18)]
      void linesGeom(point GeometryInput p[1], inout LineStream<FragmentInput> outStream)
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

      static const float EdgeThreshold = 1.9f;

      float calcEdgeFactor(float4 edge)
      {
        float edgeFactor = abs(edge.x) + abs(edge.y);
        edgeFactor = max(abs(edge.x) + abs(edge.z), edgeFactor);
        edgeFactor = max(abs(edge.y) + abs(edge.z), edgeFactor);
        return edgeFactor;
      }

      // Fragment Shader -----------------------------------------------
      float4 frag(FragmentInput input) : COLOR
      {
        float edgeFactor = calcEdgeFactor(input.edge);
        edgeFactor *= input.colour.a;
        if (edgeFactor < input.edgeThreshold.x)
        {
          discard;
        }
        return input.colour;
      }

        // Fragment Shader -----------------------------------------------
      float4 frag_depth(FragmentInput input) : COLOR
      {
        float edgeFactor = calcEdgeFactor(input.edge);
        edgeFactor *= input.colour.a;
        if (edgeFactor < input.edgeThreshold.x)
        {
          discard;
        }
        return input.colour;
      }

      ENDCG
    }
  }
}
