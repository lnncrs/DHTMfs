# Sistema de arquivos distribuído (baseado em Hash Tables) para aplicações multimidia

**Distributed (Hash Table) file system  for multimedia applications.**

![UFABC Logo](assets/logotipo-ufabc-extenso.png)

Universidade Federal do ABC - Bacharelado em Ciência e Tecnologia Algoritmos e Estruturas de Dados 2023/Q3

Lenin Cristi, Daniel Byoung, Gustavo Prado, Carolina Riccomi, Vitor Carmignoli

{lenin.cristi,daniel.jung,p.gustavo,riccomi.carolina,carmignoli.v}@aluno.ufabc.edu.br


## Resumo do artigo

**Resumo.** O presente artigo trata da viabilidade, aspectos de arquitetura e de implementação do uso de tabelas de hash distribuídas na disponibilização de conteúdo multimídia.

**Abstract.** This article addresses the viability, architectural and implementation aspects of distributed hash tables in the provision of multimedia content.

## Introdução

### O desafio da distribuição multimídia em cenários cada vez mais complexos

Existe um desafio considerável na distribuição de conteúdo multimídia, desde entrega eficiente em plataformas variadas com consumo diversificado a redes de diferentes latências e de topologia variável.
A proliferação de dispositivos e variedade de consumo, que vai desde smartphones e tablets a TVs, a dispositivos domésticos conectados e dispositivos de realidade virtual.

### Redes de topologia variável

A qualidade da conexão de rede varia significativamente em diferentes regiões, diferentes ambientes e mesmo diferentes períodos do dia para locais com grande demanda. Mesmo a rota que o dispositivo usa para recuperar conteúdo pode ter trechos e especificações alteradas durante um fluxo ou transferência de bloco de dados e é necessário otimizar e racionalizar a distribuição garantindo uma experiência de usuário satisfatória, mesmo em condições de rede desafiadoras.

A qualidade de conteúdo distribuído que aumenta sensivelmente a cada ano, na última década, o avanço nos padrões de rede, tanto cabeadas quanto sem fio, permite um padrão de uso e consumo de dados considerado impossível antes disso, considere dois cenários: Uma transmissão em tempo real em resolução 4k usando rede celular; Duas estações de trabalho conectadas a uma rede local sem fio que colaboram num mesmo projeto de edição de vídeo com dezenas de horas gravadas em 8k que precisam renderiza-lo localmente na GPU e transferir os artefatos para um NAS na mesma rede. Eram ambas tarefas inatingíveis, mas tornadas possíveis hoje por tecnologias como a 5G e a WiFi 6 respectivamente.

### Tamanhos de arquivo e fluxos

A evolução dos padrões de rede e técnicas de distribuição portanto, que no limite é o que tratamos aqui, foi acompanhada por uma rápida evolução nos padrões de uso e especificação de multimídia sendo consumidas, tornando semi obsoletas tecnologias de distribuição, compressão e transmissão inovadoras em não mais que um ano.

## Arquitetura proposta

Foram analisadas três abordagens [1][2][3] de implementação do BitTorrent e uma de DHT modificada [4], e estudadas diferentes abordagens de implementação do protocolo BitTorrent. As abordagens de implementação do protocolo BitTorrent são robustas mas com abstração alta do assunto central de DHTs, a abordagem de implementação direta de DHTs tem vantagens claras no que tange a completude técnica da solução mas também abstrai a noção mais clara de DHTs, e uma peça chave da solução é a pedagógica, uma solução abstraída ou mais formal apesar de desejável em ambientes de operação mas esconderia na apresentação da solução a estrutura de dados central no artigo, qual seja a DHT.

Optamos por uma abordagem de implementar DHTs baseadas diretamente na sua definição [7] onde é possível acompanhar as operações e deliberadamente não automatizar as alterações de nós e arquivos na rede, o que facilita sua demonstração.

### Características iniciais da solução

A solução foi pensada construída em torno de DHTs que são visíveis por métodos de consulta para facilitar o entendimento de seu funcionamento, e é baseada em nós colaborativos de papel e peso idêntico na rede (ou seja, sem arquitetura master/slave) que por sua vez tem mudança fácil de topologia.

<div style="text-align:center">

![Esboço](/assets/esboco.png "Esboço")<br>
O primeiro esboço da solução foi incluído aqui para ilustrar o processo criativo da elaboração, e dá um mapa consistente das funcionalidades:

</div>

1. O processo de envio e consulta de arquivos na rede

2. O processo de sincronização colaborativa de nós e arquivos na rede

3. As 4 funcionalidades principais da rede

    3.1 F1 Troca de listas de arquivos (sincronização da DHT)

    3.2 F2 Envio de arquivo para a rede

    3.3 F3 Recuperação de arquivo na rede

    3.4 F4 Pesquisa na rede por um arquivo específico

4. As DHTs centrais da solução: nós e arquivos

