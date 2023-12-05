using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apiUniversidade.Context;
using apiUniversidade.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using apiUniversidade.DTO;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

[ApiController]
[Route("[Controller]")]
public class AutorizaController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AutorizaController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> SignInManager)
    {
        _userManager = userManager;
        _signInManager = SignInManager;
    }

    [HttpGet]
    public ActionResult<string> Get(){
        return "AutorizaController :: Acesso em : " + DateTime.Now.ToLongDateString();
    }

    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody]UsuarioDTO model){
        var user = new IdentityUser{
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if(!result.Succeeded){
            return BadRequest(result.Errors);
        }

        await _signInManager.SignInAsync(user, false);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login ([FromBody]UsuarioDTO userInfo){
        var result = await _signInManager.PasswordSignInAsync(userInfo.Email, userInfo.Password, isPersistent: false, lockoutOnFailure: false);
        if(result.Succeeded){
            return Ok();
        }else{
            ModelState.AddModelError(string.Empty,"login inválido...");
            return BadRequest(ModelState);
        }
    }

    private readonly IConfiguration _configuration;

    public AutorizaController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration){
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    private UsuarioToken GeraToken(UsuarioDTO userInfo){
        var claims = new[]{
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName,userInfo.Email),
            new Claim("IFRN","TecInfo"),
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:ket"]));
        var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

        var expiracao = _configuration["TokenCOnfiguration:ExpiredHours"];
        var expiration = DateTime.UtcNow.AddHours(double.Parse(expiracao));

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["TokenConfiguration: Issuer"],
            audience: _configuration["TokenConfiguration: Audeince"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new UsuarioToken(){
            Authenticated = true,
            Expiration = expiration,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Message = "JWT Ok."
        };
    }

    
}