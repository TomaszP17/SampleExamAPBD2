namespace SampleExam2.DTOs;

public record AddAnimalDTO(
    int Id,
    string Name,
    string Type,
    DateTime AdmissionDate,
    int OwnerId
    //tutaj jakos liste tych wszystkich procedur
    
    );