using System;
using Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser, IEntityBase
    {
        public bool? IsEnable { get; set; }
        public bool IsDeleted { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}