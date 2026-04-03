using UnityEngine;

/// <summary>
/// 挂载在导线 GameObject 上，每帧更新直线两端点位置，并同步刷新 MeshCollider。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class WireUpdater : MonoBehaviour
{
    private LineRenderer lr;
    private MeshCollider meshCollider;
    private WireConnectionPoint pointA;
    private WireConnectionPoint pointB;
    private Camera cam;

    private Vector3 lastPosA;
    private Vector3 lastPosB;

    public void Initialize(LineRenderer lineRenderer, MeshCollider mc,
        WireConnectionPoint a, WireConnectionPoint b, Camera camera)
    {
        lr = lineRenderer;
        meshCollider = mc;
        pointA = a;
        pointB = b;
        cam = camera;

        lastPosA = a.GetConnectionWorldPosition();
        lastPosB = b.GetConnectionWorldPosition();
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;

        Vector3 posA = pointA.GetConnectionWorldPosition();
        Vector3 posB = pointB.GetConnectionWorldPosition();

        // 只在端点移动时才更新（节省性能）
        if (posA == lastPosA && posB == lastPosB) return;
        lastPosA = posA;
        lastPosB = posB;

        // 更新直线两端
        lr.SetPosition(0, posA);
        lr.SetPosition(1, posB);

        // 同步碰撞 Mesh
        if (meshCollider != null && cam != null)
        {
            Mesh mesh = new Mesh();
            lr.BakeMesh(mesh, cam, true);
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }
}
