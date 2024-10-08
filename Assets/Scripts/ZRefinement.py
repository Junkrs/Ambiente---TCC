import torch
import torch.nn as nn
import json

# Load the JSON file (replace the path with the actual path)
with open('C:\Users\Gabriel Junqueira\Desktop\Unity Projects\Ambiente - TCC\Assets\Resources\JSON\abacaxi_articulador1.mp4_landmarks.json', 'r') as file:
    data = json.load(file)

# Extract the landmarks for a specific frame
# Assuming 'landmarks_quadros' is the key in your JSON file
landmarks = data['landmarks_quadros'][0]  # Example: First frame


# Define a simple neural network for depth refinement
class DepthRefinementNet(nn.Module):
    def __init__(self):
        super(DepthRefinementNet, self).__init__()
        self.fc1 = nn.Linear(3, 64)  # 3 input features (x, y, z from MediaPipe)
        self.fc2 = nn.Linear(64, 128)
        self.fc3 = nn.Linear(128, 64)
        self.fc4 = nn.Linear(64, 1)  # Output refined z-coordinate

    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = torch.relu(self.fc2(x))
        x = torch.relu(self.fc3(x))
        refined_z = self.fc4(x)
        return refined_z

# Initialize the model
model = DepthRefinementNet()

# Collect all x, y, and z coordinates as inputs for the NN
# Assuming 'landmarks' is a dictionary where each landmark has x, y, z values
landmark_inputs = []

for quadro_name, pontos in landmarks.items():
    for ponto in pontos:
        for key, coordinates in ponto.items():
            x = coordinates['x']
            y = coordinates['y']
            z = coordinates['z']
            # Append (x, y, z) as input for the NN
            landmark_inputs.append([x, y, z])

# Convert the inputs to a tensor for the neural network
landmark_tensor = torch.tensor(landmark_inputs, dtype=torch.float32)

# Assume model is your pre-defined neural network from the earlier example
# Pass the input through the network to get refined z-values
refined_z_values = model(landmark_tensor)

# Print or store the refined z-values for further use
print(refined_z_values)
