namespace SampleExam2.DTOs;

public record GetAnimalDto(
        int Id,
        string Name,
        string Type,
        DateTime AdmissionDate,
        GetOwner ChOwner,
        GetProcedure Procedure
    );