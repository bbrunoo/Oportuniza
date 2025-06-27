﻿using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class Certification
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? FileUrl { get; set; }
        public Guid CurriculumId { get; set; }
        [JsonIgnore]
        public Curriculum Curriculum { get; set; }
    }
}