### Aspectos funcionais

A solução foi desenhada para permitir sincronização de dados de nós e arquivos, ser densa ou seja, permitir conexão direta entre quaisquer dois nós, ser resistente a cenários de conexão eventual entre grupos de nós (alteração de topologia), ser compatível com cenários de trechos (ou toda) na internet e usar portas padrão SSL para não ser bloqueada por firewalls de pacote.

<div style="text-align:center">

![Rede](/assets/rede12.png "Rede")<br>
Diagrama de rede densa

</div>

Cada nó ao disparar gera sua identidade, carrega sua lista de nós conhecidos e gera sua DHT de arquivos a partir de seu diretório local.

<div style="text-align:center">

![Nó](/assets/no2.png "Nó")<br>
Diagrama de blocos de cada nó

</div>

A pesquisa de arquivos ocorre por difusão na rede, mas usa um cache local sincronizável da DHT de arquivos como recurso de pesquisa rápida.

<div style="text-align:center">

![Rede](/assets/rede22.png "Rede")<br>
Diagrama de pesquisa na topologia variável

</div>

A sincronização ocorre na rede entre os nós para novos nós e novos arquivos adicionados a rede, com propagação colaborativa de atualização das estruturas DHTs de nós e arquivos direta ou indiretamente. Existem duas chaves que guardam a ultima sincronização de nós e arquivos naquele nó que impede sincronização cíclica em loopback.

<div style="text-align:center">

![Rede](/assets/rede52.png "Rede")<br>
Diagrama de sincronização

</div>

## Implementação

A implementação foi pensada para permitir múltiplas plataformas, facilidade de subida de nós, linguagem de alto nível e extensa biblioteca de apoio.
Plataformas, linguagem e framework

Foi escolhida como linguagem C# +11 no framework de código aberto .Net Core +8. Este stack de desenvolvimento permite o uso de uma ampla biblioteca pronta para uso, linguagem moderna de alto nível multiparadigma e adoção facilitada de padrões de projeto.

Foram utilizados princípios SOLID no projeto como um todo, foi feita uma separação forte entre controles e serviços fazendo com que as estruturas principais da solução (as DHTs) tivessem suas operações acessíveis somente via serviços específicos e estas foram implementadas como dicionários concorrentes (thread safe) em classes Singleton que por sua vez são usadas exclusivamente via injeção de dependência.
Para a persistência de dados em banco foi usado o Entity Framework que é o MER padrão do .NET Core com o banco de dados SQLite e abordagem code-first.

## Como fazer o build & executar a solução

### Instalando o .NET Core 8 (Windows)

https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net80

```bash
winget install Microsoft.DotNet.SDK.8
```

### Instalando o .NET Core 8 (Linux)

https://learn.microsoft.com/en-us/dotnet/core/install/linux

**Fedora**

```bash
sudo dnf install dotnet-sdk-8.0
```

**Ubuntu**

```bash
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```

### Instalando o .NET Core 8 (macOS)

https://learn.microsoft.com/en-us/dotnet/core/install/macos


Efetue o download dos binários de <br>
https://dotnet.microsoft.com/download/dotnet

Execute os seguintes passos <br>
```bash
DOTNET_FILE=dotnet-sdk-8.0.100-osx-x64.tar.gz
export DOTNET_ROOT=$(pwd)/dotnet

mkdir -p "$DOTNET_ROOT" && tar zxf "$DOTNET_FILE" -C "$DOTNET_ROOT"

export PATH=$PATH:$DOTNET_ROOT
```

### Ajustando a identidade do nó

Abra o arquivo **appsettings.json** e ajuste as URLs HTTP e HTTPS do nó

> É imperativo nós (instâncias do aplicativo) rodando no mesmo host terem portas distintas, e as portas configuradas aqui não estarem em uso

