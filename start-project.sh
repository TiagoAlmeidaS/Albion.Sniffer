#!/bin/bash

# Script para iniciar o projeto AlbionOnlineSniffer

# Caminho para o dotnet SDK
DOTNET_PATH="/usr/local/share/dotnet/dotnet"

# Verifica se o dotnet está no PATH, caso contrário usa o caminho completo
if command -v dotnet &> /dev/null; then
    DOTNET_CMD="dotnet"
else
    DOTNET_CMD="$DOTNET_PATH"
    echo "Usando dotnet em $DOTNET_PATH"
fi

# Restaura as dependências do projeto
echo "Restaurando dependências..."
$DOTNET_CMD restore AlbionOnlineSniffer.sln

# Compila o projeto
echo "Compilando o projeto..."
$DOTNET_CMD build AlbionOnlineSniffer.sln

# Verifica se a compilação foi bem-sucedida
if [ $? -eq 0 ]; then
    echo "Compilação concluída com sucesso!"
    
    # Abre o projeto no Rider, se disponível
    if [ -d "/Users/tiagoalmeida/Applications/Rider.app" ]; then
        echo "Abrindo o projeto no Rider..."
        open -a "/Users/tiagoalmeida/Applications/Rider.app" AlbionOnlineSniffer.sln
    else
        echo "Rider não encontrado em /Users/tiagoalmeida/Applications/Rider.app"
    fi
else
    echo "Erro na compilação do projeto."
    exit 1
fi