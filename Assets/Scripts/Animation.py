import UnityEngine
import json

# Time tracking
start_time = UnityEngine.Time.time
delay = 10.0  # 10 seconds delay

# Move spheres after the delay

current_time = UnityEngine.Time.time
if current_time - start_time >= delay:
    for i in range(1, len(data['landmarks_quadros'])):
        current_frame = data['landmarks_quadros'][i]
        for quadro_name, pontos in current_frame.items():
            for ponto in pontos:
                for ponto_name, new_coordinates in ponto.items():
                    if ponto_name in spheres:
                        sphere = spheres[ponto_name]
                        sphere.transform.position = UnityEngine.Vector3(new_coordinates['x'], new_coordinates['y'], new_coordinates['z'])

        UnityEngine.Debug.Log(f"Updated positions for frame {i}")
    # Reset the start_time to apply delay before the next frame
    start_time = current_time

#UnityEngine.Debug.Log("Movement script initialized.")