namespace SampleExam2.DTOs;

public record AddAnimalDTO(
    string Name,
    string Type,
    DateTime AdmissionDate,
    int OwnerId,
    List<ProcedureDTO>? Procedures = null
    );
    
public record ProcedureDTO(
    int ProcedureId,
    DateTime Date
    );