using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;

namespace Spine.Unity.Examples {

    /// <summary>
    /// 基本的游戏平台控制器
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
	public class BasicPlatformerController : MonoBehaviour {

        // 角色状态
        public enum CharacterState {
			None,
			Idle,
			Walk,
			Run,
			Crouch,
			Rise,
			Fall,
			Attack
		}

        // 角色控制器
        [Header("Components")]
		public CharacterController controller;

        // 控制项
		[Header("Controls")]
		public string XAxis = "Horizontal";// 水平
		public string YAxis = "Vertical";// 竖直
		public string JumpButton = "Jump";// 跳
        public KeyCode attackKey = KeyCode.Return;// 攻击

        // 移动
        [Header("Moving")]
		public float walkSpeed = 1.5f;// 行走速度
		public float runSpeed = 7f;// 奔跑速度
		public float gravityScale = 6.6f;// 重力缩放

        // 跳跃
        [Header("Jumping")]
		public float jumpSpeed = 25;// 跳跃速度
		public float minimumJumpDuration = 0.5f;// 最小跳跃持续时间
        public float jumpInterruptFactor = 0.5f;// 跳跃中断
		public float forceCrouchVelocity = 25;// 落下时速度
		public float forceCrouchDuration = 0.5f;// 蹲伏持续时间
        public float forceAttackDuration = 0.1f;// 攻击持续时间*

        // 骨骼动画处理器
        [Header("Animation")]
		public SkeletonAnimationHandleExample animationHandle;

		// 事件
		public event UnityAction OnJump, OnLand, OnHardLand;

		Vector2 input = default(Vector2);// 输入控制
		Vector3 velocity = default(Vector3);// 速度控制
		float minimumJumpEndTime = 0;// 最小跳跃结束时间
		float forceCrouchEndTime;// 落下蹲伏动作结束时间
        float forceAttackEndTime;// 攻击动作结束时间
        bool wasGrounded = false;// 落到地面时

        // 角色状态
        CharacterState previousState, currentState;// 以前状态 当前状态

