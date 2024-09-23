import UnityEngine
import json
import math

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

point_names = {
    0: "Nariz",
    1: "Olho_Esquerdo_Interno",
    2: "Olho_Esquerdo",
    3: "Olho_Esquerdo_Externo",
    4: "Olho_Direito_Interno",
    5: "Olho_Direito",
    6: "Olho_Direito_Externo",
    7: "Orelha_Esquerda",
    8: "Orelha_Direita",
    9: "Boca_Esquerda",
    10: "Boca_Direita",
    11: "Ombro_Esquerdo",
    12: "Ombro_Direito",
    13: "Cotovelo_Esquerdo",
    14: "Cotovelo_Direito",
    15: "Pulso_Esquerdo",
    16: "Pulso_Direito",
    17: "Dedo_Mindinho_Esquerdo",
    18: "Dedo_Mindinho_Direito",
    19: "Dedo_Indicador_Esquerdo",
    20: "Dedo_Indicador_Direito",
    21: "Dedao_Mao_Esquerda",
    22: "Dedao_Mao_Direita",
    23: "Quadril_Esquerdo",
    24: "Quadril_Direito",
    25: "Joelho_Esquerdo",
    26: "Joelho_Direito",
    27: "Tornozelo_Esquerdo",
    28: "Tornozelo_Direito",
    29: "Calcanhar_Esquerdo",
    30: "Calcanhar_Direito",
    31: "Dedo_Indicador_Pe_Esquerdo",
    32: "Dedo_Indicador_Pe_Direito",
}

# Lista de pontos para serem ignorados
ignored_points = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 17, 18, 19, 20, 21, 22, 25, 26, 27, 28, 29, 30, 31, 32}

# Pontos conectados segundo MediaPipe Pose
connections = [
    (0, 11),  # Nariz to Ombro_Esquerdo
    (0, 12),  # Nariz to Ombro_Direito
    (11, 12), # Ombro_esquerdo to Ombro_direito
    (11, 13), # Ombro_Esquerdo to Cotovelo_Esquerdo
    (13, 15), # Cotovelo_Esquerdo to Pulso_Esquerdo
    (12, 14), # Ombro_Direito to Cotovelo_Direito
    (14, 16), # Cotovelo_Direito to Pulso_Direito
    (23, 24), # Quadril_Esquerdo to Quadril_Direito
    (23, 11), # Quadril_Esquerdo to Ombro_Esquerdo
    (24, 12), # Quadril_Direito to Ombro_Direito
]

# Função para calcular a distância entre dois pontos
def calculate_distance(point1, point2):
    x1, y1, z1 = point1['x'], point1['y'], point1['z']
    x2, y2, z2 = point2['x'], point2['y'], point2['z']
    
    distance = math.sqrt((x2 - x1) ** 2 + (y2 - y1) ** 2 + (z2 - z1) ** 2)
    return distance

# Cria as esferas e calcula as distâncias
spheres = {}
distances = []
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

            if ponto1 not in ignored_points and ponto2 not in ignored_points:
                point1_name = f"{ponto_name}"
                point2_name = f"{ponto_name}"
                
                # Obtem as coordenadas dos pontos conectados
                ponto1 = first_frame[point1_name]
                ponto2 = first_frame[point2_name]
                
                # Calcula a distância entre os dois pontos
                distance = calculate_distance(ponto1, ponto2)
                distances.append((point1_name, point2_name, distance))

# Imprime as distâncias calculadas
for point1, point2, distance in distances:
    UnityEngine.Debug.Log(f"Distância entre {point_names[int(point1.split('_')[1])]} e {point_names[int(point2.split('_')[1])]}: {distance}")