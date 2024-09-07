using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;

public class MPpythonJSONparser : MonoBehaviour
{
    private Animator animator;
    private Transform headBone;
    private Vector3 nosePosition;
    private float rotationSpeed = 5.0f;

    // This will run the Python script once when the scene starts
    void Start()
    {
        // Run the Python script to load JSON and set up initial conditions
        PythonRunner.RunFile("Assets/Scripts/MPpythonJSONparser.py");

        // Get the animator and head bone once Python has set up the avatar
        GameObject avatar = GameObject.Find("MixamoAvatar");  // Ensure this matches your avatar's name in Unity
        if (avatar != null)
        {
            animator = avatar.GetComponent<Animator>();
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }

    // Animation part moved to Update() for per-frame head movement
    void Update()
    {
        if (headBone != null)
        {
            // This is where you'll continuously update the head bone's rotation based on nose position
            // The Python script should have already calculated and provided nosePosition
            UpdateHeadRotation();
        }
    }

    // Function to handle head rotation based on nose landmark (can be adjusted)
    void UpdateHeadRotation()
    {
        // Example of how you can update the head's rotation using a direction
        // Assuming the Python script sets 'nosePosition' properly after running once

        // Calculate the direction the head should face (from head to nose)
        Vector3 headPosition = headBone.position;
        Vector3 direction = (nosePosition - headPosition).normalized;

        // Use Quaternion.LookRotation to calculate the head's new rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate the head bone to the new target rotation
        headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
