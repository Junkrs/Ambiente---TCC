using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Ponto // Coordenadas de cada um dos landmarks
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Landmark
{
    //public string point_name; //Nome de cada landmark
    //public float x;
    //public float y;
    //public float z;
    public Dictionary<string, float> coordenadas;
    //public int num; //Num. de cada landmark baseado na documentacao
    //public Ponto ponto; //Coordenadas
}

[System.Serializable]
public class Frame
{
    //public string frame_count; //Num. de cada quadro
    //public Dictionary<string, Landmark>[] pose_landmarks; //Vetor com todos os landmarks do quadro
    public Landmark[] pose_landmarks; //Vetor com todos os landmarks do quadro
}

[System.Serializable]
public class VideoObject
{
    public string nome_video;
    public Frame[] landmarks_quadros; //Vetor com todos os quadros

    public static VideoObject CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<VideoObject>(jsonString);
    }
}

public class MediaPipeJSONParser : MonoBehaviour
{
    //Diretório raiz dos arquivos JSON
    public string jsonFilePath = "JSON/ProcessedJSON/";
    void Start()
    {

        //Recuperar arquivo(s) JSON dos Resources
        //Aqui tem um erro de leitura, se nao colocar o path completo ele nao encontra o .json

        //var jsonFile = Resources.Load<TextAsset>(jsonFilePath + "Abacaxi_Articulador1.mp4_landmarks");
        var jsonFile = Resources.Load<TextAsset>("JSON/abacaxi_articulador1.mp4_landmarks");
        if (jsonFile == null)
        {
            Debug.LogError("JSON file not found or failed to load.");
            return;
        }
        //Debug.Log("Funciona a leitura");
        VideoObject videoObject = VideoObject.CreateFromJSON(jsonFile.text);
        if (videoObject == null)
        {
            Debug.LogError("Failed to parse JSON.");
            return;
        }

        //Testando a recuperação do JSON
        Debug.Log($"Nome do Objeto: {videoObject.nome_video}");
        Debug.Log($"Quantidade de quadros: {videoObject.landmarks_quadros.Length}");
        Debug.Log($"Quantidade de pontos: {videoObject.landmarks_quadros[0].pose_landmarks.Length}");
        Debug.Log($"Valor: {videoObject.landmarks_quadros[0].pose_landmarks[0].coordenadas.Values}");
        //Debug.Log(jsonFile.text);
        /*foreach (var frame in videoObject.landmarks_quadros)
        {
            Debug.Log($"Quadro: {frame.frame_count}");
            foreach (var landmark in frame.pose_landmarks)
            {
                Debug.Log($"Landmark {landmark.num} - {landmark.point_name}: x: {landmark.ponto.x}, y: {landmark.ponto.y}, z: {landmark.ponto.z}");
            }
        }*/
    }
}