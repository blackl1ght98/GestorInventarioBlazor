﻿using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Server.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Application.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly DbcrudBlazorContext _context;

        public TokenService(IConfiguration configuration, DbcrudBlazorContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public async Task<DTOLoginResponse> GenerarToken(Usuario credencialesUsuario)
        {

            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == credencialesUsuario.Id);



            var claims = new List<Claim>()
            {
                new(ClaimTypes.Name, credencialesUsuario.NombreCompleto),
                 new Claim(ClaimTypes.Email, credencialesUsuario.Email),
                 new Claim(ClaimTypes.Role, credencialesUsuario.IdRolNavigation.Nombre),

                 new Claim(ClaimTypes.NameIdentifier, credencialesUsuario.Id.ToString())

            };

            var clave = _configuration["ClaveJWT"];
            var claveKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave));
            var signinCredentials = new SigningCredentials(claveKey, SecurityAlgorithms.HmacSha256);
            var securityToken = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtAudience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: signinCredentials);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new DTOLoginResponse()
            {
                Id = credencialesUsuario.Id,
                Token = tokenString,
                Rol = credencialesUsuario.IdRolNavigation.Nombre,
                Email=credencialesUsuario.Email,
                NombreCompleto=credencialesUsuario.NombreCompleto,
             
            };
        }
    }
}
