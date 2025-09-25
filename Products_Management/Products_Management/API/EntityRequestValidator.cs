using FluentValidation;

namespace Products_Management.API
{
    public class EntityRequestValidator : AbstractValidator<EntityRequest>
    {
        public EntityRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.ImageUrl)
                .Must(file => file == null || file.Length > 0)
                .WithMessage("Invalid image file");
        }
    }
}