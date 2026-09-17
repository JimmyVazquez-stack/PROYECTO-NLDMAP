namespace NLDMAP.Domain.ValueObjects
{
    public class ExternalUserInfo
    {
        public required string Email { get; init; }  //accesor init para que propiedad Email sea inmutable
        public required string FullName { get; init; } 
        
        public required string ProviderId { get; init; }
        
        //guardar proveedor
        public string ProviderName { get; init; } = "Google";

    }
}