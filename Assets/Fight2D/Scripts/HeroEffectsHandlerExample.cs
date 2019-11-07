using UnityEngine;
using UnityEngine.Events;

namespace Spine.Unity.Examples {
    /// <summary>
    /// 英雄效果处理程序示例
    /// </summary>
	public class HeroEffectsHandlerExample : MonoBehaviour {
		public BasicPlatformerController eventSource;// 基本的游戏平台控制器（角色的）
        public UnityEvent OnJump, OnLand, OnHardLand;// 事件

		public void Awake () {
			if (eventSource == null)
				return;

            // 添加事件委托（出发时相应对应事件）
            eventSource.OnLand += OnLand.Invoke;// 播放声音和特效
			eventSource.OnJump += OnJump.Invoke;
			eventSource.OnHardLand += OnHardLand.Invoke;
		}
	}
}
