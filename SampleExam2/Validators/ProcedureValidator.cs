using FluentValidation;
using SampleExam2.DTOs;

namespace SampleExam2.Validators;

public class ProcedureValidator : AbstractValidator<ProcedureDTO>
{
    public ProcedureValidator()
    {
        RuleFor(x => x.ProcedureId).NotEmpty().WithMessage("procedure id must be provided");
        RuleFor(x => x.Date).NotEmpty().WithMessage("date ");
    }
}