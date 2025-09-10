using Oportuniza.Domain.Models;

namespace Oportuniza.API.Viewmodel
{
    public class PublicationWithCandidates
    {
        public Guid PublicationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public IEnumerable<CandidateApplication> Candidates { get; set; } = new List<CandidateApplication>();
    }
}
