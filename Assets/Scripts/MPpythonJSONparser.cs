using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;

public class MPpythonJSONparser : MonoBehaviour
{
    void Start()
    {
        PythonRunner.RunFile("Assets/Scripts/MPpythonJSONparser.py");
    }

    private void Update()
    {
        PythonRunner.RunFile("Assets/Scripts/Animation.py");
    }
}
