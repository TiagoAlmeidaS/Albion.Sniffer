# 🔒 Migração de Segurança dos Eventos - SafeParameterExtractor

## 🎯 **Objetivo**
Migrar todos os eventos para usar `SafeParameterExtractor` ao invés de acessar diretamente `parameters[offsets[index]]`, evitando `KeyNotFoundException`.

## 📊 **Mapeamento de Eventos Afetados**

### **🔴 ALTA PRIORIDADE - KeyNotFoundException frequente:**

#### **1. HealthUpdateEvent** ✅ **MIGRADO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Models/Events/HealthUpdateEvent.cs`
- **Problema:** `parameters[offsets[3]]` - offset 3 pode não existir
- **Status:** ✅ **CORRIGIDO**
- **⚠️ CORREÇÃO ADICIONAL:** `IndexOutOfRangeException` - offsets.json tem apenas [0,3] mas código tentava acessar [0,1,2,3]

#### **2. NewCharacterEvent** ✅ **MIGRADO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Models/Events/NewCharacterEvent.cs`
- **Problema:** `parameters[offsets[2]]` e `parameters[offsets[3]]` - offsets 8 e 51 podem não existir
- **Status:** ✅ **CORRIGIDO**

### **🟡 MÉDIA PRIORIDADE - Potencial KeyNotFoundException:**

#### **3. ChangeClusterEvent** ✅ **MIGRADO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Models/Events/ChangeClusterEvent.cs`
- **Problema:** `parameters[offsets[1,2]]` - acessos diretos no segundo construtor
- **Status:** ✅ **CORRIGIDO**

#### **4. NewDungeonEvent** ✅ **MIGRADO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Models/Events/NewDungeonEvent.cs`
- **Problema:** `parameters[offsets[0,1,2,3]]` - múltiplos acessos diretos
- **Status:** ✅ **CORRIGIDO**

#### **5. NewHarvestableEvent** ✅ **MIGRADO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Models/Events/NewHarvestableEvent.cs`
- **Problema:** `parameters[offsets[0-4]]` - múltiplos acessos diretos
- **Status:** ✅ **CORRIGIDO**

#### **6. NewMobEvent** ✅ **MIGRADO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Models/Events/NewMobEvent.cs`
- **Problema:** `parameters[offsets[0-5]]` - múltiplos acessos diretos
- **Status:** ✅ **CORRIGIDO**

### **🟢 BAIXA PRIORIDADE - Já usa ContainsKey:**

#### **7. JoinResponseOperation** ✅ **JÁ SEGURO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Handlers/JoinResponseOperation.cs`
- **Status:** ✅ **Já usa ContainsKey**

#### **8. MoveRequestOperation** ✅ **JÁ SEGURO**
- **Arquivo:** `src/AlbionOnlineSniffer.Core/Handlers/MoveRequestOperation.cs`
- **Status:** ✅ **Já usa ContainsKey**

## 🛠️ **Padrão de Migração**

### **❌ ANTES (INSEGURO):**
```csharp
Id = Convert.ToInt32(parameters[offsets[0]]);
Name = (string)parameters[offsets[1]] ?? string.Empty;
Health = Convert.ToSingle(parameters[offsets[2]]);
```

### **✅ DEPOIS (SEGURO):**
```csharp
Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
Name = SafeParameterExtractor.GetString(parameters, offsets[1]);
Health = SafeParameterExtractor.GetFloat(parameters, offsets[2]);
```

## 📝 **Métodos Disponíveis no SafeParameterExtractor**

| Tipo | Método | Descrição |
|------|---------|-----------|
| **int** | `GetInt32(parameters, offset, defaultValue = 0)` | Extrai inteiro com valor padrão |
| **float** | `GetFloat(parameters, offset, defaultValue = 0f)` | Extrai float com valor padrão |
| **string** | `GetString(parameters, offset, defaultValue = "")` | Extrai string com valor padrão |
| **byte** | `GetByte(parameters, offset, defaultValue = 0)` | Extrai byte com valor padrão |
| **byte[]** | `GetByteArray(parameters, offset, defaultValue = null)` | Extrai array de bytes |
| **float[]** | `GetFloatArray(parameters, offset, defaultValue = null)` | Extrai array de floats |
| **T** | `GetValue<T>(parameters, offset, defaultValue)` | Extrai com conversão de tipo |

## 🚀 **Próximos Passos**

### **1. Migrar Eventos de Alta Prioridade** ✅ **COMPLETO**
- [x] HealthUpdateEvent
- [x] NewCharacterEvent

### **2. Migrar Eventos de Média Prioridade** ✅ **COMPLETO**
- [x] ChangeClusterEvent
- [x] NewDungeonEvent  
- [x] NewHarvestableEvent
- [x] NewMobEvent

### **3. Verificar Eventos de Baixa Prioridade** ✅ **VERIFICADO**
- [x] JoinResponseOperation
- [x] MoveRequestOperation
- [x] Outros que já usam ContainsKey



## 🎉 **Benefícios da Migração**

1. **✅ Zero KeyNotFoundException** - Sistema nunca trava por offsets inexistentes
2. **✅ Zero IndexOutOfRangeException** - Sistema nunca trava por tentar acessar offsets fora do array
3. **✅ Valores padrão inteligentes** - Campos ficam com valores sensatos ao invés de vazios
4. **✅ Conversão de tipos segura** - Tratamento de erros de conversão
5. **✅ Código mais limpo** - Menos verificações manuais de ContainsKey
6. **✅ Manutenibilidade** - Padrão consistente em todos os eventos
7. **✅ Compatibilidade** - Funciona com diferentes versões do jogo

## 🔧 **Como Aplicar**

Para cada evento:
1. Adicionar `using AlbionOnlineSniffer.Core.Utility;`
2. Substituir `Convert.ToInt32(parameters[offsets[index]])` por `SafeParameterExtractor.GetInt32(parameters, offsets[index])`
3. Substituir `(string)parameters[offsets[index]] ?? ""` por `SafeParameterExtractor.GetString(parameters, offsets[index])`
4. **⚠️ IMPORTANTE:** Verificar se o número de offsets no código corresponde ao `offsets.json`
5. Testar compilação
6. Verificar se eventos funcionam sem KeyNotFoundException e IndexOutOfRangeException

## ⚠️ **Problemas Adicionais Identificados**

### **IndexOutOfRangeException**
Além do `KeyNotFoundException`, identificamos outro problema: **mismatch entre offsets definidos no código e no `offsets.json`**.

#### **Exemplo - HealthUpdateEvent:**
- **offsets.json:** `"HealthUpdateEvent": [0, 3]` (2 offsets)
- **Código antigo:** `new byte[] { 0, 1, 2, 3 }` (4 offsets)
- **Resultado:** `IndexOutOfRangeException` ao tentar acessar `offsets[1]`, `offsets[2]`

#### **Solução Aplicada:**
```csharp
// ✅ ANTES (INSEGURO):
offsets = packetOffsets?.HealthUpdateEvent ?? new byte[] { 0, 1, 2, 3 };
Health = SafeParameterExtractor.GetFloat(parameters, offsets[1]);        // ❌ offsets[1] não existe
MaxHealth = SafeParameterExtractor.GetFloat(parameters, offsets[2]);     // ❌ offsets[2] não existe

// ✅ DEPOIS (SEGURO):
offsets = packetOffsets?.HealthUpdateEvent ?? new byte[] { 0, 3 };
Health = SafeParameterExtractor.GetFloat(parameters, offsets[1], 100f);  // ✅ Valor padrão se offset[1] existir
MaxHealth = 100f;                                                        // ✅ Valor padrão fixo
```
