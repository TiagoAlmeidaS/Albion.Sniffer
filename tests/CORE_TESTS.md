# 🧪 Albion.Sniffer Core Tests Documentation

## 📋 Overview

This document describes the unit testing strategy and implementation for the Core layer of Albion.Sniffer, a .NET 8 application following Clean Architecture principles.

## 🎯 Objectives

- **85%+ code coverage** for Core layer (Domain + Application)
- **Deterministic tests** without external dependencies
- **Property-based testing** for critical components
- **Golden tests** with known binary vectors
- **CI/CD integration** with automated coverage reporting

## 🏗️ Architecture

```
tests/
└── Albion.Sniffer.Core.Tests/
    ├── Domain/              # Domain model tests
    ├── Protocol16/          # Protocol deserializer tests
    ├── Normalization/       # Event normalizer tests
    ├── Utils/              # Utility function tests
    ├── Helpers/            # Test helpers and builders
    ├── Fixtures/           # Test fixtures
    └── TestData/           # Binary test data
        ├── packets/        # Raw packet files
        └── golden/         # Expected outputs
```

## 🛠️ Technology Stack

| Package | Version | Purpose |
|---------|---------|---------|
| xUnit | 2.9.0 | Test framework |
| FluentAssertions | 6.12.0 | Readable assertions |
| AutoFixture | 4.18.1 | Test data generation |
| FsCheck | 2.16.6 | Property-based testing |
| NSubstitute | 5.1.0 | Mocking framework |
| Moq | 4.20.72 | Alternative mocking |
| Coverlet | 6.0.2 | Code coverage |
| Verify | 26.2.0 | Snapshot testing |

## 🚀 Running Tests

### Local Development

```bash
# Run all tests
dotnet test tests/Albion.Sniffer.Core.Tests/

# Run with coverage
dotnet test tests/Albion.Sniffer.Core.Tests/ \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=TestResults/

# Run specific test category
dotnet test --filter "Category=Protocol16"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Coverage Report

```bash
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:TestResults/coverage.cobertura.xml \
  -targetdir:coverage-report \
  -reporttypes:Html

# Open report
open coverage-report/index.html
```

## 📊 Test Categories

### 1. Binary Reader Tests (`ByteReaderTests`)
- ✅ Read primitives (byte, ushort, uint)
- ✅ Read strings (length-prefixed UTF-8)
- ✅ Boundary conditions
- ✅ Buffer overflow protection
- ✅ Round-trip properties

### 2. Protocol16 Deserializer Tests
- ✅ Known packet parsing (golden tests)
- ✅ Unknown opcode handling
- ✅ Truncated buffer detection
- ✅ Invalid data resilience
- ✅ UTF-8 string support

### 3. Event Normalizer Tests
- ✅ Direction inference (port-based)
- ✅ Correlation ID stability
- ✅ Timestamp monotonicity
- ✅ Session consistency
- ✅ Payload preservation

### 4. Utility Tests
- ✅ HexDump formatting
- ✅ Endian conversion
- ✅ Checksum calculation
- ✅ Binary helpers

## 🔍 Property-Based Tests

Using FsCheck for property validation:

```csharp
[Property(MaxTest = 200)]
public void Reader_Never_Reads_Past_End(byte[] data)
{
    var reader = new ByteReader(data ?? Array.Empty<byte>());
    
    while (reader.CanRead(1))
        reader.ReadByte();
    
    reader.Invoking(r => r.ReadByte())
        .Should().Throw<EndOfStreamException>();
}
```

## 📦 Test Data Management

### Golden Test Files

Located in `TestData/packets/`:
- `newchar.bin` - NEW_CHARACTER packet
- `movepos.bin` - MOVE_POSITION packet
- `chat.bin` - CHAT_MESSAGE packet

### Test Data Builders

```csharp
// Create test packets
var packet = TestDataBuilder.BuildNewCharacterPacket("Player", 123);
var movePacket = TestDataBuilder.BuildMovePacket(100.5f, 200.3f, 50.0f);

// Generate random data
var bytes = TestDataBuilder.GenerateRandomBytes(100);
```

## ✅ Coverage Requirements

| Component | Target | Current |
|-----------|--------|---------|
| ByteReader | 90% | - |
| Protocol16Deserializer | 85% | - |
| EventNormalizer | 85% | - |
| HexDump | 90% | - |
| BinaryUtils | 90% | - |
| **Overall Core** | **85%** | **-** |

## 🔄 CI/CD Integration

### GitHub Actions Workflow

- **Trigger**: Push/PR to main/develop
- **Steps**:
  1. Setup .NET 8.0
  2. Restore dependencies
  3. Build solution
  4. Run tests with coverage
  5. Generate reports
  6. Upload to Codecov
  7. Quality gate check (85% threshold)

### Badge Integration

```markdown
![Coverage](https://codecov.io/gh/[org]/[repo]/branch/main/graph/badge.svg?flag=core-tests)
![Tests](https://github.com/[org]/[repo]/actions/workflows/core-tests.yml/badge.svg)
```

## 📝 Test Conventions

### Naming Convention
```
MethodName_Should_ExpectedBehavior_When_Condition
```

Example:
```csharp
public void ReadUInt16_Should_ReturnCorrectValue_When_LittleEndian()
```

### Test Structure (AAA)
```csharp
[Fact]
public void TestMethod()
{
    // Arrange
    var data = new byte[] { 0x01, 0x02 };
    
    // Act
    var result = ProcessData(data);
    
    // Assert
    result.Should().NotBeNull();
}
```

## 🐛 Debugging Tests

### Run single test
```bash
dotnet test --filter "FullyQualifiedName~ByteReaderTests.ReadByte"
```

### Debug with VS Code
```json
{
    "type": "coreclr",
    "request": "launch",
    "name": "Debug Tests",
    "program": "${workspaceFolder}/tests/Albion.Sniffer.Core.Tests/bin/Debug/net8.0/Albion.Sniffer.Core.Tests.dll",
    "args": [],
    "cwd": "${workspaceFolder}",
    "console": "internalConsole"
}
```

## 🚧 Known Issues & Limitations

1. **Mock Photon Receiver**: Current tests use simplified protocol implementation
2. **Binary Test Data**: Limited samples, need more edge cases
3. **Performance Tests**: Basic timing checks, could use BenchmarkDotNet

## 🔮 Future Improvements

- [ ] Add mutation testing (Stryker.NET)
- [ ] Implement fuzzing for packet parser
- [ ] Add performance benchmarks
- [ ] Create test data generator tool
- [ ] Add integration test layer

## 📚 References

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Best Practices](https://fluentassertions.com/best-practices/)
- [Property-Based Testing with FsCheck](https://fscheck.github.io/FsCheck/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)

## 🤝 Contributing

When adding new tests:

1. Follow existing patterns and conventions
2. Ensure tests are deterministic
3. Add appropriate test data files
4. Update this documentation
5. Maintain coverage above 85%

---

*Last Updated: 2024*
*Version: 1.0.0*