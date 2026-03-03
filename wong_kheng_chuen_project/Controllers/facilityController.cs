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

        // Get All - Admin only
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult GetAll()
        {
            return Ok(_context.facility.ToList());
        }

        // Get One - Admin and User
        // User can only view their own booking (admin can view any)
        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult GetById(int id)
        {
            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            // If not admin, only allow viewing own record
            if (!IsAdmin && entity.Booked_By != CurrentUsername)
                return Forbid();

            return Ok(entity);
        }

        // Create - Admin and User
        // Booked_By is always set from token username
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult CreateBooking([FromBody] facility facility)
        {
            var username = CurrentUsername;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Username claim not found in token.");

            // Always set Booked_By from logged-in user
            facility.Booked_By = username;

            _context.facility.Add(facility);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = facility.Booking_ID }, facility);
        }

        // Update - Admin and User
        // User can only update own booking (admin can update any)
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult UpdateBooking(int id, [FromBody] facility facility)
        {
            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            // Non-admin cannot update other users' bookings
            if (!IsAdmin && entity.Booked_By != CurrentUsername)
                return Forbid();

            entity.Facility_Description = facility.Facility_Description;
            entity.Booking_Date_From = facility.Booking_Date_From;
            entity.Booking_Date_To = facility.Booking_Date_To;
            entity.Booking_Status = facility.Booking_Status;

            // Prevent changing owner
            // entity.Booked_By stays as-is

            _context.SaveChanges();
            return Ok(entity);
        }

        // Delete - Admin and User
        // User can only delete own booking (admin can delete any)
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult DeleteBooking(int id)
        {
            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            // Non-admin cannot delete other users' bookings
            if (!IsAdmin && entity.Booked_By != CurrentUsername)
                return Forbid();

            _context.facility.Remove(entity);
            _context.SaveChanges();

            return Ok(entity);
        }
    }
}