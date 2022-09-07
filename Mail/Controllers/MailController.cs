using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using DB.Interfaces;
using DB.Migrations;
using Entities;
using Mail.Inferfaces;
using Mail.Models;
using Mail.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Mail.Controllers;

[ApiController]
[Route("[controller]")]
public class MailController : ControllerBase
{
    

    private readonly ILogger<MailController> _logger;
    private readonly IMailInterface _mails;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly IRabbitMqProvider _rabbitMqProvider;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    public MailController(IConfiguration configuration,IUserService userService,ILogger<MailController> logger, IMailInterface mails, IRabbitMqPublisher rabbitMqPublisher, IRabbitMqProvider rabbitMqProvider)
    {
        _logger = logger;
        _mails = mails;
        _rabbitMqPublisher = rabbitMqPublisher;
        _rabbitMqProvider = rabbitMqProvider;
        _userService = userService;
        _configuration = configuration;
    }
    [AllowAnonymous]
    [HttpPost("/Token")]
    public async Task<IActionResult> Token(UserInfo _userData)
    {
        if (_userData != null && _userData.dealerMail != null && (_userData.secretKey != null || _userData.password != null))
        {
            User user = _userService.GetByDealerMail(_userData.dealerMail);
            if (user == null)
            {
                return BadRequest("Invalid credentials");
            }
            if (_userData.secretKey!= null && _userData.secretKey != "")
            {
                if (user.secret_key != _userData.secretKey)
                {
                    return BadRequest("Invalid credentials");
                }
            } else
            {
                if (user.password != _userService.GenSHA512(_userData.password))
                {
                    return BadRequest("Invalid credentials");
                }
            }

            if (user != null)
            {
                var issuer = _configuration.GetValue<string>("Jwt:Issuer");
                var audience = _configuration.GetValue<string>("Jwt:Audience");
                var key = Encoding.ASCII.GetBytes
                (_configuration.GetValue<string>("Jwt:Key"));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, user.user_name),
                        new Claim(JwtRegisteredClaimNames.Email, user.dealer_mail),
                        new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }
        else
        {
            return BadRequest();
        }
    }
    [AllowAnonymous]
    [HttpPost("/Register")]
    public async Task<IActionResult> Register(UserCreate _userData)
    {
        User checkUser = _userService.GetByDealerMail(_userData.dealerMail);
        if (checkUser != null)
        {
            return BadRequest("There is a registered user with this email");
        }
        User userData = new User();
        userData.dealer_mail = _userData.dealerMail;
        userData.active = 1;
        string[] explodedMail = userData.dealer_mail.Split('@');
        userData.site_url = "https://www." + explodedMail[1];
        userData.secret_key = _userService.NewSecretKey(55);
        userData.user_name = explodedMail[1];
        userData.password = _userService.GenSHA512(_userData.password);
        User user = _userService.Create(userData);
        return Ok(true);
    }
    [Authorize]
    [HttpPost("/SendMail")]
    public IActionResult NewMail(MailRequest mailRequest)
    {
        _logger.LogInformation("NewMail RQ : "+JsonConvert.SerializeObject(mailRequest));
        getMailRequestOnQueue getMailRequestOnQueue = new getMailRequestOnQueue();
        getMailRequestOnQueue.From = mailRequest.From;
        getMailRequestOnQueue.To = mailRequest.To;
        getMailRequestOnQueue.TransactionId = _userService.NewTransaction();
        var dealerMail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
        if (String.IsNullOrEmpty(dealerMail))
        {
            return BadRequest("Invalid Token");
        }

        getMailRequestOnQueue.DealerMail = dealerMail;

        User getUser = _userService.GetByDealerMail(dealerMail);
        
        string userGuid = getUser.guid;
        int userId = getUser.id;
        

        getMailRequestOnQueue.Guid = userGuid;
        getMailRequestOnQueue.UserId = userId;

        this.SendToTable(mailRequest, getMailRequestOnQueue);


        return Ok(true);
    }

    private void SendToTable(MailRequest mailRequest, getMailRequestOnQueue getMailRequestOnQueue)
    {
        using (var connection = _rabbitMqProvider.ProvideConnection())
        {
            using (var channel = connection.CreateModel())
            {

                channel.QueueDeclare(queue: "mail_create",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
                foreach (string mail in mailRequest.Mails)
                {
                    getMailRequestOnQueue.Mail = mail;
                    _rabbitMqPublisher.Publish(channel, JsonConvert.SerializeObject(getMailRequestOnQueue), "mail_create");
                }
            }
        }
        
    }

}


