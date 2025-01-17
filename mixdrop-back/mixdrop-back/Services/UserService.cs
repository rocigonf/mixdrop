using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.Text.RegularExpressions;
using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Helper;
using mixdrop_back.Models.Mappers;
using System.Text;

namespace mixdrop_back.Services;

public class UserService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly TokenValidationParameters _tokenParameters;
    private readonly UserMapper _userMapper;

    Regex emailRegex = new(@"^([\w\.-]+)@([\w-]+)((\.(\w){2,3})+)$");

    public UserService(UnitOfWork unitOfWork, IOptionsMonitor<JwtBearerOptions> jwtOptions, UserMapper userMapper)
    {
        _unitOfWork = unitOfWork;
        _userMapper = userMapper;
        _tokenParameters = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme)
                .TokenValidationParameters;
    }


    // buscar usuario
    public async Task<List<UserDto>> SearchUser( string search)
    {

        string searchSinTildes = Regex.Replace(search.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");

        var users = await _unitOfWork.UserRepository.SearchUser(searchSinTildes.ToLower());

        return _userMapper.ToDto(users).ToList();
    }



    // desconectar usuario
    public async Task DisconnectUser(int userId)
    {
        if (userId == 0)
        {
            throw new InvalidOperationException("El usuario no es valido.");
        }

        var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        if (existingUser == null)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }

        // desconectado
        var estadoDesconectado = await _unitOfWork.StateRepositoty.GetByIdAsync(1);

        existingUser.StateId = 1; // desconectado
        existingUser.State = estadoDesconectado;

        _unitOfWork.UserRepository.Update(existingUser);

        await _unitOfWork.SaveAsync();

        Console.WriteLine("Usuario desconectado: " + existingUser.Nickname);
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

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.UserRepository.GetUserById(id);

        if (user == null)
        {
            return null;
        }

        return _userMapper.ToDto(user);
    }


    // REGISTRO E INICIO DE SESION
    public async Task<User> LoginAsync(LoginRequest loginRequest)
    {
        var user = await _unitOfWork.UserRepository.GetByEmailOrNickname(loginRequest.EmailOrNickname.ToLower());

        if (user == null || user.Password != PasswordHelper.Hash(loginRequest.Password))
        {
            return null;
        }

        // estado de conectado
        var estadoConectado = await _unitOfWork.StateRepositoty.GetByIdAsync(2);

        user.StateId = 2; // conectado
        user.State = estadoConectado;

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveAsync();

        return user;
    }

    public async Task<User> RegisterAsync(RegisterDto model)
    {
        // validacion email

        if (!emailRegex.IsMatch(model.Email)) {
            throw new Exception("Email no valido.");
        }

        try
        {
            
            // Verifica si el usuario ya existe
            var existingUser = await GetUserByEmailAsync(model.Email.ToLower());

            if (existingUser != null)
            {
                throw new Exception("El usuario ya existe.");
            }

            ImageService imageService = new ImageService();

            var newUser = new User
            {
                Email = model.Email.ToLower(),
                Nickname = model.Nickname.ToLower(),
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
