using ExampleLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ExampleLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        //Esta inyeccion nos permite usar lo que tenemos en el appsettings.json
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        // Este metodo es el que usaremos para enviar el usuario y la contraseña.
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserModel login)
        {
            //Inicializamos por defecto que estè desautorizado
            IActionResult response = Unauthorized();
            //Enviamos los datos de request al metodo privado AuthenticateUser()
            var user = AuthenticateUser(login);

            //validamos nuevamente que el usuario exista en la db
            if (user != null)
            {
                //Guardamos en una variable el string que genera el metodo privado GenerateJSONWebToken()
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }
            //retornamos la respuesta
            return response;
        }


        #region METODOS PRIVADOS - Lo ideal seria que estos metodos esten en una clase servicio y consumirlos por inyeccion de dependencias en el controller
        private string GenerateJSONWebToken(UserModel userInfo)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = null;

            //Aca validamos que el usuario existe en la base de datos,    
            //para el proposito de esta demo, se esta pasando el usuario Harcodeado, no estamos usando ningun contexto de DB.  
            if (login.Username == "Alejandro" && login.Password == "password")
            {
                user = new UserModel { Username = "Alejandro", Password = "password" };
            }
            return user;
        }
        #endregion
    }
}
