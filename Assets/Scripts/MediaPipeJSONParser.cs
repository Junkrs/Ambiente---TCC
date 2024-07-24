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
        var jsonFile = Resources.Load<TextAsset>(jsonFilePath + "Abacaxi_Articulador1.mp4_landmarks");
        VideoObject videoObject = VideoObject.CreateFromJSON(jsonFile.ToString());

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
