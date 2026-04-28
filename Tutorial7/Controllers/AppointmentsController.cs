using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial7.DTOs;
using Tutorial7.Services;

namespace Tutorial7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsService _appointmentsService;

        public AppointmentsController(IAppointmentsService appointmentsService)
        {
            _appointmentsService = appointmentsService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? status,[FromQuery]string? patientLastName)
        {
            var appointments = await _appointmentsService.GetAllAppointmentsAsync(status, patientLastName);
            return Ok(appointments);
        }

        [HttpGet("{idAppointment}")]
        public async Task<IActionResult> GetByIdAppointment(int idAppointment)
        {
            var appointment=await _appointmentsService.GetAppointmentByIdAsync(idAppointment);
            if(appointment is null){
                return NotFound();
            }
            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(CreateAppointmentRequestDto createAppointmentRequestDto)
        {
            try
            {
                var id=await _appointmentsService.CreateAppointmentAsync(createAppointmentRequestDto);
                return Created($"api/Appointments/{id}",new{id});
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpPut("{idAppointment}")]
        public async Task<IActionResult> UpdateAppointment(int idAppointment,
            UpdateAppointmentRequestDto updateAppointmentRequestDto)
        {
            try
            {
            var updateAppointmentRequest=await _appointmentsService.UpdateAppointmentAsync(idAppointment,updateAppointmentRequestDto);
            if(!updateAppointmentRequest)
            {
                return NotFound();
            }
            return Ok();
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpDelete("{idAppointment}")]
        public async Task<IActionResult> DeleteAppointment(int idAppointment)
        {
            try
            {
            var deleteAppointmentRequest=await _appointmentsService.DeleteAppointmentAsync(idAppointment);
            if (!deleteAppointmentRequest)
            {
                return NotFound();
            }
            return NoContent();
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }
    }
}
