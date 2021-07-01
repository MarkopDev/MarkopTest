using System;

namespace Application.DTOs.User
{
    public class ProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
    }
}