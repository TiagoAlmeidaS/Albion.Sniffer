# 📦 Documentação de Publicação: `AlbionSniffer.App`

## 🎯 Objetivo

Gerar um **executável único e autossuficiente (.exe)** para o projeto `AlbionSniffer.App`, com todas as dependências incluídas, pronto para rodar em qualquer máquina Windows 64 bits **sem necessidade de .NET Runtime instalado**.

---

## 🗂️ Estrutura Recomendada do Projeto

Assumindo que o projeto siga um layout em camadas, o executável será gerado a partir do projeto de entrada `AlbionSniffer.App`:

```
/AlbionSniffer.App
  ├─ Program.cs
  ├─ AlbionSniffer.App.csproj
/AlbionSniffer.Core
/AlbionSniffer.Infrastructure
/... bibliotecas comuns
```

---

## ⚙️ Configuração do `.csproj`

Abra o arquivo `AlbionSniffer.App.csproj` e adicione:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>

  <!-- Publicação como executável único -->
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <PublishTrimmed>false</PublishTrimmed>

  <!-- Opcional: embutir bibliotecas nativas -->
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

> 📌 O `SelfContained=true` garante que o runtime do .NET seja distribuído junto ao executável, permitindo execução em máquinas que não possuem .NET instalado.

---

## 🏗️ Comando para Gerar Executável

No terminal, posicionado na raiz do repositório, execute:

```bash
dotnet publish src/AlbionSniffer.App/AlbionSniffer.App.csproj -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true -o ./publish
```

* `-c Release`: compila em modo otimizado
* `-r win-x64`: runtime alvo (Windows 64 bits)
* `--self-contained true`: embute o runtime do .NET
* `-o ./publish`: pasta destino

Após o comando, o executável estará disponível em:

```
./publish/AlbionSniffer.App.exe
```

---

## 📁 Conteúdo da Pasta de Publicação

Exemplo típico:

```
publish/
├─ AlbionSniffer.App.exe
├─ AlbionSniffer.App.pdb         # (opcional, debug)
├─ AlbionSniffer.App.deps.json   # dependências
├─ AlbionSniffer.App.runtimeconfig.json
```

> ✅ Apenas o `.exe` é necessário para execução. Inclua os `.json` somente se o aplicativo utilizar configurações específicas.

---

## 🔒 Permissões e Firewall

Se o sniffer utilizar `SharpPcap` para capturar pacotes:

* O executável deve ser executado com privilégios de **Administrador** para acessar a interface de rede.
* Crie um atalho do `.exe` e configure:
  * Propriedades → Avançado → **Executar como administrador**

---

## 🧪 Validação Pós-Build

Checklist sugerido:

| Item                                       | OK |
| ------------------------------------------ | -- |
| Executável gerado em `/publish`            | ✅  |
| Roda em ambiente limpo (sem .NET)          | ✅  |
| Captura pacotes na porta 5050              | ✅  |
| Publica eventos no RabbitMQ                | ✅  |
| Logs gravados corretamente                 | ✅  |

---

## 🛠️ (Opcional) Empacotamento como .zip

Para distribuição simplificada:

```bash
Compress-Archive -Path ./publish/* -DestinationPath AlbionSniffer.App.zip
```

---

## 🧪 Testes Sugeridos

1. Executar em VM ou máquina sem .NET instalado.
2. Verificar detecção automática das interfaces de rede.
3. Validar captura de pacotes (modo debug pode ajudar).
4. Confirmar publicação de eventos via RabbitMQ.

---

## 🤖 (Futuro) Automatização com GitHub Actions

É possível criar um workflow de CI/CD para gerar o `.exe` e empacotar em `.zip` automaticamente a cada release. Caso deseje, este processo pode ser documentado ou automatizado em outro momento.

---

## ✅ Conclusão

Seguindo os passos acima, o `AlbionSniffer.App` será publicado como um executável portátil, ideal para execução em máquinas Windows que não possuem o .NET instalado, facilitando:

* Uso isolado em VMs ou servidores dedicados.
* Distribuição para outros componentes da solução.
* Execução offline sem dependências externas.

