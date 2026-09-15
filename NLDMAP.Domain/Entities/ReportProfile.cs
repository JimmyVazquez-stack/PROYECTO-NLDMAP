using System;
using System.Collections.Generic;
using NLDMAP.Domain.ValueObjects;

namespace NLDMAP.Domain.Entities
{
    public class ReportProfile
    {
        //Id sera el mismo en postgres, MongoDB y Neo4j para enlace de informacion
        public Guid ReportId { get; private set; }

        public string FullName { get; private set; }
        public int Age { get; private set; }
        public string Gender { get; private set; }

        //Datos flexibles para mongo
        public PhysicalCharacteristics Characteristics { get; private set; }
        public ClothingDescription Clothing { get; private set; }

        //Usar IReadOnlyCollection para que nadie pueda modificar desde fuera DistinctiveFeatures
        private readonly List<string> _distinctiveFeatures = new List<string>();
        public IReadOnlyCollection<string> DistinctiveFeatures => _distinctiveFeatures.AsReadOnly();

        //constuctor privador requerido por ORM/Deserializacion MONGO
        private ReportProfile() { }

        //FactoryMethod para crear reportes validos
        public static ReportProfile Create(Guid reportId, string fullName, int age, string gender)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("El nombre completo es obligatorio");

            if (age < 0 || age > 120)
                throw new ArgumentException("Edad fuera de rango");

            return new ReportProfile
            {
                ReportId = reportId,
                FullName = fullName,
                Age = age,
                Gender = gender,
                Characteristics = new PhysicalCharacteristics(), //inician vacios y actualizan con metodos
                Clothing = new ClothingDescription(),
            };
        }

        //Metodos de comportamiento
        public void UpdateClothing(string top, string bottom, string footwear)
        {
            //Validaciones
            Clothing = new ClothingDescription(top, bottom, footwear);
        }
        
        
        public void UpdatePhysicalCharacteristics(double height, string build, string hairColor, string eyeColor)
        {
            //Validaciones
            Characteristics = new PhysicalCharacteristics(height, build, hairColor, eyeColor);
        }

        public void AddDistinctiveFeature(string feature)
        {
            if (!string.IsNullOrWhiteSpace(feature) && !_distinctiveFeatures.Contains(feature))
            {
                _distinctiveFeatures.Add(feature);
            }
        }
    }

    //value objects 
    public class PhysicalCharacteristics
    {
        public double Height { get; init; } //en metros
        public string Build { get; init; } //complexion
        public string HairColor { get; init; }
        public string EyeColor { get; init; }

        //constructor vacio para uso con mongo
        internal PhysicalCharacteristics()
        {
        }

        public PhysicalCharacteristics(double height, string build, string hairColor, string eyeColor)
        {
            Height = height;
            Build = build;
            HairColor = hairColor;
            EyeColor = eyeColor;
        }
    }
}
