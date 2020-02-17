Shader "VertexColour/VertexColoured"
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
    #include "UnityCG.cginc"

    uniform float4 _Color;
    uniform float4 _Tint;
    StructuredBuffer<float3> _Vertices;
    #ifdef WITH_NORMALS
    StructuredBuffer<float3> _Normals;
    #endif // WITH_NORMALS
    #ifdef WITH_COLOURS
    StructuredBuffer<float4> _Colours;
    #endif // WITH_COLOURS

    struct VertexInput
    {
      float4 vertex : POSITION;
      #ifdef WITH_NORMALS
      float4 normal : NORMAL;
      #endif // WITH_NORMALS
      #ifdef WITH_COLOURS
      float4 colour : COLOR;
      #endif // WITH_COLOURS
    };

    struct FragmentInput
    {
      float4 vertex : SV_POSITION;
      float4 colour : COLOR;
    };


    FragmentInput calcVert(float3 vertexPosition,
                          #ifdef WITH_NORMALS
                           float3 vertexNormal,
                          #endif // WITH_NORMALS
                          #ifdef WITH_COLOURS
                           float4 vertexColour,
                          #endif // WITH_COLOURS
                           float4 faceColour)
    {
      FragmentInput o;
      o.vertex = UnityObjectToClipPos(vertexPosition);
      o.colour = _Tint * faceColour
        #ifdef WITH_COLOURS
          * vertexColour
        #endif // WITH_COLOURS
        #ifdef WITH_NORMALS
           * float4(ShadeVertexLights(vertexPosition, vertexNormal), 1.0f)
        #endif // WITH_NORMALS
        ;
      return o;
    }
    ENDCG

    Pass
    {
      Lighting On

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      FragmentInput vert(uint vid : SV_VertexID)
      {
        return calcVert(_Vertices[vid],
          #ifdef WITH_NORMALS
            _Normals[vid],
          #endif // _Tint
          #ifdef WITH_COLOURS
            _Colours[vid],
          #endif // WITH_COLOURS
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
          #ifdef WITH_COLOURS
            _Colours[vid],
          #endif // WITH_COLOURS
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
