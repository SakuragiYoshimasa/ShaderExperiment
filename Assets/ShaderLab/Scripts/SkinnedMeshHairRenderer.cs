using System.Collections.Generic;
using UnityEngine;

namespace ShaderLab{
	
	public class SkinnedMeshHairRenderer : MonoBehaviour {

		[SerializeField] Hair _hair;
		[SerializeField] Material _material;
		[SerializeField] int _instanceCount;
		[SerializeField] float _scale = 1.0f;
		[SerializeField] float _zScale = 0.07f;
		[SerializeField] float _noisePower = 0.1f;
		[SerializeField] float _frequency = 1.0f;
		[SerializeField] float _radiusAmp = 1.0f;
		[SerializeField] Vector3 _gravity = new Vector3(0f, -8.0f, 4.0f);
		[SerializeField] Color _color = new Vector4(0,0,0,0);
		[SerializeField] Color _gradcolor = new Vector4(0,0,0,0);
		
		uint[] _drawArgs = new uint[5]{0, 0, 0, 0, 0};
		ComputeBuffer _drawArgsBuffer;
		ComputeBuffer positionBuffer;
		ComputeBuffer rotationBuffer;
		ComputeBuffer indicesBuffer;
		ComputeBuffer weightsBuffer;
		ComputeBuffer prevPositionBuffer;
		
		SkinnedMeshRenderer _skinnedMeshR;
		Mesh _targetmesh;
		Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 4 * 32);
		MaterialPropertyBlock _props;
		AnimatedSkinnedMesh asm;

		[SerializeField] ComputeShader _recalcGlownPositionsShader;

		void Start(){
			
			_drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
			_drawArgs[0] = (uint)_hair.mesh.GetIndexCount(0);
			_drawArgs[1] = (uint)_instanceCount;
			_drawArgsBuffer.SetData(_drawArgs);

			_props = new MaterialPropertyBlock();
            _props.SetFloat("_UniqueID", Random.value);

            _material.SetInt("_ArraySize", _hair.segments + 1);
            _material.SetInt("_InstanceCount", _instanceCount);
			_material.SetInt("_SegmentCount", _hair.segments);

			asm = GetComponent<AnimatedSkinnedMesh>();
			_skinnedMeshR = asm.targetSMR;
			_targetmesh = new Mesh();
			
			InitBuffers();
		}

		void Update(){
			UpdatePositionBuffer();
		}

		void LateUpdate(){
			_material.SetFloat("_Scale",_scale);
			_material.SetFloat("_ZScale",_zScale);
			_material.SetFloat("_NoisePower",_noisePower);
			_material.SetVector("_Gravity", _gravity);
			_material.SetFloat("_Scale",_scale);
			_material.SetFloat("_Frequency", _frequency);
			_material.SetFloat("_RadiusAmp", _radiusAmp);
			_material.SetColor("_MainColor", _color);
			_material.SetColor("_GradColor", _gradcolor);
			_material.SetBuffer("_PositionBuffer", positionBuffer);
			_material.SetBuffer("_RotationBuffer", rotationBuffer);
			_material.SetBuffer("_PrevPositionBuffer", prevPositionBuffer);
			Graphics.DrawMeshInstancedIndirect(_hair.mesh, 0, _material, _bounds, _drawArgsBuffer, 0, _props);
		}

		void UpdatePositionBuffer(){
		
			int kernel =_recalcGlownPositionsShader.FindKernel("RecalcPositions");
			prevPositionBuffer = positionBuffer;
			var data = new Vector4[_instanceCount];
			positionBuffer.GetData(data);
			prevPositionBuffer.SetData(data);
			
			_recalcGlownPositionsShader.SetBuffer(kernel, "PositionBuffer", positionBuffer); 
			_recalcGlownPositionsShader.SetBuffer(kernel, "RotationBuffer", rotationBuffer); 
			_recalcGlownPositionsShader.SetBuffer(kernel, "IndicesBuffer", indicesBuffer);  
			_recalcGlownPositionsShader.SetBuffer(kernel, "WeightsBuffer", weightsBuffer);
			_recalcGlownPositionsShader.SetTexture(kernel, "CurrPositionBuffer", asm.CurrPositionBuffer);
			_recalcGlownPositionsShader.SetTexture(kernel, "CurrNormalBuffer", asm.CurrNormalBuffer);
			_recalcGlownPositionsShader.Dispatch(kernel, 64, 1, 1);
		}
		
