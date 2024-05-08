using System.Data;
using System.Data.SqlClient;
using SampleExam2.DTOs;

namespace SampleExam2.Services;

public interface IDbService
{
    Task<GetAnimalDto?> GetAnimalByIdAsync(int id);
}

public class DbService(IConfiguration configuration) : IDbService
{
    public async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }


    public async Task<GetAnimalDto?> GetAnimalByIdAsync(int id)
    {
        await using var connection = await GetConnection();
        var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = @"
                                SELECT a.ID, a.Name, a.Type, a.AdmissionDate, o.ID, o.FirstName, o.LastName,
                                       [p].Name, [p].Description, pa.Date
                                FROM Animal a
                                INNER JOIN Owner o ON a.Owner_ID = o.ID
                                INNER JOIN Procedure_Animal pa ON a.ID = pa.Animal_ID
                                INNER JOIN [Procedure] [p] ON pa.Procedure_ID = [p].ID
                                WHERE a.ID = @id;
                            ";
        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
        {
            return null;
        }

        await reader.ReadAsync();

        var result = new GetAnimalDto(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDateTime(3),
            new GetOwner(reader.GetInt32(4), reader.GetString(5), reader.GetString(6)),
            new GetProcedure(reader.GetString(7), reader.GetString(8), reader.GetDateTime(9))
            );

        return result;
    }
}