using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ponto
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Landmark
{
    public Ponto ponto; //Coordenadas
}

[System.Serializable]
public class Frame
{
    public Dictionary<string, Landmark[]> quadro; //Map of frame name to landmarks
}

[System.Serializable]
public class VideoObject
{
    public string nome_video;
    public List<Frame> landmarks_quadros; //List of frames

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
        var jsonFile = Resources.Load<TextAsset>("JSON/ProcessedJSON/Abacaxi_Articulador1.mp4_landmarks");
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
        for (int i = 0; i < videoObject.landmarks_quadros.Count; i++)
        {
            var frame = videoObject.landmarks_quadros[i];
            foreach (var quadro in frame.quadro)
            {
                Debug.Log($"Quadro: {quadro.Key}");
                foreach (var pontos in quadro.Value)
                {
                    Debug.Log($"Ponto - x: {pontos.ponto.x}, y: {pontos.ponto.y}, z: {pontos.ponto.z}");
                }
            }
        }
    }
}