```json
{
  "Urls": "http://localhost:5000;https://localhost:5001",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db;Cache=Shared"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Build & Execução

Na pasta do projeto execute o seguinte comando para fazer o build da solução

```bash
dotnet build
```

E o seguinte comando para executar a solução

```bash
dotnet run
```

### Checagem da subida do nó

É possivel checar o nó rodando executando a função de status do nó acessando o ponto HTTPS do nó, para o exemplo da configuração mostrada acima

```text
https://localhost:5001/node/status
```

> Apesar de existir a configuração HTTP, utilize somente a HTTPS para acesso ao nó

A saida deve ser proxima a seguinte

<div style="text-align:center">

![Status do nó](/assets/status.png "Status do nó")<br>
Status do nó

</div>

Note o carimbo de data/hora, estrutura de dados do nó, e diretório de cache onde o nó armazena seus arquivos para distribuição na rede

## Uma demonstração de funcionamento

### Subir o  nó em modo debug (para tornar o swagger acessível)

Swagger é uma UI padronizada que facilita invocar métodos REST do aplicativo sendo testado, para subir o nó e tornar seu swagger acessível é necessário subir o aplicativo em modo debug, para isso utilize a seguinte linha de comando na pasta do nó

```bash
dotnet run --configuration Debug
```

Acesse a página HTTPS do nó

```text
https://localhost:5001/swagger/index.html
```

> Apesar de existir a configuração HTTP, utilize somente a HTTPS para acesso ao nó

A saída deve ser próxima a seguinte

<div style="text-align:center">

![Status do nó](/assets/rest.png "Status do nó")<br>
Status do nó

</div>

### Funções

#### De Arquivo

**/file/list** - Lista os arquivos na DHT de arquivos do nó

**/file/synclocal** - Lista os arquivos na pasta de cache local e os adiciona na DHT de arquivos

**/file/sync** - Sincroniza a DHT de arquivos com os outros nós na rede (necessário subir mais de um nó e adicionar o nó na rede)

**/file/search** - Pesquisa um arquivo pelo hash na DHT de arquivos em memória e depois nos nós da rede de não encontrada executando a função searchlocal em cada um (necessário subir mais de um nó e adicionar o nó na rede)

**/file/searchlocal** - Pesquisa um arquivo pelo hash na DHT de arquivos em memória

#### De Nó

**/node/ping** - Retorna um carimbo de data hora para indicar que o nó está on line

**/node/status** - Retorna o status detalhado do nó

**/node/list** - Lista os nós da rede conhecidos pelo nó local

**/node/add** - Adiciona um nó a lista de nós conhecidos do nó local

**/node/remove** - Remove um nó da lista de nós conhecidos do nó local

**/node/sync** - Sincroniza a lista de nós conhecidos do nó local com as listas de nós conhecidos dos nós que o nó atuial conhece. Em outras palavras, esta função atualiza a lista de nós conhecidos entre os nós, se o nó A conhece B e B conhece C, após a execução dessa função A conhece C.

**/node/check** - Atualiza a lista de nós conhecidos verificando os que estão on line, via função ping em cada um

### Sugestão de passo a passo para demonstração

1. Triplicar a pasta do repositório

2. Configurar as **portas distintas** nos três nós

3. Subir os três nós distintos (A, B e C) usando o **modo debug**

4. Tomar nota dos diretórios de cache de cada um, adicionar dois arquivos distintos em cada pasta de cache, seis arquivos no total portanto

5. Listar nós conhecidos no nó A (deve retornar 1)

6. Abrir o nó A e adicionar o nó B, Sincronizar o nó A, Checar conhecidos no nó A

7. Listar nós conhecidos no nó A (deve retornar 2)

8. Abrir o nó B e adicionar o nó C, Sincronizar o nó B, Checar conhecidos no nó B

9. Listar nós conhecidos no nó B (deve retornar 2)

10. Abrir o nó A, Sincronizar o nó A, Checar conhecidos no nó A

11. Listar nós conhecidos no nó A (deve retornar 3)

12. Sincronizar **localmente** a lista de aquivos nos nós A, B e C

13. Listar arquivos conhecidos no nó A (deve retornar 2)

14. Listar arquivos conhecidos no nó C, tomar nota de um dos hashs de arquivo (deve retornar 3)

15. Pesquisar o hash de arquivo do nó C no nó A onde este arquivo **não é conhecido** ainda (deve retornar um arquivo no nó C, pesquisa usando REST)

16. Sincronizar a lista de aquivos no nó A

17. Listar arquivos conhecidos no nó A (deve retornar 6)

18. Pesquisar o hash de arquivo do nó C no nó A onde este arquivo **agora é conhecido** (deve retornar um arquivo no nó C, pesquisa usando DHT em memória)




## Referências

[1] Building a BitTorrent Client<br>
https://roadmap.sh/guides/torrent-client<br>
https://github.com/veggiedefender/torrent-client

[2] Simple, robust, BitTorrent wire protocol implementation<br>
https://github.com/webtorrent/bittorrent-protocol

[3] Kademlia Peer-to-Peer Distributed Hash Table Implementation in C#<br>
https://marcclifton.wordpress.com/2017/11/10/kademlia-peer-to-peer-distributed-hash-table-implementation-in-c/<br>
https://github.com/SyncfusionSuccinctlyE-Books/The-Kademlia-Protocol-Succinctly

Luiz Monnerat, Claudio L. Amorim<br>
[4] An effective single-hop distributed hash table with high lookup performance and low traffic overhead<br>
https://arxiv.org/abs/1408.7070

Teo Parashkevov<br>
[7] [Data Structures] Distributed hash table<br>
https://medium.com/the-code-vault/data-structures-distributed-hash-table-febfd01fc0af


## [Artigo completo](/assets/ARTIGO.pdf)

## [Slides](/assets/SLIDES.pdf)

___

CMCC - Universidade Federal do ABC (UFABC) - Santo André - SP - Brasil
