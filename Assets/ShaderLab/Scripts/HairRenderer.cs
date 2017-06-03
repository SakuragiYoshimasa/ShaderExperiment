using System.Collections.Generic;
using UnityEngine;

namespace ShaderLab{
	public class HairRenderer : MonoBehaviour {

		[SerializeField] Hair _hair;
		[SerializeField] Mesh _targetmesh;
		[SerializeField] Material _material;
		[SerializeField] int _instanceCount;
		[SerializeField] float _scale = 1.0f;
		[SerializeField] float _zScale = 0.07f;
		[SerializeField] float _noisePower = 0.1f;
		[SerializeField] Vector3 _gravity = new Vector3(0f, -8.0f, 4.0f);
		[SerializeField] Color _color = new Vector4(0,0,0,0);
		[SerializeField] Color _gradcolor = new Vector4(0,0,0,0);

		uint[] _drawArgs = new uint[5]{0, 0, 0, 0, 0};
		ComputeBuffer _drawArgsBuffer;
		ComputeBuffer positionBuffer;
		ComputeBuffer rotationBuffer;

		Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 4 * 32);
		MaterialPropertyBlock _props;

		void Start(){
			UpdateBuffers();

			_drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
			_drawArgs[0] = (uint)_hair.mesh.GetIndexCount(0);
			_drawArgs[1] = (uint)_instanceCount;
			_drawArgsBuffer.SetData(_drawArgs);

			_props = new MaterialPropertyBlock();
            _props.SetFloat("_UniqueID", Random.value);

            _material.SetInt("_ArraySize", _hair.segments + 1);
            _material.SetInt("_InstanceCount", _instanceCount);
			_material.SetInt("_SegmentCount", _hair.segments);
		}


		void LateUpdate(){
			_material.SetFloat("_Scale",_scale);
			_material.SetFloat("_ZScale",_zScale);
			_material.SetFloat("_NoisePower",_noisePower);
			_material.SetVector("_Gravity", _gravity);
			_material.SetFloat("_Scale",_scale);
			_material.SetColor("_MainColor", _color);
			_material.SetColor("_GradColor", _gradcolor);

			Graphics.DrawMeshInstancedIndirect(_hair.mesh, 0, _material, _bounds, _drawArgsBuffer, 0, _props);
		}
		
		void UpdateBuffers(){
			if (positionBuffer != null)
				positionBuffer.Release();
			if(rotationBuffer != null)
				rotationBuffer.Release();
			positionBuffer = new ComputeBuffer(_instanceCount, sizeof(float) * 4);
			rotationBuffer = new ComputeBuffer(_instanceCount, sizeof(float) * 4);
			
			Vector4[] positions = new Vector4[_instanceCount];
			Vector4[] rotations = new Vector4[_instanceCount];

			int targetTriangleCount = _targetmesh.triangles.Length / 3;
			
			HashSet<int> nouse = new HashSet<int>();
			int index = (int)Random.Range(0f, targetTriangleCount + 0.5f);
			float threshold = 0.1f;

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
			}
			positionBuffer.SetData(positions);
			rotationBuffer.SetData(rotations);
			_material.SetBuffer("_PositionBuffer", positionBuffer);
			_material.SetBuffer("_RotationBuffer", rotationBuffer);
		}

		void OnDestroy(){
			_drawArgsBuffer.Release();
			positionBuffer.Release();
			rotationBuffer.Release();
		}
	}
}