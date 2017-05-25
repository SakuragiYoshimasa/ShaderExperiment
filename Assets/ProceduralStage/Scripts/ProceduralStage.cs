using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentUtilities;

namespace ProceduralStage {

	public class ProceduralStage : MonoBehaviour {

		#region Resources
		[Header("Instances")]
		[SerializeField] List<MeshRenderer> _walls;
		[SerializeField] Material _standardMat;

		[Header("Texture Settings")]
		[SerializeField, Range(64, 1024)] int _texsize;
		[SerializeField] TextureType _texType = TextureType.Grid;
		StageTexture stex;
		
		[Header("Material Propaties")]
		[SerializeField] Color _mainColor;
		[SerializeField, Range(0.0f, 1.0f)] float _smoothness;
		[SerializeField, Range(0.0f, 1.0f)] float _metallic;
		#endregion

		void Awake(){

			stex = new StageTexture();
			UpdateTexture();
			foreach (MeshRenderer wall in _walls) {
				wall.material = _standardMat;
			}
		}
		
		void Update () {
			if(_texType != stex.Type) UpdateTexture();
			UpdateMaterialProperties();
		}

		void UpdateTexture(){
			stex.Tex = StageTextureFactory.getTex(_texType ,_texsize);
			stex.Type = _texType;
			_standardMat.SetTexture("_MainTex", stex.Tex);
		}

		void UpdateMaterialProperties(){
			_standardMat.SetColor("_Color", _mainColor);
			_standardMat.SetFloat("_Glossoness", _smoothness);
			_standardMat.SetFloat("_Metallic", _metallic);

			/*StandardMat properties
			_Color("Color", Color) = (1,1,1,1)
			_MainTex("Albedo", 2D) = "white" {}
			
			_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

			_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
			_GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
			[Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

			[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
			_MetallicGlossMap("Metallic", 2D) = "white" {}

			[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
			[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

			_BumpScale("Scale", Float) = 1.0
			_BumpMap("Normal Map", 2D) = "bump" {}

			_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
			_ParallaxMap ("Height Map", 2D) = "black" {}

			_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
			_OcclusionMap("Occlusion", 2D) = "white" {}

			_EmissionColor("Color", Color) = (0,0,0)
			_EmissionMap("Emission", 2D) = "white" {}
			
			_DetailMask("Detail Mask", 2D) = "white" {}

			_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
			_DetailNormalMapScale("Scale", Float) = 1.0
			_DetailNormalMap("Normal Map", 2D) = "bump" {}
			 */
		}
	}
}