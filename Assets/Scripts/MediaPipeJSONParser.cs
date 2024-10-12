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
    private Dictionary<string, List<Dictionary<string, Vector3>>> framesLandmarks; // Para armazenar os landmarks de cada quadro
    private List<GameObject> spheres; // Esferas que representarão os pontos
    private int currentFrame = 0; // Quadro atual da animação
    //public float avatarScaleFactor = 1.0;

    private Animator animator;
    private Transform upperArmLeft, lowerArmLeft, wristLeft;

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


    void Start() {
        // Carregar o JSON
        string jsonPath = Application.dataPath + "/Resources/JSON/abacaxi_articulador1.mp4_landmarks.json"; // Coloque o arquivo JSON na pasta "Assets"
        string jsonString = File.ReadAllText(jsonPath);

        // Decodificar o JSON em uma estrutura apropriada
        var data = JsonConvert.DeserializeObject<RootObject>(jsonString);
        Debug.Log("Nome do vídeo: " + data.nome_video);
        Debug.Log("Quantidade de quadros: " + data.landmarks_quadros.Count);

        // Preparar as esferas para os landmarks
        spheres = new List<GameObject>();

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
            }
            else
            {
                continue;
            }
            index++;
        }

        // Armazenar os dados de cada quadro
        framesLandmarks = new Dictionary<string, List<Dictionary<string, Vector3>>>();

        foreach (var quadro in data.landmarks_quadros)
        {
            Debug.Log("Quantidade de pontos do " + quadro.First().Key + ": " + quadro.Last().Value.Count);
            foreach (var frame in quadro)
            {
                List<Dictionary<string, Vector3>> landmarksList = new List<Dictionary<string, Vector3>>();

                foreach (var landmark in frame.Value)
                {
                    // Criar um dicionário com os valores de x, y, z
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
        // Access the Animator and initialize key bones (shoulder, elbow, wrist)
        animator = GetComponent<Animator>();
        upperArmLeft = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        lowerArmLeft = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        wristLeft = animator.GetBoneTransform(HumanBodyBones.LeftHand);

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

            // Aguardar o próximo quadro
            yield return new WaitForSeconds(0.1f); // Ajuste a velocidade da animação

            // Aplicar rotações nos ossos com base nos landmarks
            ApplyBoneRotations();

            // Avançar para o próximo quadro
            currentFrame = (currentFrame + 1) % framesLandmarks.Count;
        }
    }

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
    }

}
