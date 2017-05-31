using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExperimentUtilities {

	public class Buffer {

		public static RenderTexture CreateRTBuffer(int size){
			var format = RenderTextureFormat.ARGBFloat;
			var buffer = new RenderTexture(size, 1, 0, format);
			buffer.hideFlags = HideFlags.DontSave;
			buffer.filterMode = FilterMode.Point;
			buffer.wrapMode = TextureWrapMode.Clamp;
			return buffer;
		}

		public static RenderTexture CreateSquereRTBuffer(int size_x, int size_y){
			var format = RenderTextureFormat.ARGBFloat;
			var buffer = new RenderTexture(size_x, size_y, 0, format);
			buffer.hideFlags = HideFlags.DontSave;
			buffer.filterMode = FilterMode.Point;
			buffer.wrapMode = TextureWrapMode.Clamp;
			return buffer;
		}

		public static Texture2D CreateT2Buffer(int size){
			var texture = new Texture2D(size, 1, TextureFormat.RGBAFloat, false);
			for(int i = 0; i < size; i++) texture.SetPixel (i, 1, new Color(0, 0, 0, 0));
			texture.Apply ();
			return texture;
		}

		public static Texture2D CreateSquereT2Buffer(int size_x, int size_y){	
			var texture = new Texture2D(size_x, size_y, TextureFormat.RGBAFloat, false);
			for(int i = 0; i < size_x; i++){
				for(int j = 0; j < size_y; j++){
				texture.SetPixel (i, j, new Color(0, 0, 0, 0));
				}
			}
			texture.Apply ();
			return texture;
		}
	}
}