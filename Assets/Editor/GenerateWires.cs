using UnityEngine;

public class GenerateWires
{
    public static void Execute()
    {
        // Find terminals
        GameObject batPos = GameObject.Find("Enviro/电池/Terminal_Positive");
        GameObject batNeg = GameObject.Find("Enviro/电池/Terminal_Negative");
        GameObject volPos = GameObject.Find("DigitalVoltmeter/Terminal_Pos3");
        GameObject volNeg = GameObject.Find("DigitalVoltmeter/Terminal_Neg");

        // Fallback find if exact path fails
        if (batPos == null) batPos = GameObject.Find("Terminal_Positive");
        if (batNeg == null) batNeg = GameObject.Find("Terminal_Negative");
        if (volPos == null) volPos = GameObject.Find("Terminal_Pos3");
        if (volNeg == null) volNeg = GameObject.Find("Terminal_Neg");

        if (batPos != null && volPos != null)
        {
            // Connect positive to positive (Red wire)
            CreateWire("Wire_Positive", batPos.transform.position + Vector3.up * 0.02f, volPos.transform.position + Vector3.up * 0.02f, new Color(0.8f, 0.1f, 0.1f));
        }
        else
        {
            Debug.LogError("Could not find positive terminals.");
        }

        if (batNeg != null && volNeg != null)
        {
            // Connect negative to negative (Black wire)
            CreateWire("Wire_Negative", batNeg.transform.position + Vector3.up * 0.02f, volNeg.transform.position + Vector3.up * 0.02f, new Color(0.1f, 0.1f, 0.1f));
        }
        else
        {
            Debug.LogError("Could not find negative terminals.");
        }
    }

    private static void CreateWire(string name, Vector3 start, Vector3 end, Color color)
    {
        // Remove existing wire if any
        GameObject existing = GameObject.Find(name);
        if (existing != null) Object.DestroyImmediate(existing);

        GameObject wireObj = new GameObject(name);
        LineRenderer lr = wireObj.AddComponent<LineRenderer>();
        
        // Material
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Glossiness", 0.3f);
        mat.SetFloat("_Metallic", 0.1f);
        lr.material = mat;
        
        lr.startWidth = 0.006f;
        lr.endWidth = 0.006f;
        lr.useWorldSpace = true;
        lr.numCapVertices = 5;
        lr.numCornerVertices = 5;
        
        int segments = 40;
        lr.positionCount = segments + 1;
        
        float dist = Vector3.Distance(start, end);
        float sag = dist * 0.4f; // Adjust sag based on distance
        
        // Bezier control points
        Vector3 p0 = start;
        Vector3 p3 = end;
        // Control points go slightly up then sag down
        Vector3 p1 = p0 + Vector3.up * 0.05f + (p3 - p0).normalized * dist * 0.2f + Vector3.down * sag;
        Vector3 p2 = p3 + Vector3.up * 0.05f - (p3 - p0).normalized * dist * 0.2f + Vector3.down * sag;
        
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pos = CalculateBezierPoint(t, p0, p1, p2, p3);
            lr.SetPosition(i, pos);
        }
    }

    private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }
}
