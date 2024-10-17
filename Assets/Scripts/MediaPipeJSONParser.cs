using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// Estrutura auxiliar para armazenar os dados do JSON
[System.Serializable]
public class RootObject
{
    public string nome_video { get; set; }
    public List<Dictionary<string, List<Dictionary<string, Landmark>>>> landmarks_quadros { get; set; }
}

[System.Serializable]
public class Landmark
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
}


public class MediaPipeJSONParser : MonoBehaviour
{
    // string file_path = "D:/Downloads/Ambiente---TCC/Assets/Resources/JSON/abacaxi_articulador1.mp4_landmarks.json";

    public GameObject spherePrefab; // Prefab da esfera vermelha
    public GameObject cylinderPrefab; // Prefab da merda do cilindro
    private Dictionary<string, List<Dictionary<string, Vector3>>> framesLandmarks; // Para armazenar os landmarks de cada quadro
    private List<GameObject> spheres; // Esferas que representarão os pontos
    private List<GameObject> cylinders; // Cilindros que conectarão as esferas
    private int currentFrame = 0; // Quadro atual da animação
    public float avatarScaleFactor = 1.0f;

    private Dictionary<int, string> pointNames = new Dictionary<int, string>()
    {
        { 0, "Nariz" },
        { 1, "Ombro_Esquerdo" },
        { 2, "Ombro_Direito" },
        { 3, "Cotovelo_Esquerdo" },
        { 4, "Cotovelo_Direito" },
        { 5, "Pulso_Esquerdo" },
        { 6, "Pulso_Direito" },
        { 7, "Quadril_Esquerdo" },
        { 8, "Quadril_Direito" }
    };

    // Define sphere scale factors for different body parts
    /*private Dictionary<string, Vector3> sphereScaleMap = new Dictionary<string, Vector3>()
    {
        { "Nariz", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Ombro_Esquerdo", new Vector3(0.3f, 0.3f, 0.3f) },
        { "Ombro_Direito", new Vector3(0.3f, 0.3f, 0.3f) },
        { "Cotovelo_Esquerdo", new Vector3(0.2f, 0.2f, 0.2f) },
        { "Cotovelo_Direito", new Vector3(0.2f, 0.2f, 0.2f) },
        { "Pulso_Esquerdo", new Vector3(0.15f, 0.15f, 0.15f) },
        { "Pulso_Direito", new Vector3(0.15f, 0.15f, 0.15f) },
        { "Quadril_Esquerdo", new Vector3(0.3f, 0.3f, 0.3f) },
        { "Quadril_Direito", new Vector3(0.3f, 0.3f, 0.3f) }
    };*/

    // List of specific connections to make between spheres
    private List<(int, int)> connections = new List<(int, int)>
    {
        (1, 2), // Ombro_Esquerdo -> Ombro_Direito
        (1, 3), // Ombro_Esquerdo -> Cotovelo_Esquerdo
        (3, 5), // Cotovelo_Esquerdo -> Pulso_Esquerdo
        (2, 4), // Ombro_Direito -> Cotovelo_Direito
        (4, 6), // Cotovelo_Direito -> Pulso_Direito
        (1, 7), // Ombro_Esquerdo -> Quadril_Esquerdo
        (2, 8), // Ombro_Direito -> Quadril_Direito
        (7, 8)  // Quadril_Esquerdo -> Quadril_Direito
    };

