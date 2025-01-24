Shader "Hidden/Heatmap_Simple"
{

    SubShader 
    {
		//Tags { "Queue"="Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha

        ZWrite On
        ZTest LEqual
        Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex VShader
            #pragma fragment FShader
 
            struct VertexToFragment
            {
                float4 pos: POSITION;
            };
 
            //just get the position correct
            VertexToFragment VShader(VertexToFragment i)
            {
                VertexToFragment o;
                o.pos=UnityObjectToClipPos(i.pos);
                return o;
            }
 
            //return white
            half4 FShader():COLOR0
            {
                return half4(0,1,0,0.5);
            }
 
            ENDCG
        }
    }
}