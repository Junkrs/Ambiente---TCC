using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Landmark
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Frame
{
    public Dictionary<string, Landmark> landmarks; //Cada "Key" do dicionário é um ponto do quadro
}

[System.Serializable]
public class VideoObject
{
    public string videoName;
    public Dictionary<string, Frame> frames; //Cada "Key" é um quadro do vídeo

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
        var jsonString = Resources.Load<TextAsset>("JSON/ProcessedJSON/Abacaxi_Articulador1.mp4_landmarks");
        if (jsonString == null)
        {
            Debug.LogError("JSON file not found or failed to load.");
            return;
        }
        //Debug.Log("Funciona a leitura");
        VideoObject videoObject = VideoObject.CreateFromJSON(jsonString.text);
        if (videoObject == null)
        {
            Debug.LogError("Failed to parse JSON.");
            return;
        }

        //Testando a recuperação do JSON
        Debug.Log($"Nome do Objeto: {videoObject.videoName}");
        foreach (var frame in videoObject.frames)
        {
            Debug.Log($"Quadro: {frame.Key}");
            foreach (var landmark in frame.Value.landmarks)
            {
                Debug.Log($"Ponto: {landmark.Key} - x: {landmark.Value.x}, y: {landmark.Value.y}, z: {landmark.Value.z}");
            }
        }
    }
}
