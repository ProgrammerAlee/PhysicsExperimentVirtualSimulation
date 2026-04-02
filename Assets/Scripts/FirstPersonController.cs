using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float lookSpeed = 2f;
    public float jumpForce = 5f;

    [Header("平滑参数")]
    [Tooltip("视角平滑强度，值越大越跟手，越小越平滑")]
    public float lookSmoothTime = 0.05f;
    [Tooltip("移动速度平滑强度")]
    public float moveSmoothTime = 0.1f;

    private Camera playerCamera;
    private Rigidbody rb;
    private float rotationX = 0f;
    private bool isGrounded;

    // 视角平滑
    private float smoothMouseX;
    private float smoothMouseY;
    private float mouseXVelocity;
    private float mouseYVelocity;

    // 移动平滑
    private Vector2 currentMoveInput;
    private Vector2 moveSmoothVelocity;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.freezeRotation = true;
        // 开启 Rigidbody 插值，消除物理帧与渲染帧不同步导致的移动抖动
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 对鼠标输入做平滑，消除原始输入抖动
        smoothMouseX = Mathf.SmoothDamp(smoothMouseX, Input.GetAxis("Mouse X"), ref mouseXVelocity, lookSmoothTime);
        smoothMouseY = Mathf.SmoothDamp(smoothMouseY, Input.GetAxis("Mouse Y"), ref mouseYVelocity, lookSmoothTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void LateUpdate()
    {
        // Look - 移至 LateUpdate 确保在物理和移动更新后执行，消除摄像机抖动
        rotationX += -smoothMouseY * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, smoothMouseX * lookSpeed, 0);
    }

    void FixedUpdate()
    {
        // 对移动输入做平滑，避免速度突变
        Vector2 targetInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        currentMoveInput = Vector2.SmoothDamp(currentMoveInput, targetInput, ref moveSmoothVelocity, moveSmoothTime);

        Vector3 moveDirection = (transform.forward * currentMoveInput.y + transform.right * currentMoveInput.x).normalized * walkSpeed;

        // 使用 MovePosition 替代直接修改 velocity，这在开启 Interpolate 时能提供更平滑的移动
        Vector3 targetPosition = rb.position + moveDirection * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
