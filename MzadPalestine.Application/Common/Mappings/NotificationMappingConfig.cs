using Mapster;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Core.Entities;

namespace MzadPalestine.Application.Common.Mappings;

public class NotificationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Notification, NotificationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Message, src => src.Message)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.IsRead, src => src.IsRead)
            .Map(dest => dest.ReadAt, src => src.ReadAt)
            .Map(dest => dest.ActionUrl, src => src.ActionUrl)
            .Map(dest => dest.ImageUrl, src => src.ImageUrl)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}
