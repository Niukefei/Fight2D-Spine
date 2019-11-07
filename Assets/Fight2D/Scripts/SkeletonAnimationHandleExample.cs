using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {

    // 这是动画处理的示例。这是用字符串作为状态名称来实现的。
    // 当动画系统用作状态机和状态源时，字符串可以用作标识符。
    // 如果您不使用动画系统，则使用自定义脚本对象可能是一种更有效的方式来存储有关状态及其与特定骨骼动画连接信息的方法。

    /// <summary>
    /// 骨骼动画处理器（此动画处理实现还附带了过渡处理的虚拟实现。）
    /// </summary>
    public class SkeletonAnimationHandleExample : MonoBehaviour {
		public SkeletonAnimation skeletonAnimation;// 人物骨骼动画引用
		public List<StateNameToAnimationReference> statesAndAnimations = new List<StateNameToAnimationReference>();// 状态和动画 组
		public List<AnimationTransition> transitions = new List<AnimationTransition>(); // 或者，可以使用“动画对-动画字典”（注释掉的）来进行更有效的查找。

        // 状态名对应动画引用
        [System.Serializable]
		public class StateNameToAnimationReference {
			public string stateName;// 状态名
			public AnimationReferenceAsset animation;// 动画引用
		}

        // 动画过渡
        [System.Serializable]
		public class AnimationTransition {
			public AnimationReferenceAsset from;// 从...动画
			public AnimationReferenceAsset to;// 到...动画
			public AnimationReferenceAsset transition;// 过度动画
		}

        // 动画对 字典
		//readonly Dictionary<Spine.AnimationStateData.AnimationPair, Spine.Animation> transitionDictionary = new Dictionary<AnimationStateData.AnimationPair, Animation>(Spine.AnimationStateData.AnimationPairComparer.Instance);

        // 目标动画（正在播放动画）
		public Spine.Animation TargetAnimation { get; private set; }

		void Awake () {
            // 初始化动画引用资源
            foreach (var entry in statesAndAnimations) {
				entry.animation.Initialize();// 初始化动画
			}
			foreach (var entry in transitions) {
				entry.from.Initialize();
				entry.to.Initialize();
				entry.transition.Initialize();// 过渡动画初始化
			}

            // 构建字典
            //foreach (var entry in transitions) {
            //	transitionDictionary.Add(new AnimationStateData.AnimationPair(entry.from.Animation, entry.to.Animation), entry.transition.Animation);
            //}
        }

        /// <summary>根据非零浮点数设置骨骼的水平翻转状态。如果为负，则骨骼被翻转。如果为正，则骨骼不会翻转。</summary>
        public void SetFlip (float horizontal) {
			if (horizontal != 0) {
				skeletonAnimation.Skeleton.ScaleX = horizontal > 0 ? 1f : -1f;
			}
		}

        /// <summary>根据状态名称播放动画。</summary>
		public void PlayAnimationForState (string stateShortName, int layerIndex) {
			PlayAnimationForState(StringToHash(stateShortName), layerIndex);
		}

        /// <summary>根据状态名称的哈希值播放动画。</summary>
		public void PlayAnimationForState (int shortNameHash, int layerIndex) {
			var foundAnimation = GetAnimationForState(shortNameHash);// 寻找动画
			if (foundAnimation == null)
				return;

			PlayNewAnimation(foundAnimation, layerIndex);// 播放新动画
		}

        /// <summary>根据状态名称获取Spine动画。</summary>
		public Spine.Animation GetAnimationForState (string stateShortName) {
			return GetAnimationForState(StringToHash(stateShortName));
		}

        /// <summary>根据状态名称的哈希值获得Spine Animation。</summary>
		public Spine.Animation GetAnimationForState (int shortNameHash) {
			var foundState = statesAndAnimations.Find(entry => StringToHash(entry.stateName) == shortNameHash);
			return (foundState == null) ? null : foundState.animation;// 返回动画
		}

        /// <summary>播放动画。如果定义了过渡动画，则在播放目标动画之前播放过渡动画。</summary>
        public void PlayNewAnimation (Spine.Animation target, int layerIndex) {
			Spine.Animation transition = null;// 过度动画
			Spine.Animation current = null;// 当前动画

			current = GetCurrentAnimation(layerIndex);
			if (current != null)
				transition = TryGetTransition(current, target);// 设置过度动画

			if (transition != null) {// 有过度动画
				skeletonAnimation.AnimationState.SetAnimation(layerIndex, transition, false);// 播放过度动画
				skeletonAnimation.AnimationState.AddAnimation(layerIndex, target, true, 0f);// 添加目标动画在过渡动画之后
			} else {// 无过度动画
				skeletonAnimation.AnimationState.SetAnimation(layerIndex, target, true);// 播放目标动画
			}

			this.TargetAnimation = target;// 更新目标动画
		}

        /// <summary>播放一次非循环动画，然后继续播放状态动画。</summary>
		public void PlayOneShot (Spine.Animation oneShot, int layerIndex) {
			var state = skeletonAnimation.AnimationState;
			state.SetAnimation(0, oneShot, false);// 播放一次动画

			var transition = TryGetTransition(oneShot, TargetAnimation);
			if (transition != null)
				state.AddAnimation(0, transition, false, 0f);// 播放过度动画

			state.AddAnimation(0, this.TargetAnimation, true, 0f);// 播放目标动画
		}

        // 尝试获得过度动画
		Spine.Animation TryGetTransition (Spine.Animation from, Spine.Animation to) {
			foreach (var transition in transitions) {
				if (transition.from.Animation == from && transition.to.Animation == to) {
					return transition.transition.Animation;
				}
			}
			return null;

			//Spine.Animation foundTransition = null;
			//transitionDictionary.TryGetValue(new AnimationStateData.AnimationPair(from, to), out foundTransition);
			//return foundTransition;
		}

        // 获取当前动画
        Spine.Animation GetCurrentAnimation (int layerIndex) {
			var currentTrackEntry = skeletonAnimation.AnimationState.GetCurrent(layerIndex);
			return (currentTrackEntry != null) ? currentTrackEntry.Animation : null;// 返回当前动画 || null
		}

        // 字符串转哈希值
		int StringToHash (string s) {
			return Animator.StringToHash(s);// 从字符串生成参数ID
        }
	}
}
