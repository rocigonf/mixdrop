using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Helper;
using mixdrop_back.Models.Mappers;

namespace mixdrop_back.Services;

public class UserService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly TokenValidationParameters _tokenParameters;
    private readonly UserMapper _userMapper;

    public UserService(UnitOfWork unitOfWork, IOptionsMonitor<JwtBearerOptions> jwtOptions, UserMapper userMapper)
    {
        _unitOfWork = unitOfWork;
        _userMapper = userMapper;
        _tokenParameters = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme)
                .TokenValidationParameters;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        return _userMapper.ToDto(users).ToList();
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return null;
        }
        return _userMapper.ToDto(user);
    }

    public async Task<UserDto> GetUserByNicknameAsync(string nickname)
    {
        var user = await _unitOfWork.UserRepository.GetByNicknameAsync(nickname);
        if (user == null)
        {
            return null;
        }
        return _userMapper.ToDto(user);
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _unitOfWork.UserRepository.GetByIdAsync(id);
    }


    // REGISTRO E INICIO DE SESION
    public async Task<User> LoginAsync(LoginRequest loginRequest)
    {
        var user = await _unitOfWork.UserRepository.GetByEmailOrNickname(loginRequest.EmailOrNickname);

        if (user == null || user.Password != PasswordHelper.Hash(loginRequest.Password))
        {
            return null;
        }

        user.StateId = 2; // conectado

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveAsync();

        return user;
    }

    public async Task<User> RegisterAsync(RegisterDto model)
    {

        try
        {

            // Verifica si el usuario ya existe
            var existingUser = await GetUserByEmailAsync(model.Email);

            if (existingUser != null)
            {
                throw new Exception("El usuario ya existe.");
            }

            ImageService imageService = new ImageService();

            var newUser = new User
            {
                Email = model.Email,
                Nickname = model.Nickname,
                AvatarPath = "/" + await imageService.InsertAsync(model.Image),
                Role = "User", // Rol por defecto
                Password = PasswordHelper.Hash(model.Password),
                IsInQueue = false,  // por defecto al crearse
                StateId = 1
            };

            await _unitOfWork.UserRepository.InsertAsync(newUser);
            await _unitOfWork.SaveAsync();

            return newUser;

        }
        catch (DbUpdateException ex)
        {
            // Log más detallado del error
            Console.WriteLine($"Error al guardar el usuario: {ex.InnerException?.Message}");
            throw new Exception("Error al registrar el usuario. Verifica los datos ingresados.");
        }
    }

}
