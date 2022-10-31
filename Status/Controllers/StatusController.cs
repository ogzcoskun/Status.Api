using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Status.Models;
using Status.Models.SocketModel;
using Status.Services.WebServices;
using System;

namespace Status.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class StatusController : ControllerBase
    {
        
        private readonly IWebServices _webService;
        private readonly IConfiguration _config;

        public StatusController(IWebServices webService, IConfiguration config)
        {          
            _webService = webService;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> PingServices([FromBody] List<EndpointModel> endpointList)
        {
            try
            {

                var accessToken = Request.Headers[HeaderNames.Authorization].ToString();
                if (accessToken != _config["PingAppAuthKey"])
                {
                    return Unauthorized("You need to Authorize!");
                }

                var response = await _webService.PingServices(endpointList);

                return Ok(response);

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            } 
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFailedServices()
        {
            try
            {
                var response = await _webService.GetAllFailedServices();
                return Ok(response);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFailedServicesByDate(string start, string end)
        {

            var response = await _webService.GetFailedServicesByDate(start, end);

            return Ok(response);
        }

        // URL Database

        [HttpPost]
        public async Task<IActionResult> AddEndpoint(EndpointModel endpoint)
        {
            var response = await _webService.AddEndpoint(endpoint);

            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearStatusHistory()
        {
            var response = await _webService.ClearStatusHistory();

            return Ok(response);
        }

        //[HttpPost]
        //public async Task<IActionResult> Login()
        //{
        //    var response = await _webService.Login();
        //    return Ok(response);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllEndpoints()
        {
            try
            {
                var response = await _webService.GetAllEndpoints();
                return Ok(response);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSockets()
        {
            try
            {
                var response = await _webService.GetAllSockets();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> TestSocket(SocketInfoModel info)
        //{
        //    try
        //    {
        //        var response = await _webService.PingSocket(info);

        //        return Ok(response);
        //    }
        //    catch(Exception ex)
        //    {
        //        return Ok(ex.Message);
        //    }
        //}
    }
}