    void Start() {
        // Carregar o JSON
        string jsonPath = Application.dataPath + "/Resources/JSON/abacaxi_articulador1.mp4_landmarks.json"; // Coloque o arquivo JSON na pasta "Assets"
        string jsonString = File.ReadAllText(jsonPath);

        // Decodificar o JSON em uma estrutura apropriada
        var data = JsonConvert.DeserializeObject<RootObject>(jsonString);
        Debug.Log("Nome do vídeo: " + data.nome_video);
        Debug.Log("Quantidade de quadros: " + data.landmarks_quadros.Count);

        // Preparar as esferas para os landmarks e os cilindros para conexão
        spheres = new List<GameObject>();
        cylinders = new List<GameObject>();

        // Inicializar as esferas para os pontos do primeiro quadro
        int index = 0;
        foreach (var landmark in data.landmarks_quadros[0]["quadro_0"])
        {
            GameObject sphere = Instantiate(spherePrefab);
            sphere.GetComponent<Renderer>().material.color = Color.red; // Red color for the spheres

            // Set the name of the sphere based on the point index
            if (pointNames.ContainsKey(index))
            {
                sphere.name = pointNames[index];
                spheres.Add(sphere);

                // Apply scale based on body part
                /*if (sphereScaleMap.ContainsKey(sphere.name))
                {
                    sphere.transform.localScale = sphereScaleMap[sphere.name];
                }*/
            }
            else
            {
                continue;
            }
            index++;
        }

        // Instantiate cylinders for each specific connection
        var numCil = 1;
        foreach (var connection in connections)
        {
            GameObject cylinder = Instantiate(cylinderPrefab);
            cylinder.name = ("cilindro_" + numCil);
            cylinder.GetComponent<Renderer>().material.color = Color.blue;
            cylinders.Add(cylinder);
            numCil++;
        }

        // Armazenar os dados de cada quadro
        framesLandmarks = new Dictionary<string, List<Dictionary<string, Vector3>>>();

        foreach (var quadro in data.landmarks_quadros)
        {
            foreach (var frame in quadro)
            {
                List<Dictionary<string, Vector3>> landmarksList = new List<Dictionary<string, Vector3>>();

                foreach (var landmark in frame.Value)
                {
                    Dictionary<string, Vector3> landmarkData = new Dictionary<string, Vector3>();
                    foreach (var point in landmark)
                    {
                        Vector3 pos = new Vector3((point.Value.x) * 1.0f, 1.0f - (point.Value.y) * 1.0f, -(point.Value.z) * 0.23f);
                        landmarkData.Add(point.Key, pos);

                    }
                    landmarksList.Add(landmarkData);
                }
                framesLandmarks.Add(frame.Key, landmarksList);
            }
        }

        StartCoroutine(AnimateSpheres());
    }



    // Atualizar as posições das esferas em cada quadro
    IEnumerator AnimateSpheres()
    {
        while (true)
        {
            // Atualizar a posição de cada esfera
            for (int i = 0; i < spheres.Count; i++)
            {
                var currentLandmark = framesLandmarks["quadro_" + currentFrame][i];
                foreach (var point in currentLandmark)
                {
                    spheres[i].transform.position = new Vector3(point.Value.x, point.Value.y, point.Value.z);
                }
            }

            // Atualizar a posição e escala dos cilindros com base nas conexões específicas
            for (int i = 0; i < connections.Count; i++)
            {
                var (indexA, indexB) = connections[i]; // Get the two connected spheres
                Vector3 start = spheres[indexA].transform.position; // Start at the first sphere
                Vector3 end = spheres[indexB].transform.position; // End at the second sphere

                // Set the position of the cylinder in the middle of the two spheres
                cylinders[i].transform.position = (start + end) / 2;

                // Set the scale (length) of the cylinder based on the distance between the spheres
                float distance = Vector3.Distance(start, end);
                cylinders[i].transform.localScale = new Vector3(
                    Mathf.Min(spheres[indexA - 1].transform.localScale.x, spheres[indexB - 1].transform.localScale.x), // Diameter based on smaller sphere
                    distance / 2, // Length
                    Mathf.Min(spheres[indexA - 1].transform.localScale.z * 0.5f, spheres[indexB - 1].transform.localScale.z * 0.5f)
                ); // Diameter based on smaller sphere

                // Set the rotation of the cylinder to point from the first sphere to the second
                cylinders[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
            }

            // Aguardar o próximo quadro
            yield return new WaitForSeconds(0.1f); // Ajuste a velocidade da animação

            // Avançar para o próximo quadro
            currentFrame = (currentFrame + 1) % framesLandmarks.Count;
        }
    }

    /*
    // Método para aplicar rotações nos ossos
    void ApplyBoneRotations()
    {
        // Pegar as posições dos landmarks (Ombro_Esquerdo, Cotovelo_Esquerdo, Pulso_Esquerdo)
        var quadroAtual = framesLandmarks["quadro_" + currentFrame];

        //Debug.Log("Debug: " + quadroAtual[1]);

        Vector3 shoulderPos = quadroAtual[1]["Ombro_Esquerdo"];
        Vector3 elbowPos = quadroAtual[3]["Cotovelo_Esquerdo"];
        Vector3 wristPos = quadroAtual[5]["Pulso_Esquerdo"];

        // Calcular a direção do braço superior (ombro -> cotovelo)
        Vector3 upperArmDirection = (elbowPos - shoulderPos).normalized;
        Quaternion upperArmRotation = Quaternion.LookRotation(upperArmDirection);
        upperArmLeft.rotation = upperArmRotation;

        // Calcular a direção do antebraço (cotovelo -> pulso)
        Vector3 lowerArmDirection = (wristPos - elbowPos).normalized;
        Quaternion lowerArmRotation = Quaternion.LookRotation(lowerArmDirection);
        lowerArmLeft.rotation = lowerArmRotation;

        // Rotacionar o pulso baseado na direção do antebraço
        Quaternion wristRotation = Quaternion.LookRotation(lowerArmDirection);
        wristLeft.rotation = wristRotation;
    }*/
}
