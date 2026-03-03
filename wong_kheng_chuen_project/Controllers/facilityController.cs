using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wong_kheng_chuen_project.data;
using wong_kheng_chuen_project.models;

namespace wong_kheng_chuen_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // all endpoints require JWT unless overridden
    public class FacilityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get All - Admin only
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult GetAll()
        {
            return Ok(_context.facility.ToList());
        }

        // Get One - Admin and User
        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult GetById(int id)
        {
            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            return Ok(entity);
        }

        // Create - Admin and User
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult CreateBooking([FromBody] facility facility)
        {
            _context.facility.Add(facility);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = facility.Booking_ID }, facility);
        }

        // Update - Admin and User
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult UpdateBooking(int id, [FromBody] facility facility)
        {
            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            entity.Facility_Description = facility.Facility_Description;
            entity.Booking_Date_From = facility.Booking_Date_From;
            entity.Booking_Date_To = facility.Booking_Date_To;
            entity.Booked_By = facility.Booked_By;
            entity.Booking_Status = facility.Booking_Status;

            _context.SaveChanges();
            return Ok(entity);
        }

        // Delete - Admin and User
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public IActionResult DeleteBooking(int id)
        {
            var entity = _context.facility.FirstOrDefault(f => f.Booking_ID == id);

            if (entity == null)
                return NotFound($"Booking with id {id} not found");

            _context.facility.Remove(entity);
            _context.SaveChanges();

            return Ok(entity);
        }
    }
}