		void Update () {
			float dt = Time.deltaTime;
			bool isGrounded = controller.isGrounded;// 在地面
			bool landed = !wasGrounded && isGrounded;// 登录地面

			// 检测输入
			input.x = Input.GetAxis(XAxis);
			input.y = Input.GetAxis(YAxis);
			bool inputJumpStop = Input.GetButtonUp(JumpButton);// 跳跃停止
			bool inputJumpStart = Input.GetButtonDown(JumpButton);// 跳跃开始
			bool doCrouch = (isGrounded && input.y < -0.5f) || (forceCrouchEndTime > Time.time);// 蹲下
			bool doJumpInterrupt = false;// 跳跃中断
			bool doJump = false;// 跳跃
			bool hardLand = false;// 硬着陆

            bool inputAttackStart = Input.GetKey(attackKey);// 攻击开始*
            bool inputAttackStop = Input.GetKeyUp(attackKey);// 攻击结束*
            bool doAttack = inputAttackStart;
            //bool doAttack = inputAttackStart || (forceAttackEndTime > Time.time);// 攻击*
            //if (inputAttackStart)
            //{
            //    forceAttackEndTime = Time.time + forceAttackDuration;
            //}

            //** 判定动作变化 **//
            if (landed) {// 如果落到地面上
				if (-velocity.y > forceCrouchVelocity) {// 如果垂直速度大于 落下速度值
					hardLand = true;// 硬着陆
					doCrouch = true;// 做蹲下动作
					forceCrouchEndTime = Time.time + forceCrouchDuration;// 落下蹲伏动作结束时间
                }
			}

			if (!doCrouch) {// 如果不是做蹲下动作
				if (isGrounded) {// 在地面上
                    if (!doAttack)//*
                    {
                        if (inputJumpStart)
                        {// 如果输入跳跃
                            doJump = true;// 做跳跃
                        }
                    }
                } else {// 不在地面上
					doJumpInterrupt = inputJumpStop && Time.time < minimumJumpEndTime;// 跳跃中断 = 跳跃停止 && 最小跳跃结束时间
                }
			}

            //** 虚拟物理和控制器使用（计算速度）**//
            // 重力加速度
            Vector3 gravityDeltaVelocity = Physics.gravity * gravityScale * dt;// 重力 * 重力缩放 * 这帧时间

			if (doJump) {// 做跳跃动作
				velocity.y = jumpSpeed;// 竖直速度为跳跃速度
				minimumJumpEndTime = Time.time + minimumJumpDuration;// 跳跃结束时间
			} else if (doJumpInterrupt) {// 如果跳跃被中断
				if (velocity.y > 0)
					velocity.y *= jumpInterruptFactor;// 跳跃中断速率
			}

			velocity.x = 0;// 水平速度为0

            //if (!doCrouch)
            //{// 如果不是做蹲下动作
            if (input.x != 0)
            {// 水平轴有输入
                if (doCrouch)//*
                {// 如果不是做蹲下动作
                }
                else if (!isGrounded)//*
                {
                    velocity.x = Mathf.Abs(input.x) > 0.6f ? runSpeed : walkSpeed;// 跑 || 走
                    velocity.x *= Mathf.Sign(input.x);// 左 || 右
                }
                else if (doAttack)//*
                {
                }
                else
                {
                    velocity.x = Mathf.Abs(input.x) > 0.6f ? runSpeed : walkSpeed;// 跑 || 走
                    velocity.x *= Mathf.Sign(input.x);// 左 || 右
                }
            }
            //}

            if (!isGrounded)
            {// 如果不在地面上
                if (wasGrounded)
                {// 落到地面时
                    if (velocity.y < 0)
                        velocity.y = 0;// 竖直速度变为0
                }
                else
                {// 在空中
                    velocity += gravityDeltaVelocity;// 竖直速度为重力加速度
                }
            }
			controller.Move(velocity * dt);// 设置人物速度
			wasGrounded = isGrounded;

            //** 确定并存储角色状态 **//
            if (doAttack) {//*
                currentState = CharacterState.Attack;
            }
            else {//*
                if (isGrounded)
                {
                    if (doCrouch)
                    {
                        currentState = CharacterState.Crouch;// 蹲
                    }
                    else
                    {
                        if (input.x == 0)
                            currentState = CharacterState.Idle;// 闲置
                        else
                            currentState = Mathf.Abs(input.x) > 0.6f ? CharacterState.Run : CharacterState.Walk;// 跑 || 走
                    }
                }
                else
                {
                    currentState = velocity.y > 0 ? CharacterState.Rise : CharacterState.Fall;// 上升 || 下落
                }
            }

			bool stateChanged = previousState != currentState;// 状态是否改变
			previousState = currentState;// 状态重置

            // Animation
            // 在此阶段请勿修改字符参数或状态。只是读他们。
            // 检测状态变化，并在变化时进行动画处理。
            if (stateChanged)
				HandleStateChanged();// 改变动画状态

			if (input.x != 0)
				animationHandle.SetFlip(input.x);// 设置动画翻转

			// 触发事件（事件调用 播放粒子||声音）
			if (doJump) {
				OnJump.Invoke();// 触发跳跃事件
			}
			if (landed) {
				if (hardLand) {
					OnHardLand.Invoke();
				} else {
					OnLand.Invoke();
				}
			}
		}

        // 改变动画状态
        void HandleStateChanged () {
            // 状态更改时，将新状态通知动画处理器。
            string stateName = null;
			switch (currentState) {
				case CharacterState.Idle:// 角色状态
					stateName = "idle";
					break;
				case CharacterState.Walk:
					stateName = "walk";
					break;
				case CharacterState.Run:
					stateName = "run";
					break;
				case CharacterState.Crouch:
					stateName = "crouch";
					break;
				case CharacterState.Rise:
					stateName = "rise";
					break;
				case CharacterState.Fall:
					stateName = "fall";
					break;
				case CharacterState.Attack:
					stateName = "attack";
                    //Animation a = animationHandle.GetAnimationForState(stateName);
                    //animationHandle.PlayOneShot(a, 0);
                    //return;
                    break;
				default:
					break;
			}

            animationHandle.PlayAnimationForState(stateName, 0);// 播放状态的动画
        }

	}
}
