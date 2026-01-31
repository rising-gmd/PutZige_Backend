using AutoMapper;
using PutZige.Domain.Entities;
using PutZige.Application.DTOs.Messaging;

namespace PutZige.Application.Mappings;

public class MessagingMappingProfile : Profile
{
    public MessagingMappingProfile()
    {
        CreateMap<Message, MessageDto>()
            .ForMember(d => d.SenderUsername, opt => opt.MapFrom(s => s.Sender != null ? s.Sender.Username : string.Empty))
            .ForMember(d => d.ReceiverUsername, opt => opt.MapFrom(s => s.Receiver != null ? s.Receiver.Username : string.Empty));

        CreateMap<Message, SendMessageResponse>()
            .ConstructUsing(s => new SendMessageResponse(s.Id, s.SenderId, s.ReceiverId, s.MessageText, s.SentAt));
    }
}
