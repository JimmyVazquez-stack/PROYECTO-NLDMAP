
namespace NLDMAP.Domain.Entities
{

    public class MissingPerson
    {
        //Identidad
        public Guid Id { get; set; } = Guid.NewGuid();

        //Propiedades encapsuladas
        public string Name { get; private set; }
        public string? LastName { get; private set; }
        public string? Curp { get; private set; } //opcional si se desconoce
        public DateTime? DateOfBirth { get; private set; } // Calculado o nulo
        public int Age { get; private set; }

        public bool HasDisability { get; private set; } //si / no
        public string? Sex { get; private set; }
        public string Gender { get; private set; } //Masculino, femenino, indeterminado
        public string? Nationality { get; private set; }

        //Propiedades opcionales que se pueden llenar despues
        public double? Height { get; private set; }
        public string? IdentifyingSigns { get; private set; } //tatuaje, cicatriz ...
        public string? PhotographyUrl { get; private set; } //ruta a img almacenada

        //Historial de reportes
        public IReadOnlyCollection<Report> ReportHistory { get; private set; } = new List<Report>();


        //Constructor vacio requerido por EF
        protected MissingPerson()
        {
        }

        //Constructor de dominio, alineado con los datos iniciales disponibles
        public MissingPerson(string name, int age, string gender, double? height)
        {
            //validaciones
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre es obligatorio");

            if (age < 0)
                throw new ArgumentException("La edad no puede ser un numero negativo");

            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("El género es obligatorio");
            
            //Asignaciones
            Name = name;
            Age = age;
            Gender = gender;
            Height = height; 

            // Inicializar propiedades opcionales que no se pasan en el constructor
            LastName = null;
            Sex = null;
            Nationality = null;
        }
        
        //Metodo para actualizar datos opcionales de la persona
        public void SetAdditionalDetails(bool hasDisability, string? identifyingSigns,
            string? photographyUrl)
        {
            HasDisability = hasDisability;
            IdentifyingSigns = identifyingSigns;
            PhotographyUrl = photographyUrl;
        }
    }
}

