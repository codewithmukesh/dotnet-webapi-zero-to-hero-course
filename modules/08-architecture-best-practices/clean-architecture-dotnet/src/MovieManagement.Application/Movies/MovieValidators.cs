using FluentValidation;

namespace MovieManagement.Application.Movies;

// Input validation lives in the Application layer: it checks the SHAPE of the
// request (required fields, lengths, ranges) before it reaches the domain. The
// domain still enforces its own invariants - this just catches bad input early
// and turns it into a clean, field-level 400 instead of a 500.
internal sealed class CreateMovieRequestValidator : AbstractValidator<CreateMovieRequest>
{
    public CreateMovieRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Director).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReleaseDate).NotEqual(default(DateOnly)).WithMessage("Release date is required.");
        RuleFor(x => x.Genre).IsInEnum().WithMessage("Genre must be one of the supported values.");
        RuleFor(x => x.Synopsis).MaximumLength(2000);
    }
}

internal sealed class UpdateMovieRequestValidator : AbstractValidator<UpdateMovieRequest>
{
    public UpdateMovieRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Director).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReleaseDate).NotEqual(default(DateOnly)).WithMessage("Release date is required.");
        RuleFor(x => x.Genre).IsInEnum().WithMessage("Genre must be one of the supported values.");
        RuleFor(x => x.Synopsis).MaximumLength(2000);
    }
}

internal sealed class AddRatingRequestValidator : AbstractValidator<AddRatingRequest>
{
    public AddRatingRequestValidator()
    {
        RuleFor(x => x.Score).InclusiveBetween(1, 10);
    }
}
