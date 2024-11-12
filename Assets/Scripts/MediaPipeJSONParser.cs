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

// Estrutura com as coordenadas das esferas
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

    public GameObject spherePrefab; // Prefab da esferas que representam os landmarks
    public GameObject cylinderPrefab; // Prefab dos cilindros que representam as conexoes entre cada esfera
    private Dictionary<string, List<Dictionary<string, Vector3>>> framesLandmarks; // Dicionario para armazenar os landmarks de cada quadro
    private List<GameObject> spheres; // Lista de esferas que representarao os landmarks
    private List<GameObject> cylinders; // Lista de cilindros que conectarao os landmarks
    private int currentFrame = 0; // Quadro atual da animacao
    public float avatarScaleFactor; // Escala de distancia dos pontos do avatar desejado
    public string sinalDesejado; // Nome do video e articulador desejado

    // Dicionario para armazenar o nome de cada landmark, representados pelas esferas
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
        { 11, "Dedao_Esquerdo" },
        { 12, "Dedao_Direito" },
        { 13, "Quadril_Esquerdo" },
        { 14, "Quadril_Direito" }
    };

    // Dicionario para armazenar os tamanhos de cada esfera que representa os landmarks
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
        { "Dedao_Esquerdo", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Dedao_Direito", new Vector3(0.1f, 0.1f, 0.1f) },
        { "Quadril_Esquerdo", new Vector3(0.25f, 0.25f, 0.25f) },
        { "Quadril_Direito", new Vector3(0.25f, 0.25f, 0.25f) }
    };

    // Lista das conexoes entre as esferas que representam os landmarks
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

    // Dicionario para armazenar as cores de cada esfera que representam os landmarks
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
        { "Dedao_Esquerdo", Color.red },
        { "Dedao_Direito", Color.red },
        { "Quadril_Esquerdo", Color.blue },
        { "Quadril_Direito", Color.blue }
    };

    // Funcao Start
    void Start() {

        // Carregar o JSON, utilizando a variavel com o nome e articulador do sinal
        string jsonPath = Application.dataPath + "/Resources/JSON/" + sinalDesejado + ".mp4_landmarks.json";
        string jsonString = File.ReadAllText(jsonPath); 
        Debug.Log("Loaded JSON: " + sinalDesejado);

        // Deserializacao do arquivo JSON
        var data = JsonConvert.DeserializeObject<RootObject>(jsonString);

        // Impressao do nome do video, juntamente da quantidade de quadros do mesmo
        Debug.Log("Nome do vídeo: " + data.nome_video);
        Debug.Log("Quantidade de quadros: " + data.landmarks_quadros.Count);

        // Criacao de um GameObject para manter as esferas que representam os landmarks, e criacao do mesmo para os cilindros que representam os conectores
        GameObject parentObject = new GameObject("EsqueletoAvatar");
        GameObject cylinderParentObject = new GameObject("Conectores");

        // Preparar as esferas para os landmarks e os cilindros para os conectores
        spheres = new List<GameObject>();
        cylinders = new List<GameObject>();

        // Variaveis declaradas para manter salvas as posicoes dos ombros e do nariz (FUNCAO NAO IMPLEMENTADA - CRIACAO DE PESCOCO E CABECA)
        Vector3 ombroEsquerdoPosition = Vector3.zero;
        Vector3 ombroDireitoPosition = Vector3.zero;
        Vector3 nosePosition = Vector3.zero;

        // Dicionario para definir as relacoes de hierarquia entre as esferas, iniciando nos ombros
        Dictionary<string, GameObject> parentMap = new Dictionary<string, GameObject>();

        // Criacao das esferas
        int index = 0; // Index auxiliar para indexacao das esferas 
        foreach (var landmark in data.landmarks_quadros[0]["quadro_0"]) // Loop para percorrer todos os landmarks dentre de um quadro
        {
            // Instanciar as esferas utilizando o prefab escolhido
            GameObject sphere = Instantiate(spherePrefab);

            // Localizacao das esferas baseadas no codigo do dicionario
            if (pointNames.ContainsKey(index))
            {
                // Aplicacao de nome e insercao das esferas no ambiente virtual
                string sphereName = pointNames[index];
                sphere.name = sphereName;
                spheres.Add(sphere);

                // Aplicar escala e cor para as esferas, baseados nos seus devidos dicionarios
                if (sphereScaleMap.ContainsKey(sphereName))
                {
                    sphere.transform.localScale = sphereScaleMap[sphereName];
                    sphere.GetComponent<Renderer>().material.color = sphereColorMap[sphereName];
                }

                // Buscar posicao dos ombros e tambem do nariz (FUNCAO NAO IMPLEMENTADA - CRIACAO DE PESCOCO E CABECA)
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

                // Criacao de um GameObject vazio para criacao da hierarquia sem afetar a escala
                GameObject dummyParent = new GameObject(sphereName + "_Holder"); // Nome do GameObject vazio
                dummyParent.transform.SetParent(parentObject.transform); // Inserir o GameObject dentro da hierarquia

                // Codigo para que quando adicionar a esfera para dentro do GameObject vazio nao altere a escala
                sphere.transform.SetParent(dummyParent.transform, false);

                // Estabelecimento da logica de hierarquia
                if (sphereName.Contains("Ombro")) // Ombros sao o primeiro nivel
                {
                    parentMap[sphereName] = dummyParent; // Armazenar as esferas dentro do GameObject pai vazio, de mesmo nome
                }
                else if (sphereName.Contains("Cotovelo") || sphereName.Contains("Quadril"))
                {
                    // Colocar cotovelos e quadris como filhos de ombro
                    if (sphereName.Contains("Esquerdo"))
                        dummyParent.transform.SetParent(parentMap["Ombro_Esquerdo"].transform, false); // Filhos do ombro esquerdo
                    else if (sphereName.Contains("Direito"))
                        dummyParent.transform.SetParent(parentMap["Ombro_Direito"].transform, false); // Filhos do ombro direito

                    parentMap[sphereName] = dummyParent; // Armazenar estes como pontos pais, para que possam receber mais esferas na hierarquia
                }
                else if (sphereName.Contains("Pulso"))
                {
                    //  Colocar pulsos como filhos do cotovelo
                    if (sphereName.Contains("Esquerdo"))
                        dummyParent.transform.SetParent(parentMap["Cotovelo_Esquerdo"].transform, false); // Filhos do cotovelo esquerdo
                    else if (sphereName.Contains("Direito"))
                        dummyParent.transform.SetParent(parentMap["Cotovelo_Direito"].transform, false); // Filhos do cotovelo direito

                    parentMap[sphereName] = dummyParent; // Armazenar estes como pontos pais, para que possam receber mais esferas na hierarquian
                }
                else if (sphereName.Contains("Dedao") || sphereName.Contains("Dedo"))
                {
                    // Colocar dedos como fihos do pulso
                    if (sphereName.Contains("Esquerdo"))
                        dummyParent.transform.SetParent(parentMap["Pulso_Esquerdo"].transform, false); // Filhos do pulso esquerdo
                    else if (sphereName.Contains("Direito"))
                        dummyParent.transform.SetParent(parentMap["Pulso_Direito"].transform, false); // Filhos do pulso direito
                }
            }

            index++;
        }

        // Instanciacao dos cilindros para cada conexao especifica, baseado no dicionario de conexoes
        foreach (var connection in connections)
        {
            GameObject cylinder = Instantiate(cylinderPrefab);

            // Pegar os indices em que duas esferas estao conectadas
            int sphereIndex1 = connection.Item1;
            int sphereIndex2 = connection.Item2;

            // Pegar os nomes das duas esferas que estao conectadas
            string sphereName1 = spheres[sphereIndex1].name;
            string sphereName2 = spheres[sphereIndex2].name;

            // Nomear os cilindros de acordo com as esferas que ele conecta
            cylinder.name = $"Conector: {sphereName1} -> {sphereName2}";

            // Selecionar a cor dos cilindros
            cylinder.GetComponent<Renderer>().material.color = Color.gray;

            // Adicionar o cilindro ao GameObject pai
            cylinder.transform.SetParent(cylinderParentObject.transform);

            // Adicionar os cilindros conectores no ambiente virtual
            cylinders.Add(cylinder);
        }

        // Armazenar os dados de cada quadro
        framesLandmarks = new Dictionary<string, List<Dictionary<string, Vector3>>>();

        // Variaveis utilizadas para salvar os valores maximos e minimos para calcular o centro do "esqueleto" criado
        Vector3 minValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        // Inicio do loop de leitura da posicao dos landmarks no primeiro quadro do arquivo JSON
        foreach (var quadro in data.landmarks_quadros)
        {
            foreach (var frame in quadro)
            {
                // Dicionario de cada landmark em um certo quadro do video
                List<Dictionary<string, Vector3>> landmarksList = new List<Dictionary<string, Vector3>>();

                foreach (var landmark in frame.Value)
                {
                    // Dicionario dos valores salvos das coordenadas de cada um dos landmarks
                    Dictionary<string, Vector3> landmarkData = new Dictionary<string, Vector3>();
                    
                    foreach (var point in landmark)
                    {
                        /*  TRANSFORMACAO DAS COORDENADAS DO MEDIAPIPE PARA O UNITY3D
                                
                                Para a transformacao dos dados do MediaPipe, foi necessario fazer algumas mudancas devido ao diferente sistemas de coordenadas
                                - Inverter o eixo Y
                                - Multiplicar o eixo Z para reposicionar e melhorar a profundidade
                                - Multiplicar todos os valores por uma escala de distância

                        */
                        Vector3 pos = new Vector3((point.Value.x) * 1.0f * avatarScaleFactor, 1.0f - (point.Value.y) * 1.0f * avatarScaleFactor, -(point.Value.z) * 0.23f * avatarScaleFactor);
                        landmarkData.Add(point.Key, pos);

                        // Salvar os valores maximos e minimos de x, y e z
                        minValues = Vector3.Min(minValues, pos);
                        maxValues = Vector3.Max(maxValues, pos);
                    }
                    // Adicionar os landmarks na lista
                    landmarksList.Add(landmarkData);
                }
                //  Armazenar os dados de cada quadro na lista de landmarks
                framesLandmarks.Add(frame.Key, landmarksList);
            }
        }
        // Calcular o centro do "esqueleto" do avatar
        Vector3 bodyCenter = (minValues + maxValues) / 2;

        // Aplicar os valores para centralizar todos os landmarks com o plano de coordenadas do unity
        foreach (var quadro in framesLandmarks.Keys.ToList())
        {
            for (int i = 0; i < framesLandmarks[quadro].Count; i++)
            {
                var landmarksList = framesLandmarks[quadro][i];
                foreach (var point in landmarksList.Keys.ToList())
                {
                    // Recalcular cada posicao com base no centro do "esqueleto"
                    landmarksList[point] -= bodyCenter;
                }
            }
        }
        // Iniciar a corotina de animacao das esferas
        StartCoroutine(AnimateSpheres());
    }

    // INICIO DO CODIGO PARA ANIMACAO DAS ESFERAS DENTRO DO AMBIENTE VIRTUAL

    // Atualizar as posições das esferas em cada quadro
    IEnumerator AnimateSpheres()
    {
        while (true)
        {
            // Atualizar a posição de cada esfera durante cada frame
            for (int i = 0; i < spheres.Count; i++)
            {
                // Pegar a posicao correta de cada esfera dentro de um quadro
                var currentLandmark = framesLandmarks["quadro_" + currentFrame][i];
                foreach (var point in currentLandmark)
                {
                    // Inserir a esfera na sua posicao correta salva
                    spheres[i].transform.position = new Vector3(point.Value.x, point.Value.y, point.Value.z);
                }
            }

            // Atualizar a posição e escala dos cilindros com base nas conexões específicas
            for (int i = 0; i < connections.Count; i++)
            {
                var (indexA, indexB) = connections[i]; // Buscar na lista as duas esferas conectadas
                Vector3 start = spheres[indexA].transform.position; // Iniciar um vetor na primeira esfera
                Vector3 end = spheres[indexB].transform.position; // Iniciar um vetor na seguna esfera

                // Colocar um cilindro no meio das duas esferas selecionadas por esta conexao
                cylinders[i].transform.position = (start + end) / 2;

                // Selecionar o comprimento do cilindro com base na distancia entre as duas esferas
                float distance = Vector3.Distance(start, end);
                cylinders[i].transform.localScale = new Vector3(
                    Mathf.Min(spheres[indexA].transform.localScale.x * 0.5f, spheres[indexB].transform.localScale.x * 0.5f), 
                    distance / 2, //Comprimento
                    Mathf.Min(spheres[indexA].transform.localScale.z * 0.5f, spheres[indexB].transform.localScale.z * 0.5f)
                ); // Diametro baseado em metade do tamanho da esfera menor

                // Selecionar a rotacao do cilindro de forma que a primeira esfera esteja conectada com a segunda
                cylinders[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
            }

            // Aguardar o próximo quadro
            yield return new WaitForSeconds(0.05f); // Ajuste a velocidade da animação

            // Avançar para o próximo quadro
            currentFrame = (currentFrame + 1) % framesLandmarks.Count;
        }
    }
}
