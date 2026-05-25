using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Users;

namespace MedHub.Application.Users.RegisterTeacher;

internal sealed class RegisterTeacherCommandHandler : ICommandHandler<RegisterTeacherCommand, Guid>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITeacherRegistrationCodeValidator _teacherRegistrationCodeValidator;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterTeacherCommandHandler(
        IAuthenticationService authenticationService,
        ITeacherRegistrationCodeValidator teacherRegistrationCodeValidator,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _authenticationService = authenticationService;
        _teacherRegistrationCodeValidator = teacherRegistrationCodeValidator;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        RegisterTeacherCommand request,
        CancellationToken cancellationToken)
    {
        if (!_teacherRegistrationCodeValidator.IsValid(request.TeacherRegistrationCode))
        {
            return Result.Failure<Guid>(UserErrors.InvalidTeacherRegistrationCode);
        }

        var user = User.CreateTeacher(
            new FirstName(request.FirstName),
            new LastName(request.LastName),
            new Email(request.Email));

        var identityId = await _authenticationService.RegisterAsync(
            user,
            request.Password,
            cancellationToken);

        user.SetIdentityId(identityId);

        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync();

        return user.Id;
    }
}
