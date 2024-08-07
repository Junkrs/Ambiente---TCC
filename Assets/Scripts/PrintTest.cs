using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;

public class MenuItem_PrintTest_Class : MonoBehaviour
{
   [MenuItem("Python Scripts/PrintTest.py")]
   public static void PrintTest()
   {
       PythonRunner.RunFile("Assets/Scripts/PrintTest.py");
   }
};
