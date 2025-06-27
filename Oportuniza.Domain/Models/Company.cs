﻿namespace Oportuniza.Domain.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public Guid UserId { get; set; }
        public virtual User Manager { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<CompanyEmployee> Employees  { get; set; } = new List<CompanyEmployee>();
        public virtual ICollection<Publication> AuthoredPublications { get; set; }

    }
}
