using System;

namespace PutZige.Application.Settings
{
    public class HashingSettings
    {
        public const string SectionName = "HashingSettings";

        public int SaltSizeBytes { get; set; } = 32;
        public string Algorithm { get; set; } = "SHA512";
        public int Iterations { get; set; } = 100000;
    }
}