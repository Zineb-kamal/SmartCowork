namespace SmartCowork.Services.Billing.Models.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Propriété de commodité pour le nom complet
        public string FullName => $"{FirstName} {LastName}";
    }
}
