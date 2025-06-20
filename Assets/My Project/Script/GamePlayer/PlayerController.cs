using UnityEngine;

namespace Unity.Game.Player
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]

    public class PlayerController : MonoBehaviour
    {
        [Header("データ")]
        [SerializeField, Tooltip("アニメーション再生速度")] float animatorSpeed = 1.5f;
        [SerializeField, Tooltip("跳躍時のコライダ調整")] bool useCurves = true;
        [SerializeField, Tooltip("入力状態")] bool inputEnabled = true;
        [SerializeField, Tooltip("前進速度")] float forwardSpeed = 13.0f;
        [SerializeField, Tooltip("後退速度")] float backwardSpeed = 6.0f;
        [SerializeField, Tooltip("旋回速度")] float rotateSpeed = 450.0f;
        [SerializeField, Tooltip("跳躍力")] float jumpPower = 20.0f;
        [SerializeField, Tooltip("加速度")] float acceleration = 40.0f;
        [SerializeField, Tooltip("重力")] float gravity = 40.0f;

        AnimatorStateInfo currentBaseState; // 現アニメーション
        Vector3 directVelocity;    // 現速度 (カメラ基準)
        Vector3 orgVectColCenter;   // コライダ初期値 (中心)
        Vector3 moveDelta;  // 移動量
        const float coyoteDelay = 0.1f; // コヨーテタイム
        float airborneTime; // 滞空時間
        float orgColHight;  // コライダ初期値 (高さ)
        float directRotate; // 現回転速度 (カメラ基準)
        float speed;    // 速さ
        bool airborne;  // 空中

        // 参照
        Animator animator;
        CharacterController controller;
        Transform ground;
        static readonly int speedHash = Animator.StringToHash("Speed");
        static readonly int jumpHash = Animator.StringToHash("Jump");
        static readonly int jumpHeightHash = Animator.StringToHash("JumpHeight");
        static readonly int locoState = Animator.StringToHash("Base Layer.Locomotion");
        static readonly int jumpState = Animator.StringToHash("Base Layer.Jump");

        void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            orgColHight = controller.height;
            orgVectColCenter = controller.center;
            animator.speed = animatorSpeed;
        }

        void Update()
        {
            if (inputEnabled)
            {
                // シーン基準のベクトルを取得
                var right = Vector3.right;
                var forward = Vector3.forward;

                // カメラ基準のベクトルを取得
                if (Camera.main)
                {
                    right = Camera.main.transform.right;
                    right.y = 0.0f;
                    right.Normalize();
                    forward = Camera.main.transform.forward;
                    forward.y = 0.0f;
                    forward.Normalize();
                }

                var h = Input.GetAxisRaw("Horizontal");    // 左右キー
                var velocity_h = right * h;
                var v = Input.GetAxisRaw("Vertical");  // 前後キー
                var velocity_v = forward * v;

                var targetVelocity = velocity_h + velocity_v;  // 目標速度

                if (targetVelocity.sqrMagnitude > 0.0f)
                {
                    targetVelocity.Normalize();
                }

                if (v > 0.1f || h != 0.0f)    // 前進
                {
                    targetVelocity *= forwardSpeed;
                }
                else if (v < -0.1f || h != 0.0f)  // 後退
                {
                    targetVelocity *= backwardSpeed;
                }

                var velocityDiff = targetVelocity - directVelocity; // 速度差

                if (velocityDiff.sqrMagnitude < acceleration * acceleration * Time.deltaTime * Time.deltaTime)  // 1フレームで目標速度に到達可能
                {
                    directVelocity = targetVelocity;
                }
                else if (velocityDiff.sqrMagnitude > 0.0f)  // 目標速度未到達
                {
                    velocityDiff.Normalize();
                    directVelocity += velocityDiff * acceleration * Time.deltaTime; // 速度加算
                }

                speed = directVelocity.magnitude;
                directRotate = 0.0f;

                if (targetVelocity.sqrMagnitude > 0.0f)
                {
                    var localTargetVelocity = transform.InverseTransformDirection(targetVelocity);
                    var angleDiff = Vector3.SignedAngle(Vector3.forward, localTargetVelocity.normalized, Vector3.up);   // キャラの正面方向と目標速度の角度差

                    if (angleDiff > 0.0f)
                    {
                        directRotate = rotateSpeed;   // 右回転
                    }
                    else if (angleDiff < 0.0f)
                    {
                        directRotate = -rotateSpeed; // 左回転
                    }
                }

                moveDelta = new Vector3(directVelocity.x, moveDelta.y, directVelocity.z);

                if (Input.GetButtonDown("Jump"))    // スペースキー
                {
                    if (!airborne)  // 地上
                    {
                        moveDelta.y = jumpPower;
                        animator.SetTrigger(jumpHash);
                        airborne = true;
                    }
                }

                Motion();
            }
        }

        // キャラクタ移動, 回転, アニメーション
        void Motion()
        {
            var wasGrounded = controller.isGrounded;    // 接地状態 (移動前)

            if (!controller.isGrounded) // 空中
            {
                moveDelta.y -= gravity * Time.deltaTime;    // 重力適用
                airborneTime += Time.deltaTime;
                ground = null;
            }

            controller.Move(moveDelta * Time.deltaTime);
            transform.Rotate(0, directRotate * Time.deltaTime, 0);

            if (!wasGrounded && controller.isGrounded)  // 着地 (空中 → 地上)
            {
                moveDelta.y = 0.0f;
                airborneTime = 0.0f;
            }

            airborne = airborneTime >= coyoteDelay;
            UpdateAnimation();
        }

        // コライダ初期化
        void resetCollider()
        {
            controller.height = orgColHight;
            controller.center = orgVectColCenter;
        }

        // アニメーション更新
        void UpdateAnimation()
        {
            AnimatorStateInfo currentBaseState = animator.GetCurrentAnimatorStateInfo(0);   // ベースレイヤ
            animator.SetFloat(speedHash, speed);

            if (currentBaseState.fullPathHash == locoState)
            {
                if (useCurves)
                {
                    resetCollider();
                }
            }
            else if (currentBaseState.fullPathHash == jumpState)
            {
                if (useCurves)
                {
                    var jumpHeight = animator.GetFloat(jumpHeightHash);   // JumpHeight → アニメーション (JUMP00) から設定
                    var adjustCenterY = orgVectColCenter.y + jumpHeight;
                    controller.height = orgColHight - jumpHeight;
                    controller.center = new Vector3(0, adjustCenterY, 0);
                }
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (controller.isGrounded)  // 地上
            {
                RaycastHit raycastHit;

                if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, 0.5f, Vector3.down, out raycastHit, 0.1f, -1, QueryTriggerInteraction.Ignore))   // 接地
                {
                    ground = raycastHit.collider.transform;  // 接地地面
                }
            }
        }
    }
}
