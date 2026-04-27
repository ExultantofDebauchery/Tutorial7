using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        // var query = "SELECT IdAppointment, Status FROM Appointments";
        
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

    public async Task<AppointmentDetailsDto?> GetAppointmentByIdAsync(int idAppointment)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            @"SELECT a.IdAppointment,a.AppointmentDate,a.Status,a.Reason,a.InternalNotes,p.FirstName+' '+p.LastName as PatientFullName,p.Email as PatientEmail
,d.FirstName+' '+d.LastName as DoctorFullName,d.LicenseNumber
FROM Appointments a join Patients p on p.IdPatient=a.IdPatient join Doctors d on d.IdDoctor=a.IdDoctor where a.IdAppointment=@IdAppointment",connection);
        command.Parameters.AddWithValue("@idAppointment", idAppointment);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new AppointmentDetailsDto
        {
            IdAppointment = reader.GetInt32(0),
            AppointmentDate = reader.GetDateTime(1),
            Status = reader.GetString(2),
            Reason = reader.GetString(3),
            InternalNotes = reader.IsDBNull(4) ? null : reader.GetString(4),
            PatientFullName = reader.GetString(5),
            PatientEmail = reader.GetString(6),
            DoctorFullName = reader.GetString(7),
            DoctorLicenseNumber = reader.GetString(8),

        };
    }

    public async Task<int> CreateAppointmentAsync(CreateAppointmentRequestDto request)
    {
        if (request.AppointmentDate < DateTime.Now)
        {
            throw new Exception("River of time flows only forward.Appointment cannot set in past");
        }
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using (var command =
                     new SqlCommand(
                         @"Select count(*) from Appointments where IdDoctor=@IdDoctor and AppointmentDate=@Date",
                         connection))
        {
            command.Parameters.AddWithValue("@IdDoctor", request.IdDoctor);
            command.Parameters.AddWithValue("@Date", request.AppointmentDate);
            var count=(int)await  command.ExecuteScalarAsync();
            if (count > 0)
            {
                throw new Exception("Doctor is busy at this time.");
            }
        }

        await using var command2 = new SqlCommand(
            @"Insert into Appointments (IdPatient,IdDoctor,AppointmentDate,Reason,Status)
Values (@IdPatient,@IdDoctor,@Date,@Reason,'Scheduled');SELECT SCOPE_IDENTITY()",connection);
        command2.Parameters.AddWithValue("@IdPatient", request.IdPatient);
        command2.Parameters.AddWithValue("@IdDoctor", request.IdDoctor);
        command2.Parameters.AddWithValue("@Date", request.AppointmentDate);
        command2.Parameters.AddWithValue("@Reason",request.Reason);
        var id=Convert.ToInt32(await command2.ExecuteScalarAsync());
        return id;
    }
}