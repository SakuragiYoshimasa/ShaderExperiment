using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentUtilities;


namespace ProceduralStage {
	public static class StageHeightFactory {

		#region Interface
		public static Texture2D getTex(TextureType type, int texSize){
			Texture2D tex = new Texture2D (texSize, texSize);

			for(int i = 0; i < texSize; i ++){
				for(int j = 0; j < texSize; j++){
					
					switch(type){
						case TextureType.Grid:
							tex.SetPixel (i, j, getGridHeight(i, j, texSize));
							break;
						case TextureType.Chessboard:
							tex.SetPixel (i, j, getChessboardHeight(i, j, texSize));
							break;
						default:break;
					}
				}
			}
			tex.Apply ();
			return tex;
		}
		#endregion


		#region Color
		private static Color getGridHeight(int i, int j, int texSize){
			if (i % 64 == 0 || j % 64 == 0) {
				return  new Color (1.0f, 1.0f, 1.0f);	
			} else {
				return new Color (0.0f, 0.0f, 0.0f);	
			}
		}

		private static Color getChessboardHeight(int i, int j, int texSize){
			//float noise = Perlin.Noise((float)i/(float)texSize * 10.0f, (float)j/(float)texSize * 10.0f);
			float noise = Mathf.Sin(((float)i + (float)j)/ 10.0f);
			return  new Color (noise, noise, noise);

			if ( !(i % 64 >= 32 && j % 64 >= 32) && (i % 64 >= 32 || j % 64 >= 32)) {
			
				return  new Color (1.0f, 1.0f, 1.0f);	
			} else {
				return new Color (0.0f, 0.0f, 0.0f);	
			}
		}
		#endregion

	}
}
