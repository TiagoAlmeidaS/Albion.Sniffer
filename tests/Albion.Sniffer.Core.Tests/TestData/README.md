# Test Data Files

This directory contains binary test data files and their expected golden outputs for testing the Albion.Sniffer protocol parser.

## Directory Structure

```
TestData/
├── packets/        # Raw binary packet files
│   ├── newchar.bin    # NEW_CHARACTER packet sample
│   ├── movepos.bin    # MOVE_POSITION packet sample
│   ├── chat.bin       # CHAT message packet sample
│   └── ...
└── golden/         # Expected parsed outputs (JSON)
    ├── newchar.json   # Expected parsing of newchar.bin
    ├── movepos.json   # Expected parsing of movepos.bin
    ├── chat.json      # Expected parsing of chat.bin
    └── ...
```

## File Formats

### Binary Packet Files (.bin)

Raw binary data captured from Albion Online network traffic. Each file contains a single packet with:
- 2 bytes: Opcode (little-endian)
- N bytes: Payload (format depends on opcode)

### Golden Output Files (.json)

Expected parsing results in JSON format containing:
- `eventType`: String identifier of the event
- `opcode`: Numeric opcode value
- `fields`: Parsed fields specific to each event type
- `size`: Total packet size in bytes

## Adding New Test Cases

1. Capture or create a new binary packet file
2. Save it in `packets/` with descriptive name
3. Create corresponding expected output in `golden/`
4. Add test case to `Protocol16DeserializerTests.cs`

## Sample Packet Structures

### NEW_CHARACTER (0x0001)
```
[ushort] opcode = 0x0001
[uint]   playerId
[string] playerName (length-prefixed)
[int]    health
[int]    mana
```

### MOVE_POSITION (0x0002)
```
[ushort] opcode = 0x0002
[uint]   timestamp
[float]  x
[float]  y
[float]  z
```

### CHAT_MESSAGE (0x0003)
```
[ushort] opcode = 0x0003
[byte]   channel
[string] sender (length-prefixed)
[string] message (length-prefixed)
```