Shader "Tes/VertexColoured"
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
    LOD 200

    CGINCLUDE
    #pragma multi_compile __ WITH_COLOURS_UINT WITH_COLOURS_V4
    #pragma multi_compile __ WITH_NORMALS

    #include "UnityCG.cginc"

    uniform float4 _Color;
    uniform float4 _Tint;
    StructuredBuffer<float3> _Vertices;
    #ifdef WITH_NORMALS
    StructuredBuffer<float3> _Normals;
    #endif // WITH_NORMALS
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


    FragmentInput calcVert(float3 vertexPosition,
                          #ifdef WITH_NORMALS
                           float3 vertexNormal,
                          #endif // WITH_NORMALS
                          #if defined(WITH_COLOURS_UINT) || defined(WITH_COLOURS_V4)
                           float4 vertexColour,
                          #endif // defined(WITH_COLOURS_UINT) || defined(WITH_COLOURS_V4)
                           float4 faceColour)
    {
      FragmentInput o;
      o.vertex = UnityObjectToClipPos(vertexPosition);
      o.colour = _Tint * faceColour
        #if defined(WITH_COLOURS_UINT) || defined(WITH_COLOURS_V4)
          * vertexColour
        #endif // defined(WITH_COLOURS_UINT) || defined(WITH_COLOURS_V4)
        // #ifdef WITH_NORMALS
        //   * float4(ShadeVertexLights(o.vertex, vertexNormal), 1.0f)
        // #endif // WITH_NORMALS
        ;
      return o;
    }
    ENDCG

    Pass
    {
      Lighting On
      Cull Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      FragmentInput vert(uint vid : SV_VertexID)
      {
        return calcVert(_Vertices[vid],
          #ifdef WITH_NORMALS
            _Normals[vid],
          #endif // WITH_NORMALS
          #ifdef WITH_COLOURS_UINT
            float4((float)((_Colours[vid] >> 24) & 255) / 255.0f,
                           (float)((_Colours[vid] >> 16) & 255) / 255.0f,
                           (float)((_Colours[vid] >> 8) & 255) / 255.0f,
                           (float)(_Colours[vid] & 255) / 255.0f),
          #elif defined(WITH_COLOURS_V4)
            _Colours[vid],
          #endif // WITH_COLOURS_
            _Color);
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

      FragmentInput vert(uint vid : SV_VertexID)
      {
        return calcVert(_Vertices[vid],
          #ifdef WITH_NORMALS
            // Flip normal for the back face.
            -1.0f * _Normals[vid],
          #endif // _Tint
          #ifdef WITH_COLOURS_UINT
            _Colours[vid],
          #elif defined(WITH_COLOURS_V4)
            _Colours[vid],
          #endif // WITH_COLOURS_
          _Color);
      }

      float4 frag(FragmentInput i) : COLOR
      {
        return i.colour;
      }
      ENDCG
    }
  }
}
