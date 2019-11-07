using UnityEngine;

namespace Spine.Unity.Examples {
    /// <summary>
    /// 相机跟随脚本
    /// </summary>
	public class CameraFollow : MonoBehaviour {
		public Transform target;
		public Vector3 offset;
		public Vector3 min;
		public Vector3 max;
		public float smoothing = 5f;

		// Update is called once per frame
		void LateUpdate () {
			Vector3 goalPoint = target.position + offset;
			goalPoint.x = Mathf.Clamp(goalPoint.x, min.x, max.x);
			goalPoint.y = Mathf.Clamp(goalPoint.y, min.y, max.y);
			goalPoint.z = Mathf.Clamp(goalPoint.z, min.z, max.z);

			transform.position = Vector3.Lerp(transform.position, goalPoint, smoothing * Time.deltaTime);
		}
	}
}
