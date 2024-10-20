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
    private List<GameObject> spheres; // Esferas que representarao os pontos
    private List<GameObject> cylinders; // Cilindros que conectarao as esferas
    private int currentFrame = 0; // Quadro atual da animação
    public float avatarScaleFactor = 1.0f; // Escala de distancia dos pontos do avatar
    public string sinalDesejado;

    private Dictionary<int, string> pointNames = new Dictionary<int, string>()
    {
        { 0, "Nariz" },
        { 1, "Ombro_Esquerdo" },
        { 2, "Ombro_Direito" },
        { 3, "Cotovelo_Esquerdo" },
        { 4, "Cotovelo_Direito" },
        { 5, "Pulso_Esquerdo" },
        { 6, "Pulso_Direito" },
        { 7, "Dedo_Mindinho_Esquerdo" },
        { 8, "Dedo_Mindinho_Direito" },
        { 9, "Dedo_Indicador_Esquerdo" },
        { 10, "Dedo_Indicador_Direito" },
        { 11, "Dedao_Esquerda" },
        { 12, "Dedao_Direita" },
        { 13, "Quadril_Esquerdo" },
        { 14, "Quadril_Direito" }
    };

    // Define sphere scale factors for different body parts
    private Dictionary<string, Vector3> sphereScaleMap = new Dictionary<string, Vector3>()
    {
        { "Nariz", new Vector3(0.5f, 0.5f, 0.5f) },
        { "Ombro_Esquerdo", new Vector3(0.3f, 0.3f, 0.3f) },
        { "Ombro_Direito", new Vector3(0.3f, 0.3f, 0.3f) },
        { "Cotovelo_Esquerdo", new Vector3(0.2f, 0.2f, 0.2f) },
        { "Cotovelo_Direito", new Vector3(0.2f, 0.2f, 0.2f) },
        { "Pulso_Esquerdo", new Vector3(0.15f, 0.15f, 0.15f) },
        { "Pulso_Direito", new Vector3(0.15f, 0.15f, 0.15f) },
        { "Dedo_Mindinho_Esquerdo", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Dedo_Mindinho_Direito", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Dedo_Indicador_Esquerdo", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Dedo_Indicador_Direito", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Dedao_Esquerda", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Dedao_Direita", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Quadril_Esquerdo", new Vector3(0.25f, 0.25f, 0.25f) },
        { "Quadril_Direito", new Vector3(0.25f, 0.25f, 0.25f) }
    };

    // List of specific connections to make between spheres
    private List<(int, int)> connections = new List<(int, int)>
    {
        (1, 2),  // Ombro_Esquerdo -> Ombro_Direito
        (1, 13), // Ombro_Esquerdo -> Quadril_Esquerdo
        (1, 3),  // Ombro_Esquerdo -> Cotovelo_Esquerdo
        (2, 4),  // Ombro_Direito -> Cotovelo_Direito
        (2, 14), // Ombro_Direito -> Quadril_Direito
        (3, 5),  // Cotovelo_Esquerdo -> Pulso_Esquerdo
        (4, 6),  // Cotovelo_Direito -> Pulso_Direito
        (5, 7),  // Pulso_Esquerdo -> Dedo_Mindinho_Esquerdo
        (5, 9),  // Pulso_Esquerdo -> Dedo_Indicador_Esquerdo
        (5, 11), // Pulso_Esquerdo -> Dedao_Esquerda
        (7, 9),  // Dedo_Mindinho_Esquerdo -> Dedo_Indicador_Esquerdo
        (6, 8),  // Pulso_Direito -> Dedo_Mindinho_Direito
        (6, 10), // Pulso_Direito -> Dedo_Indicador_Direito
        (6, 12), // Pulso_Direito -> Dedao_Direita
        (8, 10), // Dedo_Mindinho_Direito -> Dedo_Indicador_Direito
        (13, 14) // Quadril_Esquerdo -> Quadril_Direito
    };

    // Define colors for each body part
    private Dictionary<string, Color> sphereColorMap = new Dictionary<string, Color>()
    {
        { "Nariz", Color.red },
        { "Ombro_Esquerdo", Color.blue },
        { "Ombro_Direito", Color.blue },
        { "Cotovelo_Esquerdo", Color.green },
        { "Cotovelo_Direito", Color.green },
        { "Pulso_Esquerdo", Color.yellow },
        { "Pulso_Direito", Color.yellow },
        { "Dedo_Mindinho_Esquerdo", Color.magenta },
        { "Dedo_Mindinho_Direito", Color.magenta },
        { "Dedo_Indicador_Esquerdo", Color.cyan },
        { "Dedo_Indicador_Direito", Color.cyan },
        { "Dedao_Esquerda", Color.red },
        { "Dedao_Direita", Color.red },
        { "Quadril_Esquerdo", Color.blue },
        { "Quadril_Direito", Color.blue }
    };

    void Start() {
        // Carregar o JSON
        string jsonPath = Application.dataPath + "/Resources/JSON/" + sinalDesejado + ".mp4_landmarks.json";

        string jsonString = File.ReadAllText(jsonPath);
        Debug.Log("Loaded JSON: " + sinalDesejado);
        var data = JsonConvert.DeserializeObject<RootObject>(jsonString);
        Debug.Log("Nome do vídeo: " + data.nome_video);
        Debug.Log("Quantidade de quadros: " + data.landmarks_quadros.Count);

        // Create a parent GameObject to hold all the spheres
        GameObject parentObject = new GameObject("EsqueletoAvatar");

        // Create a parent GameObject to hold all the cylinders
        GameObject cylinderParentObject = new GameObject("Conectores");

        // Preparar as esferas para os landmarks e os cilindros para conexão
        spheres = new List<GameObject>();
        cylinders = new List<GameObject>();

        // Variables to store the positions of the shoulders and nose
        Vector3 ombroEsquerdoPosition = Vector3.zero;
        Vector3 ombroDireitoPosition = Vector3.zero;
        Vector3 nosePosition = Vector3.zero;

        // Define parent-child relationships for the hierarchy (starting from the shoulders)
        Dictionary<string, GameObject> parentMap = new Dictionary<string, GameObject>();

        // Create the spheres
        int index = 0;
        foreach (var landmark in data.landmarks_quadros[0]["quadro_0"])
        {
            GameObject sphere = Instantiate(spherePrefab);

            if (pointNames.ContainsKey(index))
            {
                string sphereName = pointNames[index];
                sphere.name = sphereName;
                spheres.Add(sphere);

                // Apply scale and color based on body part
                if (sphereScaleMap.ContainsKey(sphereName))
                {
                    sphere.transform.localScale = sphereScaleMap[sphereName];
                    sphere.GetComponent<Renderer>().material.color = sphereColorMap[sphereName];
                }

                // Get shoulder and nose positions
                if (sphereName == "Ombro_Esquerdo")
                {
                    ombroEsquerdoPosition = sphere.transform.position;
                }
                else if (sphereName == "Ombro_Direito")
                {
                    ombroDireitoPosition = sphere.transform.position;
                }
                else if (sphereName == "Nariz")
                {
                    nosePosition = sphere.transform.position;
                }

                // Create a dummy GameObject for hierarchy without affecting the scale
                GameObject dummyParent = new GameObject(sphereName + "_Holder");
                dummyParent.transform.SetParent(parentObject.transform); // Attach dummy to the overall parent

                // Add the sphere to the dummy object (which doesn't have any scale transformations)
                sphere.transform.SetParent(dummyParent.transform, false);

                // Establish hierarchy logic
                if (sphereName.Contains("Ombro")) // Ombro_Esquerdo or Ombro_Direito as top-level parents
                {
                    parentMap[sphereName] = dummyParent; // Store dummy as the top-level parent
                }
                else if (sphereName.Contains("Cotovelo") || sphereName.Contains("Quadril"))
                {
                    // Assign Cotovelo or Quadril to respective shoulder or hips dummy
                    if (sphereName.Contains("Esquerdo"))
                        dummyParent.transform.SetParent(parentMap["Ombro_Esquerdo"].transform, false); // Use dummy for hierarchy
                    else if (sphereName.Contains("Direito"))
                        dummyParent.transform.SetParent(parentMap["Ombro_Direito"].transform, false); // Use dummy for hierarchy

                    parentMap[sphereName] = dummyParent; // Store dummy in parent map for further children
                }
                else if (sphereName.Contains("Pulso"))
                {
                    // Assign Pulso and Dedos to respective elbows dummy
                    if (sphereName.Contains("Esquerdo"))
                        dummyParent.transform.SetParent(parentMap["Cotovelo_Esquerdo"].transform, false); // Use dummy for hierarchy
                    else if (sphereName.Contains("Direito"))
                        dummyParent.transform.SetParent(parentMap["Cotovelo_Direito"].transform, false); // Use dummy for hierarchy

                    parentMap[sphereName] = dummyParent; // Store dummy in parent map for further children
                }
                else if (sphereName.Contains("Dedao") || sphereName.Contains("Dedo"))
                {
                    // Assign Dedos to respective pulses dummy
                    if (sphereName.Contains("Esquerdo"))
                        dummyParent.transform.SetParent(parentMap["Pulso_Esquerdo"].transform, false); // Use dummy for hierarchy
                    else if (sphereName.Contains("Direito"))
                        dummyParent.transform.SetParent(parentMap["Pulso_Direito"].transform, false); // Use dummy for hierarchy
                }
            }

            index++;
        }

        // Instantiate cylinders for each specific connection
        foreach (var connection in connections)
        {
            GameObject cylinder = Instantiate(cylinderPrefab);

            // Get the indices for the two spheres being connected
            int sphereIndex1 = connection.Item1;
            int sphereIndex2 = connection.Item2;

            // Get the names of the spheres being connected
            string sphereName1 = spheres[sphereIndex1].name;
            string sphereName2 = spheres[sphereIndex2].name;

            // Name the cylinder according to the spheres it connects
            cylinder.name = $"Conector: {sphereName1} -> {sphereName2}";

            // Set the cylinder color
            cylinder.GetComponent<Renderer>().material.color = Color.gray;

            // Add the cylinder to the parent GameObject
            cylinder.transform.SetParent(cylinderParentObject.transform);

            // Add the cylinder to the list
            cylinders.Add(cylinder);
        }

        // Armazenar os dados de cada quadro
        framesLandmarks = new Dictionary<string, List<Dictionary<string, Vector3>>>();

        // Variables to track the min and max values for calculating the center
        Vector3 minValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);

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
                        Vector3 pos = new Vector3((point.Value.x) * 1.0f * avatarScaleFactor, 1.0f - (point.Value.y) * 1.0f * avatarScaleFactor, -(point.Value.z) * 0.23f * avatarScaleFactor);
                        landmarkData.Add(point.Key, pos);

                        // Track the minimum and maximum values for x, y, and z
                        minValues = Vector3.Min(minValues, pos);
                        maxValues = Vector3.Max(maxValues, pos);
                    }
                    landmarksList.Add(landmarkData);
                }
                framesLandmarks.Add(frame.Key, landmarksList);
            }
        }
        // Calculate the center of the body
        Vector3 bodyCenter = (minValues + maxValues) / 2;

        // Apply the offset to center all the landmarks
        foreach (var quadro in framesLandmarks.Keys.ToList())
        {
            for (int i = 0; i < framesLandmarks[quadro].Count; i++)
            {
                var landmarksList = framesLandmarks[quadro][i];
                foreach (var point in landmarksList.Keys.ToList())
                {
                    // Offset each position by the calculated center
                    landmarksList[point] -= bodyCenter;
                }
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
                    Mathf.Min(spheres[indexA].transform.localScale.x * 0.5f, spheres[indexB].transform.localScale.x * 0.5f), // Diameter based on smaller sphere
                    distance / 2, // Length
                    Mathf.Min(spheres[indexA].transform.localScale.z * 0.5f, spheres[indexB].transform.localScale.z * 0.5f)
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
}
