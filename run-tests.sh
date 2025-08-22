#!/bin/bash

# Script para executar testes localmente
set -e

echo "🧪 Albion Online Sniffer - Test Runner"
echo "======================================"
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Argumentos padrão
TEST_FILTER=""
TEST_TYPE="all"
COVERAGE=false
WATCH=false

# Parse argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        --unit)
            TEST_TYPE="unit"
            TEST_FILTER="Category!=Integration&Category!=E2E"
            shift
            ;;
        --integration)
            TEST_TYPE="integration"
            TEST_FILTER="Category=Integration"
            shift
            ;;
        --e2e)
            TEST_TYPE="e2e"
            TEST_FILTER="Category=E2E"
            shift
            ;;
        --coverage)
            COVERAGE=true
            shift
            ;;
        --watch)
            WATCH=true
            shift
            ;;
        --filter)
            TEST_FILTER="$2"
            shift 2
            ;;
        --help)
            echo "Uso: $0 [opções]"
            echo ""
            echo "Opções:"
            echo "  --unit          Executa apenas testes unitários"
            echo "  --integration   Executa apenas testes de integração"
            echo "  --e2e           Executa apenas testes E2E"
            echo "  --coverage      Gera relatório de cobertura"
            echo "  --watch         Executa em modo watch"
            echo "  --filter <exp>  Aplica filtro customizado"
            echo "  --help          Mostra esta ajuda"
            echo ""
            echo "Exemplos:"
            echo "  $0 --unit                    # Executa apenas testes unitários"
            echo "  $0 --integration --coverage  # Testes de integração com cobertura"
            echo "  $0 --filter \"FullyQualifiedName~PhotonParser\"  # Testes específicos"
            exit 0
            ;;
        *)
            echo -e "${RED}Argumento desconhecido: $1${NC}"
            echo "Use --help para ver as opções disponíveis"
            exit 1
            ;;
    esac
done

# Restaura dependências
echo -e "${YELLOW}📦 Restaurando dependências...${NC}"
dotnet restore

# Build
echo -e "${YELLOW}🔨 Compilando projeto...${NC}"
dotnet build --no-restore --configuration Release

# Prepara comando de teste
TEST_CMD="dotnet"

if [ "$WATCH" = true ]; then
    TEST_CMD="$TEST_CMD watch test"
else
    TEST_CMD="$TEST_CMD test"
fi

TEST_CMD="$TEST_CMD --no-build --configuration Release"

if [ -n "$TEST_FILTER" ]; then
    TEST_CMD="$TEST_CMD --filter \"$TEST_FILTER\""
fi

TEST_CMD="$TEST_CMD --logger \"console;verbosity=normal\""

if [ "$COVERAGE" = true ]; then
    TEST_CMD="$TEST_CMD --collect:\"XPlat Code Coverage\""
    TEST_CMD="$TEST_CMD --results-directory ./TestResults"
    TEST_CMD="$TEST_CMD -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover"
fi

# Executa testes
echo ""
echo -e "${YELLOW}🚀 Executando testes ($TEST_TYPE)...${NC}"
echo "Comando: $TEST_CMD"
echo ""

eval $TEST_CMD
TEST_RESULT=$?

# Gera relatório de cobertura se solicitado
if [ "$COVERAGE" = true ] && [ $TEST_RESULT -eq 0 ]; then
    echo ""
    echo -e "${YELLOW}📊 Gerando relatório de cobertura...${NC}"
    
    # Instala ReportGenerator se não estiver instalado
    if ! command -v reportgenerator &> /dev/null; then
        echo "Instalando ReportGenerator..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
    fi
    
    # Gera relatório HTML
    reportgenerator \
        -reports:./TestResults/**/coverage.opencover.xml \
        -targetdir:./CoverageReport \
        -reporttypes:"Html;Cobertura;TextSummary"
    
    echo ""
    echo -e "${GREEN}✅ Relatório de cobertura gerado em: ./CoverageReport/index.html${NC}"
    
    # Mostra resumo no console
    if [ -f "./CoverageReport/Summary.txt" ]; then
        echo ""
        echo "📈 Resumo da Cobertura:"
        cat ./CoverageReport/Summary.txt
    fi
fi

# Resultado final
echo ""
if [ $TEST_RESULT -eq 0 ]; then
    echo -e "${GREEN}✅ Testes executados com sucesso!${NC}"
else
    echo -e "${RED}❌ Alguns testes falharam!${NC}"
    exit $TEST_RESULT
fi