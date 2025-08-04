namespace Albion.Sniffer.Core.Enums
{
    public enum Protocol16Type : byte
    {
        // Tipos de dados primitivos
        Byte = 0,
        Boolean = 1,
        Short = 2,
        Integer = 3,
        Long = 4,
        Float = 5,
        Double = 6,
        String = 7,

        // Tipos de array
        ByteArray = 8,
        IntegerArray = 9,
        StringArray = 10,
        ObjectArray = 11,

        // Tipos complexos
        Hashtable = 12,
        Dictionary = 13,

        // Outros
        Null = 255 // Representa um valor nulo
    }
}

