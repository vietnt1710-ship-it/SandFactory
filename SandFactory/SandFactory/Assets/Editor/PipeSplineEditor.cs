#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PipeSpline))]
public class PipeSplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PipeSpline pipeSpline = (PipeSpline)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate Pipe", GUILayout.Height(30)))
        {
            pipeSpline.GeneratePipe();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "HƯỚNG DẪN SỬ DỤNG:\n\n" +
            "1. Tạo Empty GameObjects làm control points\n" +
            "2. Kéo chúng vào mảng Control Points theo thứ tự\n" +
            "3. Di chuyển các points để tạo đường cong\n" +
            "4. Điều chỉnh Pipe Radius và Radial Segments\n\n" +
            "LƯU Ý:\n" +
            "- Tối thiểu 2 control points\n" +
            "- Control points phải đúng thứ tự\n" +
            "- Tăng Spline Resolution nếu đường cong bị góc",
            MessageType.Info
        );
    }
}
#endif
