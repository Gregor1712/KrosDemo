using AutoMapper;
using KrosDemo.Application.DTOs;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Mapping;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Invoice, InvoiceDTO>();
        CreateMap<InvoiceDTO, Invoice>();

        CreateMap<InvoiceItem, InvoiceItemDTO>();
        CreateMap<InvoiceItemDTO, InvoiceItem>();
    }
}