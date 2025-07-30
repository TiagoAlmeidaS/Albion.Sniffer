using System;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa a vida de um jogador ou mob
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class Health
    {
        private int _value;

        public Health(int maxValue)
        {
            MaxValue = maxValue;
            Value = maxValue;
            Regeneration = 0f;
            IsRegeneration = false;
        }

        public Health(int value, int maxValue)
        {
            MaxValue = maxValue;
            Value = value;
            Regeneration = 0f;
            IsRegeneration = false;
        }

        public Health(int value, int maxValue, float regeneration)
        {
            MaxValue = maxValue;
            Value = value;
            Regeneration = regeneration;
            IsRegeneration = true;
        }

        public int Value
        {
            get => _value;
            set
            {
                if (value >= MaxValue)
                {
                    _value = MaxValue;
                    IsRegeneration = false;
                }
                else
                {
                    _value = value;
                }
            }
        }

        public int MaxValue { get; set; }
        public float Regeneration { get; set; }
        public bool IsRegeneration { get; set; }

        public string StrPercent => Percent.ToString() + "%";

        public int Percent
        {
            get
            {
                if (Value * 100 == 0)
                    return 0;

                return (Value * 100 / MaxValue);
            }
        }

        // Propriedades para compatibilidade com o código existente
        public int CurrentHealth => Value;
        public int MaxHealth => MaxValue;
    }
} 