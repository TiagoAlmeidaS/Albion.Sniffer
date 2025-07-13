# Albion Online Sniffer (.NET 8)

Este projeto é um sniffer de rede modular para o jogo Albion Online, desenvolvido em C# com .NET 8. Ele captura pacotes de rede, faz parsing do protocolo Photon e publica eventos em filas (RabbitMQ ou Redis) para consumo por outros sistemas. O projeto é extensível, testável e pronto para integração em pipelines de dados ou sistemas de monitoramento.

---

## 🏗️ Estrutura do Projeto

- **AlbionOnlineSniffer.App**: Aplicação console principal, orquestra captura, parsing e publicação.
- **AlbionOnlineSniffer.Capture**: Captura de pacotes de rede (SharpPcap/PacketDotNet), filtro configurável.
- **AlbionOnlineSniffer.Core**: Modelos de dados, handlers de eventos, parser Photon, factories de dependências.
- **AlbionOnlineSniffer.Queue**: Publishers para RabbitMQ e Redis, interface padronizada.
- **AlbionOnlineSniffer.Tests**: Testes unitários e de integração para todos os módulos.

---

## ⚙️ Configuração

As configurações são feitas via `appsettings.json` em `AlbionOnlineSniffer.App`:

```json
{
  "PacketCaptureSettings": {
    "InterfaceName": "", // Nome da interface de rede (ex: Ethernet, Wi-Fi)
    "Filter": "udp and port 5056" // Filtro BPF para capturar apenas pacotes relevantes
  },
  "QueueSettings": {
    "PublisherType": "RabbitMQ", // Ou "Redis"
    "RabbitMQ": {
      "HostName": "localhost",
      "QueueName": "albion_online_packets"
    },
    "Redis": {
      "ConnectionString": "localhost:6379",
      "ChannelName": "albion_online_packets"
    }
  }
}
```

- **InterfaceName**: Nome da interface de rede a ser monitorada.
- **Filter**: Filtro BPF para capturar apenas pacotes UDP do Albion Online.
- **PublisherType**: Define se será usado RabbitMQ ou Redis.
- **RabbitMQ/Redis**: Configurações de conexão para o publisher escolhido.

> **Atenção:** Para descriptografar o tráfego do Albion Online, é necessário rodar o [Cryptonite](https://github.com/pxlbit228/albion-radar-deatheye-2pc) (ou ferramenta equivalente) em paralelo, conforme instruções do projeto original. O sniffer espera receber pacotes já descriptografados.

---

## 🚀 Como Executar

1. **Pré-requisitos:**
   - .NET SDK 8.0 ou superior
   - Npcap (Windows) ou libpcap (Linux/macOS)
   - RabbitMQ ou Redis em execução
   - [Cryptonite](https://github.com/pxlbit228/albion-radar-deatheye-2pc) rodando para descriptografia

2. **Clonar o Repositório:**
   ```bash
   git clone <URL_DO_REPOSITORIO>
   cd AlbionOnlineSniffer
   ```

3. **Restaurar Dependências:**
   ```bash
   dotnet restore
   ```

4. **Compilar o Projeto:**
   ```bash
   dotnet build
   ```

5. **Executar a Aplicação:**
   ```bash
   dotnet run --project src/AlbionOnlineSniffer.App
   ```

---

## 🧪 Testes Automatizados

Os testes são implementados com xUnit e cobrem handlers, parser, capturador e publishers. Para rodar:

```bash
dotnet test
```

---

## 🔄 Fluxo Resumido

1. **Captura:** Pacotes UDP são capturados da interface de rede.
2. **Parsing:** O parser Photon decodifica os pacotes e dispara eventos para handlers.
3. **Publicação:** Dados parseados são publicados em RabbitMQ ou Redis.
4. **Consumo:** Outros sistemas podem consumir os dados em tempo real.

---

## 📝 Contribuição

Contribuições são bem-vindas! Abra issues ou pull requests. Siga o padrão modular e adicione testes para novas funcionalidades.

---

## 📚 Próximos Passos
- Expandir o parser Photon para mais eventos do jogo
- Criar modelos de dados detalhados para cada tipo de evento
- Melhorar documentação e exemplos de integração
- (Opcional) Criar consumidores de fila para UI, dashboards ou analytics

---

## 📄 Licença

MIT. Veja o arquivo `LICENSE` para detalhes.

---

**Autor:** Manus AI / Comunidade
**Data:** 2025


