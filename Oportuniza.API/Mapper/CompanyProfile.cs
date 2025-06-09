using AutoMapper;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile() {
            CreateMap<CompanyDTO, Company>();
            CreateMap<CompanyByIdDTO, Company>();
            CreateMap<CompanyInfoDTO, Company>();
            CreateMap<AllCompanyInfoDTO, Company>();

            CreateMap<Company, CompanyDTO>();
            CreateMap<Company, CompanyByIdDTO>();
            CreateMap<Company, CompanyInfoDTO>();
            CreateMap<Company, AllCompanyInfoDTO>();
        }
    }
}
