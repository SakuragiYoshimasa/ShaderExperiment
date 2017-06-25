using System.Collections.Generic;
using UnityEngine;
using ExperimentUtilities;

namespace ShaderLab{
	public class AnimatedSkinnedMesh : MonoBehaviour {

		[SerializeField] Shader positionExtractionShader;
		[SerializeField] Material placeHolder;
		[SerializeField] public SkinnedMeshRenderer targetSMR;
		RenderTexture prevPositionBuffer;
		RenderTexture currPositionBuffer;
		RenderTexture currNormalBuffer;
		private Camera camera;

		public RenderTexture CurrPositionBuffer {
			get {
				if(currPositionBuffer == null) currPositionBuffer = Buffer.CreateRTBuffer(targetSMR.sharedMesh.vertexCount);
				return currPositionBuffer;
			}
		}

		public RenderTexture PrevPositionBuffer {
			get {
				if(prevPositionBuffer == null) prevPositionBuffer = Buffer.CreateRTBuffer(targetSMR.sharedMesh.vertexCount);
				return prevPositionBuffer;
			}
		}

		public RenderTexture CurrNormalBuffer {
			get {
				if(currNormalBuffer == null) currNormalBuffer = Buffer.CreateRTBuffer(targetSMR.sharedMesh.vertexCount);
				return currNormalBuffer;
			}
		}
		
		void Start () {
			prevPositionBuffer = Buffer.CreateRTBuffer(targetSMR.sharedMesh.vertexCount);
			currPositionBuffer = Buffer.CreateRTBuffer(targetSMR.sharedMesh.vertexCount);
			currNormalBuffer = Buffer.CreateRTBuffer(targetSMR.sharedMesh.vertexCount);
			
			//Camera Attatchment
			GameObject cameraobj = new GameObject("Camera");
			cameraobj.hideFlags = HideFlags.HideInHierarchy;
			camera = cameraobj.AddComponent<Camera>();
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.depth = -10000;
			camera.nearClipPlane = -100;
			camera.farClipPlane = 100;
			camera.orthographic = true;
			camera.orthographicSize = 100;
			camera.gameObject.transform.parent = transform;
			camera.gameObject.transform.position = Vector3.zero;
			camera.gameObject.transform.rotation = Quaternion.identity;
			camera.renderingPath = RenderingPath.Forward;
			camera.enabled = false;
			//Layer Setting to Cilling mask
			camera.cullingMask = 1 << LayerMask.NameToLayer("PositionExtractedMesh");
			camera.SetReplacementShader(positionExtractionShader, "Extract");
			RecreateMesh();

			targetSMR.receiveShadows = false;
			targetSMR.gameObject.layer = LayerMask.NameToLayer("PositionExtractedMesh");
			targetSMR.enabled = false;
			var culler = cameraobj.AddComponent<CullingStateController>();
            culler.target = targetSMR;
		}
		
		void LateUpdate () {
			
			SwapBuffer();
			currPositionBuffer.filterMode = FilterMode.Point;
			Material temp = targetSMR.material;
			targetSMR.material = placeHolder;
			camera.SetTargetBuffers(new RenderBuffer[2]{currPositionBuffer.colorBuffer, currNormalBuffer.colorBuffer}, currPositionBuffer.depthBuffer);
			camera.Render();		
			targetSMR.material = temp;
		}

		void SwapBuffer(){
			RenderTexture temp = currPositionBuffer;
			currPositionBuffer = prevPositionBuffer;
			prevPositionBuffer = temp;
		}

		void RecreateMesh(){
			//Recalculate index of vertices to uv
			Mesh mesh = new Mesh();
			Mesh origMesh = targetSMR.sharedMesh;

			var vertices = new List<Vector3>(origMesh.vertices);
			var normals = new List<Vector3>(origMesh.normals);
			var tangents = new List<Vector4>(origMesh.tangents);
			var boneWeights = new List<BoneWeight>(origMesh.boneWeights);
			int[] indices = new int[origMesh.vertexCount];
			
			List<Vector2> uv = new List<Vector2>();
			for(int i = 0; i < origMesh.vertexCount; i++){
				uv.Add(new Vector2(((float)i+0.5f) / (float) origMesh.vertexCount, 0));
				indices[i] = i;
			}
			
			mesh.subMeshCount = 1;
			mesh.SetVertices(vertices);
			mesh.SetNormals(normals);
			mesh.SetTangents(tangents);
			mesh.SetIndices(indices, MeshTopology.Points, 0);
			mesh.SetUVs(0, uv);
			mesh.bindposes = origMesh.bindposes;
			mesh.boneWeights = boneWeights.ToArray();

			mesh.UploadMeshData(true);

			targetSMR.sharedMesh = mesh;
		}

		void OnDestroy(){
			if(camera != null) DestroyImmediate(camera.gameObject);
			if(prevPositionBuffer != null) prevPositionBuffer.Release();
			if(currPositionBuffer != null) currPositionBuffer.Release();
			if(currNormalBuffer != null) currNormalBuffer.Release();
		}
	}
}