using System;

namespace NLDMAP.Domain.ValueObjects
{

    public class PhysicalCharacteristics
    {
        public double Height { get; init; }
        public string Build { get; init; }
        public string HairColor { get; init; }
        public string EyeColor { get; init; }
        
        //constructor para que el driver de mongodb no tenga conflictos al leer el json
        internal PhysicalCharacteristics()
        {
            Build = string.Empty;
            HairColor = string.Empty;
            EyeColor = string.Empty;
        }
        
        //Constructor publico donde exigimos datos para crear Value Objects
        public PhysicalCharacteristics(double height, string build, string hairColor, string eyeColor)
        {
            if (height < 0 || height > 3.0)
                throw new ArgumentException("La estatura ingresada no es valida");
            
            Height = height;
            Build = build ?? string.Empty;
            HairColor = hairColor ?? string.Empty;
            EyeColor = eyeColor ?? string.Empty;
        }
        
    }

}