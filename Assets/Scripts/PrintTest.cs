using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;

public class PrintTest : MonoBehaviour
{
   void Start()
   {
       PythonRunner.RunFile("Assets/Scripts/PrintTest.py");
   }
}
