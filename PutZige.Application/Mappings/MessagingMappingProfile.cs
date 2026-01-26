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
            .ForMember(d => d.MessageId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.SenderId, opt => opt.MapFrom(s => s.SenderId))
            .ForMember(d => d.ReceiverId, opt => opt.MapFrom(s => s.ReceiverId))
            .ForMember(d => d.MessageText, opt => opt.MapFrom(s => s.MessageText))
            .ForMember(d => d.SentAt, opt => opt.MapFrom(s => s.SentAt));
    }
}
