using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderLab{
	public class HairRenderer : MonoBehaviour {

		[SerializeField] Hair _hair;
		[SerializeField] Mesh _targetmesh;
		[SerializeField] Material _material;
		[SerializeField] int _instanceCount;

		uint[] _drawArgs = new uint[5]{0, 0, 0, 0, 0};
		ComputeBuffer _drawArgsBuffer;
		ComputeBuffer positionBuffer;
		ComputeBuffer rotationBuffer;

		Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 128*64);
		MaterialPropertyBlock _props;

		void Start(){
			UpdateBuffers();

			_drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
			_drawArgs[0] = _hair.mesh.GetIndexCount(0);
			_drawArgs[1] = (uint)_instanceCount;
			_drawArgsBuffer.SetData(_drawArgs);

			_props = new MaterialPropertyBlock();
            _props.SetFloat("_UniqueID", Random.value);

            _material.SetInt("_ArraySize", _hair.segments + 1);
            _material.SetInt("_InstanceCount", _instanceCount);
			_material.SetInt("_SegmentCount", _hair.segments);
		}


		void LateUpdate(){
			Graphics.DrawMeshInstancedIndirect(_hair.mesh, 0, _material, _bounds, _drawArgsBuffer, 0, _props);
		}

		void UpdateBuffers(){
			if (positionBuffer != null)
				positionBuffer.Release();
			if(rotationBuffer != null)
				rotationBuffer.Release();
			positionBuffer = new ComputeBuffer(_instanceCount, sizeof(float));
			rotationBuffer = new ComputeBuffer(_instanceCount, sizeof(float));

			Vector4[] positions = new Vector4[_instanceCount];
			Vector4[] rotations = new Vector4[_instanceCount];

			int targetTriangleCount = _targetmesh.triangles.Length / 3;
			//Debug.Log(targetTriangleCount);
			
			HashSet<int> used = new HashSet<int>();
			int index = (int)Random.Range(0f, targetTriangleCount + 0.5f);
			float threshold = 0.3f;

			for (int i=0; i < _instanceCount; i++) {
				index = (index + (int)Random.Range(0f, targetTriangleCount + 0.5f)) % targetTriangleCount;

				while(used.Contains(index)){
					index = (index + (int)Random.Range(0f, targetTriangleCount + 0.5f)) % targetTriangleCount;
					if(used.Count > (float)_instanceCount * 0.8f){
						used.Clear();
						threshold += 0.2f;
					}
				}

				Vector3 v1 = _targetmesh.vertices[_targetmesh.triangles[index * 3]];
				Vector3 v2 = _targetmesh.vertices[_targetmesh.triangles[index * 3 + 1]];
				Vector3 v3 = _targetmesh.vertices[_targetmesh.triangles[index * 3 + 2]];
		
				Vector3 n1 = _targetmesh.normals[_targetmesh.triangles[index * 3]];
				Vector3 n2 = _targetmesh.normals[_targetmesh.triangles[index * 3 + 1]];
				Vector3 n3 = _targetmesh.normals[_targetmesh.triangles[index * 3 + 2]];
				
				float mag = (v1 - v2).magnitude;
				if(mag < threshold) continue;
				int additionalCount = (int)((v1 - v2).magnitude * 10.0f);
				//Debug.Log(additionalCount);

				for(int j=0; j < additionalCount && i < _instanceCount; j++){

					float p1 = Random.Range(0, 1.0f);
					float p2 = Random.Range(0, 1.0f - p1);
					float p3 = 1.0f - p1 - p2;
					Vector3 p = v1 * p1 + v2 * p2 + v3 * p3;
					Vector3 n = n1 * p1 + n2 * p2 + n3 * p3;

					float radius = Random.Range(0.001f, 0.01f);
					positions[i] = new Vector4(p.x, p.y, p.z, radius);

					Vector3 rotation = Quaternion.LookRotation(n, Vector3.up).eulerAngles;
					rotations[i] = new Vector4(rotation.x / 180.0f * Mathf.PI, rotation.y / 180.0f * Mathf.PI, rotation.z / 180.0f * Mathf.PI, (float)additionalCount);

					i++;
				}
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