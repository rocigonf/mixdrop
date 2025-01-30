using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Helper;
using mixdrop_back.Models.Mappers;
using System.Text;
using System.Text.RegularExpressions;

namespace mixdrop_back.Services;

public class UserService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly UserMapper _userMapper;

    Regex emailRegex = new(@"^([\w\.-]+)@([\w-]+)((\.(\w){2,3})+)$");

    public UserService(UnitOfWork unitOfWork, UserMapper userMapper)
    {
        _unitOfWork = unitOfWork;
        _userMapper = userMapper;
    }


    // buscar usuario
    public async Task<List<UserDto>> SearchUser(string search)
    {

        string searchSinTildes = Regex.Replace(search.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");

        var users = await _unitOfWork.UserRepository.SearchUser(searchSinTildes.ToLower());

        return _userMapper.ToDto(users).ToList();
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        return _userMapper.ToDto(users).ToList();
    }

    public async Task<User> GetBasicUserByIdAsync(int userId)
    {
        return await _unitOfWork.UserRepository.GetByIdAsync(userId);
    }

    public async Task<User> GetFullUserByIdAsync(int userId)
    {
        return await _unitOfWork.UserRepository.GetUserById(userId);
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


    // INICIO DE SESION
    public async Task<User> LoginAsync(LoginRequest loginRequest)
    {
        var user = await _unitOfWork.UserRepository.GetByEmailOrNickname(loginRequest.EmailOrNickname.ToLower());

        if (user == null || user.Password != PasswordHelper.Hash(loginRequest.Password))
        {
            return null;
        }

        // estado de conectado
        /*var estadoConectado = await _unitOfWork.StateRepositoty.GetByIdAsync(2);

        user.StateId = 2; // conectado
        user.State = estadoConectado;

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveAsync();*/

        return user;
    }

    // REGISTRO 
    public async Task<User> RegisterAsync(RegisterDto model)
    {
        // validacion email

        if (!emailRegex.IsMatch(model.Email))
        {
            throw new Exception("Email no valido.");
        }

        if (model.Password == null || model.Password.Length < 6)
        {
            throw new Exception("Contraseña no válida");
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
                AvatarPath = "",
                Role = "User", // Rol por defecto
                Password = PasswordHelper.Hash(model.Password),
                IsInQueue = false,  // por defecto al crearse
                StateId = 1
            };

            if (model.Image != null)
            {
                newUser.AvatarPath = "/" + await imageService.InsertAsync(model.Image);
            }

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

    public async Task ConnectUser(User user)
    {
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveAsync();
    }

    public async Task<UserDto> UpdateUser(RegisterDto model, User existingUser, string role)
    {
        // validacion email

        if (!emailRegex.IsMatch(model.Email))
        {
            throw new Exception("Email no valido.");
        }

        if (model.Password != null && model.Password != "" && model.Password.Length < 6)
        {
            throw new Exception("La contraseña no es válida");
        }

        try
        {
            // Verifica si el usuario ya existe
            if (!model.Email.Equals(existingUser.Email))
            {
                var otherUser = await GetUserByEmailAsync(model.Email.ToLower());

                if (otherUser != null)
                {
                    throw new Exception("El usuario ya existe.");
                }
            }

            ImageService imageService = new ImageService();

            existingUser.Email = model.Email.ToLower();
            existingUser.Nickname = model.Nickname.ToLower();

            // Si han pasado que la imagen debe cambiar y no es nula, guardo la imagen, pero si es nula, borro la que ya tenía
            if (model.ChangeImage.Equals("true"))
            {
                if (model.Image != null)
                {
                    existingUser.AvatarPath = await imageService.InsertAsync(model.Image);
                }
                else
                {
                    existingUser.AvatarPath = "";
                }
            }

            existingUser.Role = role;

            if (model.Password != null && model.Password != "")
            {
                existingUser.Password = PasswordHelper.Hash(model.Password);
            }

            _unitOfWork.UserRepository.Update(existingUser);
            await _unitOfWork.SaveAsync();

            return _userMapper.ToDto(existingUser);

        }
        catch (DbUpdateException ex)
        {
            // Log más detallado del error
            Console.WriteLine($"Error al guardar el usuario: {ex.InnerException?.Message}");
            throw new Exception("Error al registrar el usuario. Verifica los datos ingresados.");
        }
    }

    public UserDto ToDto(User user)
    {
        return _userMapper.ToDto(user);
    }
}
