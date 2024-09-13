using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;

public class MPpythonJSONparser : MonoBehaviour
{

    // This will run the Python script once when the scene starts
    void Start()
    {
        // Run the Python script to load JSON and set up initial conditions
        PythonRunner.RunFile("Assets/Scripts/MPpythonJSONparser.py");
    }
}
