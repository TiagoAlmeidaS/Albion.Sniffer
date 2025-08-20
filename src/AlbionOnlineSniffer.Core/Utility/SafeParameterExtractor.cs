using System;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Utility
{
    /// <summary>
    /// Utilitário para extrair parâmetros de forma segura, evitando KeyNotFoundException
    /// </summary>
    public static class SafeParameterExtractor
    {
        /// <summary>
        /// Extrai um valor inteiro de forma segura
        /// </summary>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser acessado</param>
        /// <param name="defaultValue">Valor padrão se o offset não existir</param>
        /// <returns>Valor extraído ou defaultValue</returns>
        public static int GetInt32(Dictionary<byte, object> parameters, byte offset, int defaultValue = 0)
        {
            if (parameters.ContainsKey(offset))
            {
                try
                {
                    return Convert.ToInt32(parameters[offset]);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Extrai um valor float de forma segura
        /// </summary>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser acessado</param>
        /// <param name="defaultValue">Valor padrão se o offset não existir</param>
        /// <returns>Valor extraído ou defaultValue</returns>
        public static float GetFloat(Dictionary<byte, object> parameters, byte offset, float defaultValue = 0f)
        {
            if (parameters.ContainsKey(offset))
            {
                try
                {
                    return Convert.ToSingle(parameters[offset]);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Extrai uma string de forma segura
        /// </summary>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser acessado</param>
        /// <param name="defaultValue">Valor padrão se o offset não existir</param>
        /// <returns>Valor extraído ou defaultValue</returns>
        public static string GetString(Dictionary<byte, object> parameters, byte offset, string defaultValue = "")
        {
            if (parameters.ContainsKey(offset))
            {
                try
                {
                    return parameters[offset] as string ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Extrai um byte de forma segura
        /// </summary>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser acessado</param>
        /// <param name="defaultValue">Valor padrão se o offset não existir</param>
        /// <returns>Valor extraído ou defaultValue</returns>
        public static byte GetByte(Dictionary<byte, object> parameters, byte offset, byte defaultValue = 0)
        {
            if (parameters.ContainsKey(offset))
            {
                try
                {
                    return Convert.ToByte(parameters[offset]);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Extrai um array de bytes de forma segura
        /// </summary>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser acessado</param>
        /// <param name="defaultValue">Valor padrão se o offset não existir</param>
        /// <returns>Valor extraído ou defaultValue</returns>
        public static byte[] GetByteArray(Dictionary<byte, object> parameters, byte offset, byte[]? defaultValue = null)
        {
            if (parameters.ContainsKey(offset) && parameters[offset] is byte[] result)
            {
                return result;
            }
            return defaultValue ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Extrai um array de float de forma segura
        /// </summary>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser acessado</param>
        /// <param name="defaultValue">Valor padrão se o offset não existir</param>
        /// <returns>Valor extraído ou defaultValue</returns>
        public static float[] GetFloatArray(Dictionary<byte, object> parameters, byte offset, float[]? defaultValue = null)
        {
            if (parameters.ContainsKey(offset) && parameters[offset] is float[] result)
            {
                return result;
            }
            return defaultValue ?? Array.Empty<float>();
        }

        /// <summary>
        /// Verifica se um offset existe e extrai o valor se existir
        /// </summary>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser verificado</param>
        /// <param name="value">Valor extraído (se existir)</param>
        /// <returns>True se o offset existir, false caso contrário</returns>
        public static bool TryGetValue<T>(Dictionary<byte, object> parameters, byte offset, out T? value)
        {
            if (parameters.ContainsKey(offset) && parameters[offset] is T result)
            {
                value = result;
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Extrai um valor com conversão de tipo de forma segura
        /// </summary>
        /// <typeparam name="T">Tipo de destino</typeparam>
        /// <param name="parameters">Dicionário de parâmetros</param>
        /// <param name="offset">Offset a ser acessado</param>
        /// <param name="defaultValue">Valor padrão se o offset não existir ou conversão falhar</param>
        /// <returns>Valor convertido ou defaultValue</returns>
        public static T GetValue<T>(Dictionary<byte, object> parameters, byte offset, T defaultValue)
        {
            if (parameters.ContainsKey(offset))
            {
                try
                {
                    if (parameters[offset] is T result)
                    {
                        return result;
                    }
                    
                    // Tentar conversão para tipos básicos
                    var obj = parameters[offset];
                    if (typeof(T) == typeof(int))
                    {
                        return (T)(object)Convert.ToInt32(obj);
                    }
                    if (typeof(T) == typeof(float))
                    {
                        return (T)(object)Convert.ToSingle(obj);
                    }
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)(obj?.ToString() ?? "");
                    }
                    if (typeof(T) == typeof(byte))
                    {
                        return (T)(object)Convert.ToByte(obj);
                    }
                }
                catch
                {
                    // Ignorar erros de conversão
                }
            }
            return defaultValue;
        }
    }
}
