using AutoMapper;
using DatingApp.Dtos;
using DatingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>().ForMember(dest => dest.photoUrl,opt => {
                opt.MapFrom(src => src.photos.FirstOrDefault(p => p.IsMain).Url);
            }).ForMember(dest =>dest.age, opt => {
                opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
                });
            CreateMap<User, UserForDetailedDto>().ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.photos.FirstOrDefault(p => p.IsMain).Url);
            }).ForMember(dest => dest.age, opt => {
                opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
            }); ;
            CreateMap<Photo, PhotosForDetailedDto>();
            CreateMap<UserForUpdateDto,User>();
            CreateMap<Photo, photoForReturnDto>();
            CreateMap<photoForCreationDto, Photo>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, opt => opt
                  .MapFrom(u => u.Sender.photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, opt => opt
                  .MapFrom(u => u.Recipient.photos.FirstOrDefault(p => p.IsMain).Url));
        }
    }
}
