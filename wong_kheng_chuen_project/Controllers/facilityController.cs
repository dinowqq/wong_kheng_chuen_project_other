using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wong_kheng_chuen_project.data;
using wong_kheng_chuen_project.models;

namespace wong_kheng_chuen_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FacilityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? CurrentUsername => User.FindFirstValue(ClaimTypes.Name);
        private bool IsAdmin => User.IsInRole(UserRoles.Admin);

        private IActionResult? CheckDbSet()
        {
            if (_context.facility == null)
                return StatusCode(500, "Database set 'facility' is not available.");
            return null;
        }

        // Get All - Admin only
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult GetAll()
        {
            var dbCheck = CheckDbSet();
            if (dbCheck != null) return dbCheck;

            return Ok(_context.facility.ToList());
        }

        // Get bookings with sorting
        // Admin = all bookings
        // User = only own bookings
        [HttpGet("bookings")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult GetBookings(string? sortBy = null)
        {
            var username = CurrentUsername;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Username claim not found in token.");

            var dbCheck = CheckDbSet();
            if (dbCheck != null) return dbCheck;

            IQueryable<facility> query = _context.facility;

            if (!IsAdmin)
            {
                query = query.Where(f => f.Booked_By == username);
            }

            query = sortBy?.ToLower() switch
            {
                "dateasc" => query.OrderBy(f => f.Booking_Date_From),
                "datedesc" => query.OrderByDescending(f => f.Booking_Date_From),
                "facilityasc" => query.OrderBy(f => f.Facility_Description),
                "facilitydesc" => query.OrderByDescending(f => f.Facility_Description),
                _ => query.OrderBy(f => f.Booking_ID)
            };

            return Ok(query.ToList());
        }

        // Create - Admin and User
        // Booked_By automatically uses the logged-in username
        // Booking conflict detection included
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult CreateBooking([FromBody] facility facility)
        {
            var dbCheck = CheckDbSet();
            if (dbCheck != null) return dbCheck;

            var username = CurrentUsername;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("User not found.");

            facility.Booked_By = username;

            var newFrom = facility.Booking_Date_From;
            var newTo = facility.Booking_Date_To;

            if (newTo < newFrom)
                return BadRequest("Booking end date cannot be earlier than booking start date.");

            var hasConflict = _context.facility.Any(f =>
                f.Facility_Description == facility.Facility_Description &&
                f.Booking_Date_From <= newTo &&
                f.Booking_Date_To >= newFrom
            );

            if (hasConflict)
                return BadRequest("Booking conflict detected. This facility is already booked for the selected dates.");

            _context.facility.Add(facility);
            _context.SaveChanges();

            return Ok(facility);
        }

        // Get by id - Admin and User (users only their own)
        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult GetById(int id)
        {
            var username = CurrentUsername;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Username claim not found in token.");

            var dbCheck = CheckDbSet();
            if (dbCheck != null) return dbCheck;

            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);
            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            if (!IsAdmin && entity.Booked_By != username)
                return Forbid();

            return Ok(entity);
        }

        // Update - Admin and User
        // User can only update own booking (admin can update any)
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult UpdateBooking(int id, [FromBody] facility facility)
        {
            var dbCheck = CheckDbSet();
            if (dbCheck != null) return dbCheck;

            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            if (!IsAdmin && entity.Booked_By != CurrentUsername)
                return Forbid();

            var newFrom = facility.Booking_Date_From;
            var newTo = facility.Booking_Date_To;

            if (newTo < newFrom)
                return BadRequest("Booking end date cannot be earlier than booking start date.");

            var hasConflict = _context.facility.Any(f =>
                f.Booking_ID != id &&
                f.Facility_Description == facility.Facility_Description &&
                f.Booking_Date_From <= newTo &&
                f.Booking_Date_To >= newFrom
            );

            if (hasConflict)
                return BadRequest("Booking conflict detected. This facility is already booked for the selected dates.");

            entity.Facility_Description = facility.Facility_Description;
            entity.Booking_Date_From = facility.Booking_Date_From;
            entity.Booking_Date_To = facility.Booking_Date_To;
            entity.Booking_Status = facility.Booking_Status;

            _context.SaveChanges();
            return Ok(entity);
        }

        // Delete - Admin and User
        // User can only delete own booking (admin can delete any)
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult DeleteBooking(int id)
        {
            var dbCheck = CheckDbSet();
            if (dbCheck != null) return dbCheck;

            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            if (!IsAdmin && entity.Booked_By != CurrentUsername)
                return Forbid();

            _context.facility.Remove(entity);
            _context.SaveChanges();

            return Ok(entity);
        }
    }
}