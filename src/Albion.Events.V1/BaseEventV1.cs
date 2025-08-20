namespace Albion.Events.V1;

/// <summary>
/// Classe base para todos os eventos V1 com validação automática de valores float
/// </summary>
public abstract class BaseEventV1
{
    /// <summary>
    /// Valida float e substitui valores inválidos por 0
    /// </summary>
    protected static float ValidateFloat(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return 0f;
        }
        return value;
    }

    /// <summary>
    /// Valida array de floats e substitui valores inválidos por 0
    /// </summary>
    protected static float[] ValidateFloatArray(float[] values)
    {
        if (values == null) return Array.Empty<float>();
        
        var validated = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            validated[i] = ValidateFloat(values[i]);
        }
        return validated;
    }

    /// <summary>
    /// Valida Vector2 e substitui valores inválidos por Vector2.Zero
    /// </summary>
    protected static (float X, float Y) ValidateVector2(float x, float y)
    {
        return (ValidateFloat(x), ValidateFloat(y));
    }
}
