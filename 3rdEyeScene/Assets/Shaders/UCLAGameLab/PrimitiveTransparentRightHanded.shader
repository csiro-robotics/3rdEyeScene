Shader "Tes/RightHanded/PrimitivesTransparent"
{
  Properties
  {
    // Tint is for UI highlight. Colour is the primary colour.
    _Color("Colour", Color) = (1, 1, 1, 1)
    _FlatShaded("Flat shaded", Range(0, 1)) = 0
  }

  SubShader
  {
    // Tags { "RenderType" = "Opaque" }
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass
    {
      Lighting On
      Cull Front

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing
      #include "UnityCG.cginc"
      #include "Primitives.cginc"
      ENDCG
    }
  }
}