		void InitBuffers(){
			if(positionBuffer != null) positionBuffer.Release();
			if(rotationBuffer != null) rotationBuffer.Release();
			if(indicesBuffer != null) indicesBuffer.Release();
			if(weightsBuffer != null) weightsBuffer.Release();
			if(prevPositionBuffer != null) prevPositionBuffer.Release();

			positionBuffer = new ComputeBuffer(_instanceCount, sizeof(float) * 4);
			prevPositionBuffer = new ComputeBuffer(_instanceCount, sizeof(float) * 4);
			rotationBuffer = new ComputeBuffer(_instanceCount, sizeof(float) * 4);
			indicesBuffer = new ComputeBuffer(_instanceCount * 3, sizeof(int));
			weightsBuffer = new ComputeBuffer(_instanceCount, sizeof(float) * 3);

			Vector4[] positions = new Vector4[_instanceCount];
			Vector4[] rotations = new Vector4[_instanceCount];
			Vector3[] weights = new Vector3[_instanceCount];
			int[] indices = new int[_instanceCount * 3];

			_skinnedMeshR.BakeMesh(_targetmesh);
			int targetTriangleCount = _targetmesh.triangles.Length / 3;
			
			HashSet<int> nouse = new HashSet<int>();
			int index = (int)Random.Range(0f, targetTriangleCount + 0.5f);
			float threshold = 0.04f;

			for (int i=0; i < _instanceCount; i++) {
				index = (index + (int)Random.Range(0f, targetTriangleCount + 0.5f)) % targetTriangleCount;

				while(nouse.Contains(index)){
					index = (index + (int)Random.Range(0f, targetTriangleCount + 0.5f)) % targetTriangleCount;
				}

				Vector3 v1 = _targetmesh.vertices[_targetmesh.triangles[index * 3]];
				Vector3 v2 = _targetmesh.vertices[_targetmesh.triangles[index * 3 + 1]];
				Vector3 v3 = _targetmesh.vertices[_targetmesh.triangles[index * 3 + 2]];
		
				Vector3 n1 = _targetmesh.normals[_targetmesh.triangles[index * 3]];
				Vector3 n2 = _targetmesh.normals[_targetmesh.triangles[index * 3 + 1]];
				Vector3 n3 = _targetmesh.normals[_targetmesh.triangles[index * 3 + 2]];
				
				float mag = ((v1 - v2).magnitude + (v2 - v3).magnitude + (v1 - v3).magnitude)/3.0f; 
				if(mag < threshold) {
					nouse.Add(index);
					i--;
					continue;
				}

				float p1 = Random.Range(0, 1.0f);
				float p2 = Random.Range(0, 1.0f - p1);
				float p3 = 1.0f - p1 - p2;
				Vector3 p = v1 * p1 + v2 * p2 + v3 * p3;
				Vector3 n = n1 * p1 + n2 * p2 + n3 * p3 + new Vector3(0, 1.0f,0);

				float radius = Random.Range(0.015f, 0.05f);
				
				positions[i] = new Vector4(p.x, p.y, p.z, radius);

				Vector3 rotation = Quaternion.LookRotation(n, Vector3.up).eulerAngles;
				rotations[i] = new Vector4(rotation.x / 180.0f * Mathf.PI, rotation.y / 180.0f * Mathf.PI, rotation.z / 180.0f * Mathf.PI, mag);

				indices[i * 3] = _targetmesh.triangles[index * 3];
				indices[i * 3 + 1] = _targetmesh.triangles[index * 3 + 1];
				indices[i * 3 + 2] = _targetmesh.triangles[index * 3 + 2];
				weights[i] = new Vector3(p1, p2, p3);
			}

			positionBuffer.SetData(positions);
			rotationBuffer.SetData(rotations);
			indicesBuffer.SetData(indices);
			weightsBuffer.SetData(weights);

			_material.SetBuffer("_PositionBuffer", positionBuffer);
			_material.SetBuffer("_RotationBuffer", rotationBuffer);
		}

		void OnDestroy(){
			if(_drawArgsBuffer != null) _drawArgsBuffer.Release();
			if(positionBuffer != null) positionBuffer.Release();
			if(rotationBuffer != null) rotationBuffer.Release();
			if(indicesBuffer != null) indicesBuffer.Release();
			if(weightsBuffer != null) weightsBuffer.Release();
		}
	}
}
