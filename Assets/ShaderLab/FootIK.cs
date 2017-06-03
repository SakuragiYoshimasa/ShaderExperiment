using UnityEngine;
using System.Collections;

namespace ShaderLab{
	public class FootIK : MonoBehaviour 
	{
		private Animator animator_;

		public bool  isDrawDebug = false; 
		public float heelOffsetZ = 0f;
		public float toeOffsetZ  = 0f;
		public float rayLength   = 0.2f;

		// 身体の各ボーンの位置は Animator から取れるのでエイリアスを作っておくと便利
		private Transform leftFoot  { get { return animator_.GetBoneTransform(HumanBodyBones.LeftFoot);  } }
		private Transform rightFoot { get { return animator_.GetBoneTransform(HumanBodyBones.RightFoot);  } }
		private Transform leftToe   { get { return animator_.GetBoneTransform(HumanBodyBones.LeftToes); } }
		private Transform rightToe  { get { return animator_.GetBoneTransform(HumanBodyBones.RightToes); } }


		void Start()
		{
			animator_ = GetComponent<Animator>();
		}


		void OnAnimatorIK()
		{
			// IK の位置から踵・つま先のオフセットを設定
			var heelOffset   = Vector3.up * heelOffsetZ;
			var toeOffset    = Vector3.up * toeOffsetZ;
			var leftHeelPos  = leftFoot.position  + heelOffset;
			var leftToePos   = leftToe.position   + toeOffset;
			var rightHeelPos = rightFoot.position + heelOffset;
			var rightToePos  = rightToe.position  + toeOffset;

			// 足の位置を IK に従って動かす
			var leftIkMoveLength  = UpdateFootIk(AvatarIKGoal.LeftFoot,  leftHeelPos,  leftToePos);
			var rightIkMoveLength = UpdateFootIk(AvatarIKGoal.RightFoot, rightHeelPos, rightToePos);

			// 身体の位置を下げないと IK で移動できないので
			// IK で移動させた差分だけ身体を下げる
			animator_.bodyPosition += Mathf.Max(leftIkMoveLength, rightIkMoveLength) * Vector3.down;
		}


		float UpdateFootIk(AvatarIKGoal goal, Vector3 heelPos, Vector3 toePos)
		{
			// レイを踵から飛ばす（めり込んでた時も平気なようにちょっと上にオフセットさせる）
			RaycastHit ray;
			var from   = heelPos + Vector3.up * rayLength;
			var to     = Vector3.down;
			var length = 2 * rayLength;

			if (Physics.Raycast(from, to, out ray, length)) {
				// レイが当たった場所を踵の場所にする
				var nextHeelPos = ray.point - Vector3.up * heelOffsetZ;
				var diffHeelPos = (nextHeelPos - heelPos);

				// Animator.SetIKPosition() で IK 位置を動かせるので、
				// 踵の移動分だけ動かす
				// 第１引数は AvatarIKGoal という enum（LeftFoot や RightHand など）
				animator_.SetIKPosition(goal, animator_.GetIKPosition(goal) + diffHeelPos);
				// Animator.SetIKPositionWeight() では IK のブレンド具合を指定できる
				// 本当は 1 固定じゃなくて色々フィルタ掛けると良いと思う
				animator_.SetIKPositionWeight(goal, 1f);

				// 踵からつま先の方向に接地面が上になるように向く姿勢を求めて
				// IK に反映させる
				var rot = GetFootRotation(nextHeelPos, toePos, ray.normal);
				animator_.SetIKRotation(goal, rot);
				animator_.SetIKRotationWeight(goal, 1f);

				// レイを確認用に描画しておくと分かりやすい
				if (isDrawDebug) {
					Debug.DrawLine(heelPos, ray.point, Color.red);
					Debug.DrawRay(nextHeelPos, rot * Vector3.forward, Color.blue);
				}

				return diffHeelPos.magnitude;
			}

			return 0f;
		}


		Quaternion GetFootRotation(Vector3 heelPos, Vector3 toePos, Vector3 slopeNormal)
		{
			// つま先の位置からレイを下に飛ばす
			RaycastHit ray;
			if (Physics.Raycast(toePos, Vector3.down, out ray, 2 * rayLength)) {
				if (isDrawDebug) {
					Debug.DrawLine(toePos, ray.point, Color.red);
				}
				var nextToePos = ray.point + Vector3.up * toeOffsetZ;
				// つま先方向に接地面の法線を上向きとする傾きを求める
				return Quaternion.LookRotation(nextToePos - heelPos, slopeNormal);
			}
			// レイが当たらなかったらつま先の位置はそのままで接地面方向に回転だけする
			return Quaternion.LookRotation(toePos - heelPos, slopeNormal);
		}
	}
}