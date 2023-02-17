Shader "Custom/VolumeSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    	_SurfaceThreshold ("Denisty Threshold", Range(0,0.06)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 200

    	ZWrite On
    	
        CGPROGRAM
		// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
		#pragma exclude_renderers d3d11 gles
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        #define MAX_STEPS 64
        
        struct Input{
            float2 uv_MainTex;
        	float3 worldPos;
        	float3 viewDir;
        	float4 screenPos;
        };

        struct Ray {
			float3 origin;
			float3 dir;
		};

		struct AABB {
			float3 Min;
			float3 Max;
		};

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        sampler3D Volume;
        sampler3D SDF;
        float _SurfaceThreshold;
        float3 Translate;
        float3 Scale;
        float3 Size;

        /// samples the density at a given position
		float sample_density(float3 pos){
            return tex3D(Volume, pos).r;
        }

        float sample_SDF(float3 pos){
			return tex3D(SDF, pos).r;
		}
        
		/// Calculates the gradient in the density field
        float3 gradient(float3 pos) {
            float3 eps = float3(0.001, 0.0, 0.0);
            float3 grad;
            grad.x = sample_density(pos + eps.xyy) - sample_density(pos - eps.xyy);
            grad.y = sample_density(pos + eps.yxy) - sample_density(pos - eps.yxy);
            grad.z = sample_density(pos + eps.yyx) - sample_density(pos - eps.yyx);
            return normalize(grad);
        }

        /// Find intersection points of a ray with a box
		bool intersect_box(Ray r, AABB aabb, out float t0, out float t1) {
			const float3 inv_r = 1.0 / r.dir;
			const float3 t_bot = inv_r * (aabb.Min - r.origin);
			const float3 t_top = inv_r * (aabb.Max - r.origin);
			const float3 t_min = min(t_top, t_bot);
			const float3 t_max = max(t_top, t_bot);
			float2 t = max(t_min.xx, t_min.yz);
			t0 = max(t.x, t.y);
			t = min(t_max.xx, t_max.yz);
			t1 = min(t.x, t.y);

			return t0 <= t1;
		}

        /// trace ray until a surface is found, returns number of steps to hit surface
        /// returns -1 if no surface found
        // int ray_trace(float3 rayStart, float3 rayStop){
        //     const float step_size = distance(rayStop, rayStart)/float(MAX_STEPS);
        // 	const float3 step = normalize(rayStop-rayStart) * step_size;
	       //  for (int i=0; i < MAX_STEPS; i++){
		      //   const float3 pos = rayStart + step * i;
		      //   const float density = sample_density(pos);
	       //  	if (density >= _SurfaceThreshold)
	       //  		return i;
	       //  }
	       //  return -1;
        // }

		/// Performs a ray march on the signed distance field (SDF) Volume
		/// Returns true if ray march finds a fluid surface
		/// ray_position is the output position of the surface
   //      bool ray_march(float3 ray_start, float3 ray_dir, float3 ray_stop, out float3 ray_position){
   //          float total_dist = 0.0;
   //          ray_position = ray_start;
   //          const float max_dist = distance(ray_stop,ray_start);
			//
			// for (int i = 0; i < MAX_STEPS; i++) {
			// 	const float density = sample_density(ray_position);
			// 	const float dist = abs(density);
   //
			//     if (dist < _SurfaceThreshold)
			//     	return true;
   //
			//     total_dist += dist;
			//     ray_position = ray_start + ray_dir * total_dist;
			// 	if (total_dist >= max_dist)
			// 		return false;
			// }
			// return false;
   //      }
		
		bool ray_march(float3 ray_start, float3 ray_dir, float3 ray_stop, out float3 ray_position){
			float total_dist = 0;
            ray_position = ray_start;
            const float max_dist = distance(ray_stop,ray_start);
			float dist = 1.0;
			for (int i = 0; i < MAX_STEPS; i++) {
				const float density = sample_SDF(ray_position);
				dist = abs(density);
				
			    if (dist < _SurfaceThreshold)
			    	break;

			    total_dist += dist;
			    ray_position = ray_start + ray_dir * total_dist;
				if (total_dist >= max_dist)
					break;
			}
			
			return dist < _SurfaceThreshold;
        }
        
		/// Generates a Ray from camera
		/// Works fro both perspective and orthographic projections
        Ray create_ray(float3 worldPos){
			Ray r;
			//If orthographic projection
			if (unity_OrthoParams.w == 1.0){
				r.dir = normalize(mul(float4(0,0,-1,0), UNITY_MATRIX_V));
				r.origin = worldPos - r.dir;
			}
			// if Perspective projection
			else{
				r.origin = _WorldSpaceCameraPos;
				r.dir = normalize(worldPos - r.origin);
			}
			return r;
		}

        /// Samples the sky cubemap and returns the reflection color
        half3 reflectionColor(float3 ray_dir, float3 normal){
			const half3 world_reflection = reflect(-ray_dir, normal);
			const half4 sky_data = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, world_reflection);
	        // decode cubemap data into actual color
			return DecodeHDR (sky_data, unity_SpecCube0_HDR);
		}
        
        float diffuse(float3 normal, float3 ray_dir){
			return max(0.0, dot(normal, ray_dir));
		}

        //convert to texture space
        void translate_ray(Ray ray, float t_near, float t_far, out float3 ray_start, out float3 ray_stop){
			ray_start = ray.origin + ray.dir * t_near;
			ray_stop = ray.origin + ray.dir * t_far;
			ray_start -= Translate;
			ray_stop -= Translate;
			ray_start = (ray_start + 0.5 * Scale) / Scale;
			ray_stop = (ray_stop + 0.5 * Scale) / Scale;
		}

        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assume uniform scaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o){
			
	        const Ray ray = create_ray(IN.worldPos);
			
			AABB aabb;
			aabb.Min = float3(-0.5,-0.5,-0.5) * Scale + Translate;
			aabb.Max = float3(0.5,0.5,0.5) * Scale + Translate;
	  
			//figure out where ray from eye hit front of cube
			float t_near, t_far;
			intersect_box(ray, aabb, t_near, t_far);
	  
			//if eye is in cube then start ray at eye
			if (t_near < 0.0) t_near = 0.0;
	  
			float3 ray_start;
			float3 ray_stop;
	  
			translate_ray(ray, t_near, t_far, ray_start, ray_stop);
	        const float3 ray_dir = normalize(ray_stop-ray_start);
			// Will be changed by ray march 
			float3 ray_pos = ray_start;
	        const bool result = ray_march(ray_start, ray_dir, ray_stop, ray_pos);
			if (result == true){
				const float3 normal = gradient(ray_pos);
				o.Albedo = _Color * diffuse(normal, ray.dir) * 0.5 + 0.5;
				o.Alpha = 1;
			}
			else{
				o.Albedo = float3(0,0,0);
				o.Alpha = 0;
			}
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			
        }
        ENDCG
    }
    FallBack "Diffuse"
}
