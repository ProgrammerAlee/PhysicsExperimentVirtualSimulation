using UnityEngine;

// 挂载了这个脚本的物体，代表它是从背包里拿出来放置在场景中的道具
public class PlacedProp : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;
    private float fallTimer = 0f; // 增加一个计时器

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 如果刚体本身是运动学的，就没必要等待落地了
        if (rb != null && rb.isKinematic)
        {
            hasLanded = true;
        }
    }

    void Update()
    {
        if (hasLanded || rb == null) return;

        // 累加时间
        fallTimer += Time.deltaTime;

        // 至少给它 0.5 秒的时间去“掉落”（因为刚生成的第一帧速度是 0，会误判为已经落地）
        if (fallTimer > 0.5f)
        {
            // 如果刚体的速度极其微小（接近静止）且不再受到重力带来的加速，就视为落地
            if (rb.velocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f)
            {
                hasLanded = true;
                // 落地后直接将刚体锁死为 Kinematic
                rb.isKinematic = true;
            }
        }
    }
}