import UnityEngine
import json
from Scripting_Python import UnityScripting

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

# Cria as esferas e as coloca dentro de um dicionario
spheres = {}
first_frame = data['landmarks_quadros'][0]
for quadro_name, pontos in first_frame.items():
    for ponto in pontos:
        for ponto_name, coordinates in ponto.items():
            sphere = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere)
            sphere.name = ponto_name

            #escala relacioando ao tamanho do avatar //ignorar por enquanto pois ainda nao medi isso
            avatarScaleFactor = 1.0

            #transformacao da posicao das esferas
            sphere.transform.position = UnityEngine.Vector3(coordinates['x'] * avatarScaleFactor, 1.0 - coordinates['y'] * avatarScaleFactor, -coordinates['z'] * 0.23) 

            scale_factor = 0.05
            sphere.transform.localScale = UnityEngine.Vector3(scale_factor, scale_factor, scale_factor)
            sphere_renderer = sphere.GetComponent(UnityEngine.Renderer)
            sphere_renderer.material.color = UnityEngine.Color.red

            spheres[ponto_name] = sphere