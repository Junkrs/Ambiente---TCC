import UnityEngine
import json

# Caminho do arquivo
try:
    # Desktop
    # file_path = r"C:/Users/junqu/Documents/GitHub/Ambiente---TCC/Assets/Resources/JSON/abacaxi_articulador1.mp4_landmarks.json"
    
    # Notebook
    file_path = r"C:/Users/Gabriel Junqueira/Desktop/Unity Projects/Ambiente - TCC/Assets/Resources/JSON/abacaxi_articulador1.mp4_landmarks.json"
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

# Iteracao em cada um dos frames do video
for frame in data['landmarks_quadros']:
    for quadro_name, pontos in frame.items():

        UnityEngine.Debug.Log(f"Quadro: {quadro_name}")
        # Iteracao em cada um dos pontos
        for ponto in pontos:
            for ponto_name, coordinates in ponto.items():
                x = coordinates['x']
                y = coordinates['y']
                z = coordinates['z']
                UnityEngine.Debug.Log(f"{ponto_name} - x: {x}, y: {y}, z: {z}")
