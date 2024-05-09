using System.Data;
using System.Data.SqlClient;
using SampleExam2.DTOs;

namespace SampleExam2.Services;

public interface IDbService
{
    Task<GetAnimalDto?> GetAnimalByIdAsync(int id);
    Task<bool> IsOwnerExists(int id);
    Task<bool> IsProcedureExists(int id);
    
    Task AddAnimalAsync(AddAnimalDTO animalDto);
}

public class DbService(IConfiguration configuration) : IDbService
{
    private async Task<SqlConnection> GetConnection()
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
                                   p.Name, p.Description, pa.Date
                            FROM Animal a
                            INNER JOIN Owner o ON a.Owner_ID = o.ID
                            LEFT JOIN Procedure_Animal pa ON a.ID = pa.Animal_ID
                            LEFT JOIN [Procedure] p ON pa.Procedure_ID = p.ID
                            WHERE a.ID = @id;
                        ";
    command.Parameters.AddWithValue("@id", id);
    var reader = await command.ExecuteReaderAsync();

    if (!reader.HasRows)
    {
        return null;
    }

    GetAnimalDto animal = null;
    var procedures = new List<GetProcedure>();
    
    while (await reader.ReadAsync())
    {
        if (animal == null)
        {
            animal = new GetAnimalDto(
                reader.GetInt32(0),  // Animal ID
                reader.GetString(1), // Animal Name
                reader.GetString(2), // Animal Type
                reader.GetDateTime(3), // AdmissionDate
                new GetOwner(reader.GetInt32(4), reader.GetString(5), reader.GetString(6)), // Owner
                new List<GetProcedure>() // Initialize with an empty list
            );
        }
        
        if (!reader.IsDBNull(7)) // Check if Procedure Name is not null
        {
            procedures.Add(new GetProcedure(
                reader.GetString(7), // Procedure Name
                reader.GetString(8), // Procedure Description
                reader.GetDateTime(9) // Procedure Date
            ));
        }
    }

    // Now that the whole animal record has been read, assign the procedures if any
    animal = animal with { Procedures = procedures };  // Using the 'with' expression to create a new record with updated procedures

    return animal;
}

    public async Task<bool> IsOwnerExists(int id)
    {
        await using var connection = await GetConnection();
        var command = new SqlCommand("SELECT * FROM OWNER WHERE @id = ID", connection);
        command.Parameters.AddWithValue("@id", id);
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<bool> IsProcedureExists(int id)
    {
        await using var connection = await GetConnection();
        var command = new SqlCommand("Select * From [Procedure] WHERE @id = ID", connection);
        command.Parameters.AddWithValue("@id", id);
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    public async Task AddAnimalAsync(AddAnimalDTO animalDto)
    {
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            
            var animalCommand = new SqlCommand("INSERT INTO Animal (Name, Type, AdmissionDate, Owner_ID) " +
                                               "VALUES (@Name, @Type, @AdmissionDate, @Owner_ID); SELECT SCOPE_IDENTITY();");
            animalCommand.Connection = connection;
            animalCommand.Transaction = (SqlTransaction)transaction;
            animalCommand.Parameters.AddWithValue("@Name", animalDto.Name);
            animalCommand.Parameters.AddWithValue("@Type", animalDto.Type);
            animalCommand.Parameters.AddWithValue("@AdmissionDate", animalDto.AdmissionDate);
            animalCommand.Parameters.AddWithValue("@Owner_ID", animalDto.OwnerId);

            var animalId = (decimal)await animalCommand.ExecuteScalarAsync();

            foreach (var procedure in animalDto.Procedures ?? Enumerable.Empty<ProcedureDTO>())
            {
                var procedureAnimalCommand = new SqlCommand("INSERT INTO Procedure_Animal (Procedure_ID, Animal_ID, Date)" +
                                                            " VALUES (@Procedure_ID, @Animal_ID, @Date);");
                procedureAnimalCommand.Connection = connection;
                procedureAnimalCommand.Transaction = (SqlTransaction)transaction;
                procedureAnimalCommand.Parameters.AddWithValue("@Procedure_ID", procedure.ProcedureId);
                procedureAnimalCommand.Parameters.AddWithValue("@Animal_ID", (int)animalId);
                procedureAnimalCommand.Parameters.AddWithValue("@Date", procedure.Date);

                await procedureAnimalCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
        }
    }
}