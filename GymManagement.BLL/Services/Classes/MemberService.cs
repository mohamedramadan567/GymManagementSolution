using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IGenericRepository<Member> _memberRepository;
        private readonly IGenericRepository<MemberShip> _membershipRepository;
        private readonly IGenericRepository<Plan> _planRepository;
        private readonly IGenericRepository<HealthRecord> _healthRecordRepository;

        public MemberService(IGenericRepository<Member> memberRepository,
                             IGenericRepository<MemberShip> membershipRepository,
                             IGenericRepository<Plan> planRepository,
                             IGenericRepository<HealthRecord> healthRecordRepository)
        {
            _memberRepository = memberRepository;
            _membershipRepository = membershipRepository;
            _planRepository = planRepository;
            _healthRecordRepository = healthRecordRepository;
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            //Check Email
            var emailExit = await _memberRepository.AnyAsync(x => x.Email == model.Email, ct);
            //Check Phone
            var phoneExit = await _memberRepository.AnyAsync(x => x.Phone == model.Phone, ct);
            //if Email or Phone Exists return false
            if (emailExit || phoneExit) return false;
            //else add member
            var member = new Member()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = new Address()
                {
                    BuildingNumber = model.BuildingNumber,
                    City = model.City,
                    Street = model.Street
                },
                HealthRecord = new HealthRecord()
                {
                    BloodType = model.HealthRecordViewModel.BloodType,
                    Weight = model.HealthRecordViewModel.Weight,
                    Height = model.HealthRecordViewModel.Height,
                    Note = model.HealthRecordViewModel.Note
                }
            };


            var added = await _memberRepository.AddAsync(member);
            return added > 0;
        }


        public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct)
        {
            var members = await _memberRepository.GetAllAsync(ct: ct);

            if (!members.Any()) return [];

            var memberViewModel = members.Select(m => new MemberViewModel()
            {
                Id = m.Id,
                Photo = m.Photo,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Gender = m.Gender.ToString()
            });

            return memberViewModel;
        }

        public async Task<MemberViewModel?> GetMemberDetailsByIdAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _memberRepository.GetByIdAsync(memberId, ct);

            if (member == null) return null;

            var model = new MemberViewModel()
            {
                Name = member.Name,
                Email = member.Email,
                Phone = member.Phone,
                Gender = member.Gender.ToString(),
                DateOfBirth = member.DateOfBirth.ToShortDateString(),
                Address = $"{member.Address.BuildingNumber} - {member.Address.Street} - {member.Address.City}",
            };

            var activeMembership = await _membershipRepository.FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate > DateTime.Now);

            if(activeMembership is not null)
            {
                var activePlan = await _planRepository.GetByIdAsync(activeMembership.PlanId, ct);
                model.PlanName = activePlan?.Name;
                model.MembershipStartDate = activeMembership.CreatedAt.ToString();
                model.MembershipEndDate = activeMembership.EndDate.ToString();
            }

            return model;

        }

        public async Task<HealthRecordViewModel?> GetMemberHealthRecordByIdAsync(int memberId, CancellationToken ct = default)
        {
            var healthRecord = await _healthRecordRepository.FirstOrDefaultAsync(x => x.MemberId == memberId, ct: ct);
            if (healthRecord is null) return null;
            return new HealthRecordViewModel()
            {
                Height = healthRecord.Height,
                Weight = healthRecord.Weight,
                BloodType = healthRecord.BloodType,
                Note = healthRecord.Note
            };
        }
    }
}
