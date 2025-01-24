Shader "Hidden/Heatmap_Merge"
{
    Properties
    {
        _MainTex("Main Texture",2D)="black"{}
		_SceneTex("Scene Texture",2D)="black"{}
    }
    SubShader 
    {
		Tags { "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            CGPROGRAM
     
            sampler2D _MainTex;
			half4 _MainTex_ST;
			sampler2D _SceneTex;
			half4 _SceneTex_ST;

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
             
            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uvs : TEXCOORD0;
            };
             
            v2f vert (appdata_base v) 
            {
                v2f o;
                 
                //Despite the fact that we are only drawing a quad to the screen, Unity requires us to multiply vertices by our MVP matrix, presumably to keep things working when inexperienced people try copying code from other shaders.
                o.pos = UnityObjectToClipPos(v.vertex);
                 
                //Also, we need to fix the UVs to match our screen space coordinates. There is a Unity define for this that should normally be used.
                o.uvs = o.pos.xy / 2 + 0.5;
				o.uvs.y = 1 - (o.pos.y / 2 + 0.5);
                 
                return o;
            }
             
             
            half4 frag(v2f i) : COLOR 
            {
				//if(tex2D(_MainTex,i.uvs.xy).g>0)
                //{
				//	half4 mainCol = tex2D(_MainTex, i.uvs.xy);
                //    return tex2D(_SceneTex, i.uvs.xy) + half4(mainCol.x, mainCol.y/5, mainCol.z, 1.0f);
                //}
                //return tex2D(_SceneTex, i.uvs.xy);

				return lerp(tex2D(_SceneTex, i.uvs.xy), tex2D(_MainTex, i.uvs.xy), 0.3f);
            }
            ENDCG
        }
        //end pass    
    }
    //end subshader
}
//end shader