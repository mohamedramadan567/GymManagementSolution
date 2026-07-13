using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.BookingViewModels;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using GymManagement.BLL.ViewModels.SessionViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SessionViewModel>> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var sessions = await _unitOfWork.SessionRepository.GetAllSessionsWithTrainerAndCategoryAsync(s => s.EndDate > DateTime.Now, ct);
            var mappedSessions = _mapper.Map<IEnumerable<SessionViewModel>>(sessions);
            foreach (var session in mappedSessions)
            {
                session.AvailableSlots = session.Capacity - await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(session.Id, ct);
            }
            return mappedSessions;
        }

        public async Task<IEnumerable<MemberForSessionViewModel>> GetMembersForSession(int sessionId, CancellationToken ct = default)
        {
            var bookings = await _unitOfWork.BookingRepository.GetBySessionId(sessionId, ct);
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(sessionId, ct);
            return bookings.Select(b => new MemberForSessionViewModel
            {
                MemberId = b.MemberId,
                SessionId = b.SessionId,
                Date = b.CreatedAt,
                MemberName = b.Member.Name,
                IsAttended = session?.StartDate > DateTime.Now ? false : b.IsAttended
            }).ToList();
        }

        public async Task<IEnumerable<MemberSelectViewModel>> GetMembersForDropDown(int sessionId, CancellationToken ct = default)
        {
            var bookings = await _unitOfWork.BookingRepository.GetAllAsync(b => b.SessionId == sessionId, ct: ct);

            var bookedMemberIds = bookings.Select(b => b.MemberId);

            var availableMembers = await _unitOfWork.GetRepository<Member>().GetAllAsync(m => !bookedMemberIds.Contains(m.Id), ct:ct);

            return _mapper.Map<IEnumerable<MemberSelectViewModel>>(availableMembers);
        }

        public async Task<Result> CreateBooking(CreateBookingViewModel model, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(model.SessionId, ct);

            if (session is null)
                return Result.NotFound("Session is not found");

            if (session.StartDate <= DateTime.Now)
                return Result.Fail("You can't book a session that already started");

            var membership = await _unitOfWork.MembershipRepository.AnyAsync(m => m.MemberId == model.MemberId && m.EndDate > DateTime.Now);

            if (!membership)
                return Result.Fail("You don't have active membership");

            var alreadyBooked = await _unitOfWork.BookingRepository.AnyAsync(b => b.MemberId == model.MemberId && b.SessionId == model.SessionId);

            if (alreadyBooked)
                return Result.Fail("You already booked this session");

            var bookedSlots = await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(model.SessionId, ct);

            if (bookedSlots >= session.Capacity)
                return Result.Fail("Session is full capacity");

            var booking = new Booking
            {
                MemberId = model.MemberId,
                SessionId = model.SessionId,
                IsAttended = false,
                CreatedAt = DateTime.Now
            };

            _unitOfWork.BookingRepository.Add(booking);

            return await _unitOfWork.SaveChangesAsync() > 0
                ? Result.OK()
                : Result.Fail("Failed to book this session");
        }

        public async Task<Result> MarkAttendedAsync(int memberId, int sessionId, CancellationToken ct = default)
        {
            var booking = await _unitOfWork.BookingRepository.FirstOrDefaultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, true, ct);

            if (booking == null)
                return Result.Fail("Booking is Not Found");

            booking.IsAttended = true;
            booking.UpdatedAt = DateTime.Now;

            _unitOfWork.BookingRepository.Update(booking);

            return await _unitOfWork.SaveChangesAsync(ct) > 0 ? Result.OK() : Result.Fail("Failed to mark this member as attended.");
        }

        public async Task<Result> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(sessionId, ct);
            if (session is null)
                return Result.Fail("Session Not Found");

            if (session.StartDate <= DateTime.Now)
                return Result.Fail("Can't cancel booking for a session has already started");

            var booking = await _unitOfWork.BookingRepository.FirstOrDefaultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, true, ct);

            if (booking == null)
                return Result.Fail("Booking is Not Found");


            _unitOfWork.BookingRepository.Delete(booking);

            return await _unitOfWork.SaveChangesAsync(ct) > 0 ? Result.OK() : Result.Fail("Failed to mark this member as attended.");

        }

    }
}
