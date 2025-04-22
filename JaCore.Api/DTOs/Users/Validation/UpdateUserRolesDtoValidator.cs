using FluentValidation;
using JaCore.Api.DTOs.Users;

namespace JaCore.Api.DTOs.Users.Validation;

public class UpdateUserRolesDtoValidator : AbstractValidator<UpdateUserRolesDto>
{
    public UpdateUserRolesDtoValidator()
    {
        RuleFor(x => x.Roles)
            .NotNull().WithMessage("Roles list cannot be null."); // Can be empty, but not null
    }
}