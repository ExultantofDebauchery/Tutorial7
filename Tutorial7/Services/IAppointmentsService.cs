using Tutorial7.DTOs;

namespace Tutorial7.Services;

public interface IAppointmentsService
{
    Task<IEnumerable<AppointmentListDto>> GetAllAppointmentsAsync(string? status,string? patientLastName);
    Task <AppointmentDetailsDto> GetAppointmentByIdAsync(int idAppointment);
    Task<int> CreateAppointmentAsync(CreateAppointmentRequestDto createAppointmentRequestDto);
    Task<bool> UpdateAppointmentAsync(int idAppointment,UpdateAppointmentRequestDto updateAppointmentRequestDto);
    Task<bool> DeleteAppointmentAsync(int idAppointment);
}