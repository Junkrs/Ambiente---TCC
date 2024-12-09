# Ambiente - TCC ![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white)![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
Parte do Trabalho de Conclusão de Curso desenvolvido para minha graduação em Ciência da Computação na [UNESP](https://www.fc.unesp.br/#!/departamentos/computacao/cursos-de-graduao/bacharelado-em-ciencia-da-computacao/).
> [!IMPORTANT]
> Este repositório corresponde à renderização e animação dos esqueletos dos avatares, para o processamento dos _datasets_ e diferentes sinais de LIBRAS, utilize o código disponibilizado [neste repositório](https://github.com/Junkrs/Processamento-MediaPipe). :leftwards_arrow_with_hook:

## :green_circle: Introdução ao Projeto

Este trabalho tem como objetivo a criação de um sistema que capture e redirecione os movimentos da Língua Brasileira de Sinais (LIBRAS) para animar esqueletos de avatares 3D, usando ferramentas de inteligência artificial e visão computacional. Utilizando o [MediaPipe](https://ai.google.dev/edge/mediapipe/solutions/guide) para rastreamento de pontos chaves em vídeos que contenham sinais de LIBRAS, foram animados esqueletos de avatares 3D dentro do Unity.

A monografia do projeto completo está disponível no [repositório institucional da UNESP](https://hdl.handle.net/11449/258220).

## :green_circle: Como Usar

Ao usar abrir o projeto no Unity, você irá se deparar com essa tela:

![image](https://github.com/user-attachments/assets/f73cecdc-f79c-4d1c-9194-b87652ceb4fe)

Este é o ambiente desenvolvido no Unity. O _GameObject_ MediaPipeParser, encontrado na janela _Hierarchy_ é onde paramêtros desejados podem ser alterados.

### :green_circle: Trocar Sinal de LIBRAS para Animação

1. Clique no _GameObject_ MediaPipeParser; 
2. Escreva no campo _Sinal Desejado_ o nome do arquivo JSON que corresponda ao sinal escolhido, sem sua extensão de arquivo;
3. Salve o ambiente e clique no botão _Play_.

### :green_circle: Adicionar Mais Sinais no Programa

1. Pegue o arquivo JSON, criado pelo programa [deste repositório](https://github.com/Junkrs/Processamento-MediaPipe);
2. Adicione o arquivo na pasta indicada pelo código, indicado por padrão na linha 125 do código;
```
string jsonPath = Application.dataPath + "/Resources/JSON/" + sinalDesejado + ".mp4_landmarks.json";
```
3. Coloque quanto sinais quiser, pastas também podem ser inseridas, desde que o endereço para leitura dos arquivos seja alterado também na linha de código acima.

#### Qualquer dúvida, fique a vontade para entrar em contato comigo :D
