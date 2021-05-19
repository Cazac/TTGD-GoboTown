Shader "Custom/Vertex Blend" {

	Properties{






	  _MainTex("Color r (RGB)", 2D) = "grey" {}
	  _MainTex1("Color g (RGB)", 2D) = "grey" {}
	  _MainTex2("Color b (RGB)", 2D) = "grey" {}
	  _MainTex3("Color a (RGB)", 2D) = "grey" {}

	}

		SubShader{

			  Tags { "RenderPipeline" = "UniversalPipeline" }

			  CGPROGRAM


				 #pragma surface surf Lambert vertex:vert

						sampler2D _MainTex;
						sampler2D _MainTex1;
						sampler2D _MainTex2;
						sampler2D _MainTex3;


			  struct Input {
				  float4 color : COLOR;
				  float2 uv;
				  float2 uv_MainTex;
				  float2 uv_MainTex1;
				  float2 uv_MainTex2;
				  float2 uv_MainTex3;
			  };
				   void vert(inout appdata_full v)


		{


		}
			  void surf(Input IN, inout SurfaceOutput o) {




				float2 uv1 = IN.uv_MainTex;
				float2 uv2 = IN.uv_MainTex1;
				float2 uv3 = IN.uv_MainTex2;
				float2 uv4 = IN.uv_MainTex3;



				  fixed3 albedo;

				half4 color1 = tex2D(_MainTex, uv1) * tex2D(_MainTex, uv1 * 0.25) * 2;
				half4 color2 = tex2D(_MainTex1, uv2) * tex2D(_MainTex1, uv2 * 0.25) * 3;
				half4 color3 = tex2D(_MainTex2, uv3) * tex2D(_MainTex2, uv3 * 0.25) * 3;
				half4 color4 = tex2D(_MainTex3, uv4) * tex2D(_MainTex3, uv4 * 0.25) * 3;

					half4 mask = IN.color;
					half3 c = color1.rgb * mask.r;
						  c += color2.rgb * mask.g;
						  c += color3.rgb * mask.b;
						  c += color4.rgb * mask.a;


					o.Albedo = c;

			  }

			  ENDCG

	}

		Fallback "Vertexlit"

}