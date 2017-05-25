using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStage{
	public static class StageTextureFactory {

		#region Interface
		public static Texture2D getTex(TextureType type, int texSize){
			Texture2D tex = new Texture2D (texSize, texSize);

			for(int i = 0; i < texSize; i ++){
				for(int j = 0; j < texSize; j++){
					
					switch(type){
						case TextureType.Standard:
							tex.SetPixel (i, j, new Color(1.0f, 1.0f, 1.0f));
							break;
						case TextureType.Grid:
							tex.SetPixel (i, j, getGridColor(i, j));
							break;
						case TextureType.Chessboard:
							tex.SetPixel (i, j, getChessboardColor(i, j));
							break;
						case TextureType.Circles:
							tex.SetPixel (i, j, getCirclesColor(i, j, texSize));
							break;
						case TextureType.Perlin:
							tex.SetPixel (i, j, getPerlinColor(i, j, texSize));
							break;
						default:break;
					}
				}
			}
			tex.Apply ();
			return tex;
		}
		#endregion

		#region ColorGeneration
		
		private static Color getGridColor(int i, int j){
			if (i % 64 == 0 || j % 64 == 0) {
				return  new Color (1.0f, 1.0f, 1.0f);	
			} else {
				return new Color (0.0f, 0.0f, 0.0f);	
			}
		}

		private static Color getChessboardColor(int i, int j){
			if ( !(i % 64 >= 32 && j % 64 >= 32) && (i % 64 >= 32 || j % 64 >= 32)) {
				return  new Color (1.0f, 1.0f, 1.0f);	
			} else {
				return new Color (0.0f, 0.0f, 0.0f);	
			}
		}

		private static Color getCirclesColor(int i, int j, int texsize){
			Vector2 tex = new Vector2((float)i / (float)texsize - 0.5f, (float)j / (float)texsize - 0.5f);
			float dist = tex.magnitude * 200.0f;

			if((int)dist % 10 >= 5){
				return  new Color (1.0f, 1.0f, 1.0f);
			}else{
				return new Color (0.0f, 0.0f, 0.0f);
			}
		}

		private static Color getPerlinColor(int i, int j, int texsize){
			float noise = (Perlin.Noise((float)i/(float)texsize * 10.0f, (float)j/(float)texsize * 10.0f) + 1.0f) / 2.0f;
			return new Color(noise, noise, noise);
		}
		#endregion
	}
}
