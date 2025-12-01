using AutoMapper;
using DShop2024.Models.Blog;
using DShop2024.ViewModels;

namespace DShop2024.AutoMapper
{
    public class PostMapper : Profile
    {
        public PostMapper()
        {
            CreateMap<PostModel, PostViewModel>()
                .ForMember(pv => pv.Author, p => p.MapFrom(p => p.CreateBy))
                .ForMember(pv => pv.Author, p => p.MapFrom(p => p.CreateBy))
                .ForMember(pv => pv.CountComments, p => p.MapFrom(p => p.Comments.Where(c => c.Status != 0).Count()))
                .ForMember(pv => pv.CountLikes, p => p.MapFrom(p => p.Likes.Count()  ))
                .ForMember(pv => pv.Subjects, p => p.MapFrom(p => string.Join(", ", p.PostSubjects.Select(pc => pc.Subject.Title))));

        }
    }
}
