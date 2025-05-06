using AutoMapper;
using JaCore.Api.DTOs.Device;
using JaCore.Api.Entities.Device;

namespace JaCore.Api.Mappings.Device;

public class DeviceProfile : Profile
{
    public DeviceProfile()
    {
        // --- Device Mappings ---
        CreateMap<DeviceCreateDto, Entities.Device.Device>();
        CreateMap<DeviceUpdateDto, Entities.Device.Device>();
        CreateMap<Entities.Device.Device, DeviceDto>();

        // --- DeviceCard Mappings ---
        CreateMap<DeviceCardCreateDto, DeviceCard>();
        CreateMap<DeviceCardUpdateDto, DeviceCard>();
        CreateMap<DeviceCard, DeviceCardReadDto>();

        // --- Event Mappings ---
        CreateMap<EventCreateDto, Event>();
        // No Update DTO assumed for Event
        CreateMap<Event, EventReadDto>();

        // --- DeviceOperation Mappings ---
        CreateMap<DeviceOperationCreateDto, DeviceOperation>();
        CreateMap<DeviceOperationUpdateDto, DeviceOperation>();
        CreateMap<DeviceOperation, DeviceOperationReadDto>();

        // --- Category Mappings ---
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();
        CreateMap<Category, CategoryReadDto>();

        // --- Supplier Mappings ---
        CreateMap<SupplierCreateDto, Supplier>();
        CreateMap<SupplierUpdateDto, Supplier>();
        CreateMap<Supplier, SupplierReadDto>();

        // --- Service Mappings ---
        CreateMap<ServiceCreateDto, Service>();
        CreateMap<ServiceUpdateDto, Service>();
        CreateMap<Service, ServiceReadDto>();

        // Add any specific .ForMember configurations if needed (e.g., ignoring navigation properties)
    }
} 