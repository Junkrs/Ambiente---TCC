import UnityEngine
import json

# Caminho do arquivo
try:
    # Desktop
    file_path = r"C:/Users/junqu/Documents/GitHub/Ambiente---TCC/Assets/Resources/JSON/abacaxi_articulador1.mp4_landmarks.json"
    
    # Notebook
    # file_path = r"C:/Users/Gabriel Junqueira/Desktop/Unity Projects/Ambiente - TCC/Assets/Resources/JSON/abacaxi_articulador1.mp4_landmarks.json"
    with open(file_path, 'r') as file:
        data = json.load(file)
        UnityEngine.Debug.Log("JSON file loaded successfully!")
except FileNotFoundError:
     UnityEngine.Debug.LogError("The JSON file was not found at the specified path")   

# Impressao do nome do video
if 'nome_video' in data:
        video_name = data['nome_video']
        UnityEngine.Debug.Log(f"Nome do video: {video_name}")
else:
    UnityEngine.Debug.LogError("'nome_video' not found in JSON data.")

# Create spheres and store them in a dictionary

# Get the Animator component from the avatar
avatar = UnityEngine.GameObject.Find("Ch24_nonPBR")  # Find the Mixamo avatar in your scene
animator = avatar.GetComponent(UnityEngine.Animator)

# Access the head bone of the avatar (this is where we'll apply the retargeting for the nose landmark)
headBone = animator.GetBoneTransform(UnityEngine.HumanBodyBones.Head)

# Variable to store current frame index
current_frame_index = 0

# Get all the landmarks for every frame
landmarks_frames = data['landmarks_quadros']

avatarScaleFactor = 1.0

# Function to convert MediaPipe coordinates to Unity space
def convert_mediapipe_to_unity(mediapipe_coordinates, scale_factor, avatar_offset):
    # Extract MediaPipe coordinates (x, y, z)
    mediapipe_x = mediapipe_coordinates['x']
    mediapipe_y = mediapipe_coordinates['y']
    mediapipe_z = mediapipe_coordinates['z']

    # Step 1: Invert the y-axis for Unity
    unity_y = 1.0 - mediapipe_y

    # Step 2: Scale the coordinates to match Unity's world space
    unity_x = mediapipe_x * scale_factor
    unity_y *= scale_factor
    unity_z = -mediapipe_z * scale_factor  # Flipping z-axis for Unity

    # Return as Unity Vector3
    return UnityEngine.Vector3(unity_x, unity_y, unity_z)

# Function to update head rotation based on the current frame's landmarks
def update_head_rotation():
    global current_frame_index
    
    if current_frame_index >= len(landmarks_frames):
        UnityEngine.Debug.Log("All frames processed.")
        return

    # Get the current frame's landmarks
    current_frame = landmarks_frames[current_frame_index]

    # Look for "ponto_0 - Nariz" in the current frame
    for quadro_name, pontos in current_frame.items():
        for ponto in pontos:
            for ponto_name, coordinates in ponto.items():
                if ponto_name == "ponto_0 - Nariz":  # This is the nose landmark
                    # Convert MediaPipe coordinates to Unity space
                    nose_position = convert_mediapipe_to_unity(coordinates, avatarScaleFactor, avatar.transform.position)

                    # Get the current head position in Unity
                    head_position = headBone.position

                    # Calculate the direction the head should face (from head to nose)
                    direction = (nose_position - head_position).normalized

                    # Use Quaternion.LookRotation to calculate the head's new rotation
                    targetRotation = UnityEngine.Quaternion.LookRotation(direction)

                    # Smoothly rotate the head bone to the new target rotation
                    rotationSpeed = 5.0
                    headBone.rotation = UnityEngine.Quaternion.Slerp(headBone.rotation, targetRotation, UnityEngine.Time.deltaTime * rotationSpeed)

                    # Debug the new position
                    UnityEngine.Debug.Log(f"Frame {current_frame_index}: Nose position applied to head bone at: {nose_position}")

    # Move to the next frame
    current_frame_index += 1


# This function will be called in Unity's Update method
def Update():
    # Update the head rotation based on the current frame's landmarks
    update_head_rotation()