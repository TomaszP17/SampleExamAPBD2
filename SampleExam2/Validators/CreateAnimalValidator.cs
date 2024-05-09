using FluentValidation;
using SampleExam2.DTOs;

namespace SampleExam2.Validators;

public class CreateAnimalValidator : AbstractValidator<AddAnimalDTO>
{
    public CreateAnimalValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Max lenght for name is 100");
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100).WithMessage("Max length for type is 100");
        RuleFor(x => x.AdmissionDate).NotEmpty().WithMessage("Admission Date must be provided");
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId must be provided");
        RuleForEach(x => x.Procedures).SetValidator(new ProcedureValidator())
            .When(x => x.Procedures != null && x.Procedures.Count > 0);
    }
}