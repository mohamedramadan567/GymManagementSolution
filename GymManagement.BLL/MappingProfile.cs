using AutoMapper;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.BLL.ViewModels.PlanViewModels;
using GymManagement.BLL.ViewModels.SessionViewModels;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using GymManagement.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            MemberMap();

            TrainerMap();

            PlaMap();

            SessionMap();
        }

        private void MemberMap()
        {
            #region Member

            CreateMap<Member, MemberViewModel>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => $"{src.Address.BuildingNumber} - {src.Address.Street} - {src.Address.City}"))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ToShortDateString()));

            CreateMap<HealthRecord, HealthRecordViewModel>().ReverseMap();

            CreateMap<Member, MemberToUpdateViewModel>()
                .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(scr => scr.Address.BuildingNumber))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(scr => scr.Address.Street))
                .ForMember(dest => dest.City, opt => opt.MapFrom(scr => scr.Address.City));

            CreateMap<MemberToUpdateViewModel, Member>()
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.Photo, opt => opt.Ignore())
                //.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src));
                .AfterMap((src, dest) =>
                {
                    dest.Address.BuildingNumber = src.BuildingNumber;
                    dest.Address.Street = src.Street;
                    dest.Address.City = src.City;
                });

            CreateMap<CreateMemberViewModel, Member>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address()
                {
                    BuildingNumber = src.BuildingNumber,
                    Street = src.Street,
                    City = src.City
                }))
                .ForMember(dest => dest.HealthRecord, opt => opt.MapFrom(src => src.HealthRecordViewModel));

            #endregion
        }

        private void TrainerMap()
        {
            #region Trainer

            CreateMap<Trainer, TrainerViewModel>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => $"{src.Address.BuildingNumber} - {src.Address.Street} - {src.Address.City}"));

            CreateMap<Trainer, TrainerToUpdateViewModel>()
                .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(scr => scr.Address.BuildingNumber))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(scr => scr.Address.Street))
                .ForMember(dest => dest.City, opt => opt.MapFrom(scr => scr.Address.City));

            CreateMap<TrainerToUpdateViewModel, Trainer>()
               .ForMember(dest => dest.Name, opt => opt.Ignore())
               .AfterMap((src, dest) =>
               {
                   dest.Address.BuildingNumber = src.BuildingNumber;
                   dest.Address.Street = src.Street;
                   dest.Address.City = src.City;
               });

            CreateMap<CreateTrainerViewModel, Trainer>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address()
                {
                    BuildingNumber = src.BuildingNumber,
                    Street = src.Street,
                    City = src.City
                }));

            #endregion
        }

        private void PlaMap()
        {
            #region Plan

            CreateMap<Plan, PlanViewModel>();

            CreateMap<Plan, PlanToUpdateViewModel>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Name))
                .ReverseMap()
                .ForMember(dest => dest.Name, opt => opt.Ignore());

            #endregion
        }

        private void SessionMap()
        {
            CreateMap<Session, SessionViewModel>()
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.Trianer.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName));

            CreateMap<CreateSessionViewModel, Session>();

            CreateMap<Category, CategorySelectViewModel>();
            CreateMap<Trainer, TrainerSelectViewModel>();
        }
    }

    
}
