using System.Data;
using Microsoft.Data.SqlClient;
using Tutorial7.DTOs;

namespace Tutorial7.Services;

public class AppointmentsService : IAppointmentsService
{
    private readonly string _connectionString;

    public AppointmentsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public async Task<IEnumerable<AppointmentListDto>> GetAllAppointmentsAsync(string? status,string? patientLastName)
    {
        var query = "SELECT IdAppointment, Status FROM Appointments";
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(@"SELECT a.IdAppointment,a.AppointmentDate,a.Status,a.Reason,p.FirstName+' '+p.LastName as PatientFullName,p.Email as PatientEmail 
FROM Appointments a join Patients p on p.IdPatient=a.IdPatient where (@Status is NULL OR a.Status=@Status) AND (@PatientLastName is null  or p.LastName=@PatientLastName) order by a.AppointmentDate", connection);
        command.Parameters.Add("@Status", SqlDbType.NVarChar).Value=(object?) status??DBNull.Value;
        command.Parameters.Add("@PatientLastName", SqlDbType.NVarChar).Value=(object?) patientLastName??DBNull.Value;
        // command.Connection = connection;
        // command.CommandText = query;

        await using var reader = await command.ExecuteReaderAsync();
        
        var appointments = new List<AppointmentListDto>();
        while (await reader.ReadAsync())
        {
            var appointment = new AppointmentListDto()
            {
                IdAppointment = reader.GetInt32(0),
                AppointmentDate = reader.GetDateTime(1),
                Status =  reader.GetString(2),
                Reason = reader.GetString(3),
                PatientFullName = reader.GetString(4),
                PatientEmail = reader.GetString(5),
            };
            appointments.Add(appointment);
        }
        
        return appointments;
    }
}