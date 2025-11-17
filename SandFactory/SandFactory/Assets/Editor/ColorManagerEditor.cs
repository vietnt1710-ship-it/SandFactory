// File 2: Editor Script (đặt trong Assets/Editor/)
using ToolLevel;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColorManager))]
public class ColorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Vẽ Inspector mặc định

        ColorManager manager = (ColorManager)target;

        if (GUILayout.Button("Load Color"))
        {
            manager.LoadColor(); // Gọi hàm khi bấm button
        }
        if (GUILayout.Button("Clear"))
        {
            manager.Clear(); // Gọi hàm khi bấm button
        }
    }
    //private void OnSceneGUI()
    //{
    //    // Thêm control để ưu tiên xử lý sự kiện
    //    int controlID = GUIUtility.GetControlID(FocusType.Passive);
    //    HandleUtility.AddDefaultControl(controlID);

    //    Event e = Event.current;
    //    Vector2 mousePos = e.mousePosition;

    //    // Chuyển đổi sang world position
    //    Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
    //    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    //    if (groundPlane.Raycast(ray, out float distance))
    //    {
    //        Vector3 worldPos = ray.GetPoint(distance);

    //        // Visual feedback - vẽ sphere tại vị trí chuột
    //        Handles.color = Color.cyan;
    //        Handles.SphereHandleCap(0, worldPos, Quaternion.identity, 0.2f, EventType.Repaint);

    //        // Hiển thị tọa độ
    //        GUIStyle style = new GUIStyle();
    //        style.normal.textColor = Color.white;
    //        style.fontSize = 12;
    //        Handles.Label(worldPos + Vector3.up * 0.3f, $"Pos: {worldPos}", style);
    //    }

    //    // Xử lý click chuột trái
    //    if (e.type == EventType.MouseDown && e.button == 0)
    //    {
    //        if (groundPlane.Raycast(ray, out distance))
    //        {
    //            Vector3 clickPos = ray.GetPoint(distance);
    //            Debug.Log($"CLICKED at: {clickPos}");

    //            // Tạo cube tại vị trí click để kiểm tra
    //            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //            cube.transform.position = clickPos;
    //            cube.transform.localScale = Vector3.one * 0.5f;
    //        }

    //        e.Use(); // QUAN TRỌNG: Đánh dấu sự kiện đã xử lý
    //    }

    //    // Buộc repaint liên tục để cập nhật vị trí chuột
    //    SceneView.RepaintAll();
    //}
}