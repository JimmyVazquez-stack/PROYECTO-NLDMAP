namespace NLDMAP.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        //datos Oauth2
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string SsoProvider { get; set; }  //Google / Facebook 
        public required string SsoProviderId { get; set; } //Id unico que otorga el proveedor
        
        
        //perfil reportante
        public string? Curp { get; set; }
        public string? Sex { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? Schooling { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "Ciudadano";
        public int AlertRadioKm { get; set; } = 10;
        
        //control de anonimato
        public bool BeAnonymous { get; set; }
    }
}