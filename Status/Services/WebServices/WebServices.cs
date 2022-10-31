using RestSharp;
using Status.Models;
using System.Net;
using MongoDB.Driver;
using Status.Library;
using Status.Services.MessagingServices;
using MongoDB.Bson;
using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http;
using Status.Models.SocketModel;
using System.Windows;

using Telegram.Bot.Types;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using MongoDB.Bson.Serialization.Serializers;
using System.Diagnostics;

namespace Status.Services.WebServices
{
    public class WebServices : IWebServices
    {
        private readonly IMessageService _messageService;
        private readonly IConfiguration _config;
        private readonly ILibrary _library;
        

        public WebServices(IConfiguration config, ILibrary library, IMessageService messageService)
        {
            _messageService = messageService;
            _config = config;
            _library = library;           
        }

        public async Task<ServiceResponse> PingServices(List<EndpointModel> endpointList)
        {
            MongoClient clientDb = new MongoClient(_config["MongoDb"]);

            var errorList = new List<ServiceStatusModel>();
            var successList = new List<ServiceStatusModel>();
            var accessToken = (await Login()).Message;

            foreach(var endpoint in endpointList)
            {
                if(endpoint.Method.ToUpper() == EMethods.GET.ToString())
                {
                    var client = new RestClient(endpoint.Url);
                    client.Timeout = -1;
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    var response = await client.ExecuteAsync(request);

                    if (response.StatusCode == endpoint.ExpectedResponse)
                    {
                        var status = new ServiceStatusModel()
                        {
                            ServiceId = endpoint.EndpointId,
                            ServiceName = endpoint.Url,
                            IsRunning = true,
                            Date = DateTime.Now,
                            StatusMessage = "Service is Running"
                        };
                        successList.Add(status);
                    }
                    else
                    {
                        var status = new ServiceStatusModel()
                        {
                            ServiceId = endpoint.EndpointId,
                            ServiceName = endpoint.Url,
                            IsRunning= false,
                            Date = DateTime.Now,
                            StatusMessage = response.ErrorMessage
                        };
                        errorList.Add(status);
                    }
                }else if(endpoint.Method.ToUpper() == EMethods.POST.ToString())
                {
                    var parameters = JsonConvert.SerializeObject(endpoint.Parameters);

                    var client = new RestClient(endpoint.Url);
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    request.AddJsonBody(parameters);

                    var response = await client.ExecuteAsync(request);

                    if (response.StatusCode == endpoint.ExpectedResponse)
                    {
                        var status = new ServiceStatusModel()
                        {
                            ServiceId = endpoint.EndpointId,
                            ServiceName = endpoint.Url,
                            IsRunning = true,
                            Date = DateTime.Now,
                            StatusMessage = "Service is Running"
                        };
                        successList.Add(status);
                    }
                    else
                    {
                        var status = new ServiceStatusModel()
                        {
                            ServiceId = endpoint.EndpointId,
                            ServiceName = endpoint.Url,
                            IsRunning= false,
                            Date = DateTime.Now,
                            StatusMessage = response.ErrorMessage
                        };
                        errorList.Add(status);
                    }
                }else if(endpoint.Method.ToUpper() == EMethods.SOCKET.ToString())
                {
                    var hubGroup = endpoint.Parameters["hubGroup"];
                    var socketName = endpoint.Parameters["socketName"];
                    var returnum = Int32.Parse(endpoint.Parameters["returnNum"]);

                    var idList = new List<string>();

                    if (endpoint.Parameters.ContainsKey("idList"))
                    {
                        var id = endpoint.Parameters["idList"];
                        var idSplit = id.Split(',');

                        foreach(var i in idSplit)
                        {
                            idList.Add(i);
                        }
                    }
                    else
                    {
                        idList = null;
                    }
                    
                    var socket = new SocketInfoModel()
                    {
                        Url = endpoint.Url,
                        HubGroup = hubGroup,
                        IdList = idList,
                        SocketName = socketName,
                        ReturnNum = returnum,
                    };

                    var response = await PingSocket(socket);

                    var expectingResult = endpoint.ExpectedResponse == HttpStatusCode.OK ? true : false;

                    if(response.isSuccess == expectingResult)
                    {
                        var socketStatus = new SocketStatus()
                        {
                            EndpointId = endpoint.EndpointId,
                            Socket = hubGroup,
                            LastUpdate = (DateTime.Now.ToString("MM/dd/yyyy HH:mm")).ToString(),
                        };

                        var socketCollection = clientDb.GetDatabase("Status").GetCollection<SocketStatus>("SocketStatus");
                        await socketCollection.ReplaceOneAsync(c => c.EndpointId == endpoint.EndpointId, socketStatus);

                        //var status = new ServiceStatusModel()
                        //{
                        //    ServiceId = endpoint.EndpointId,
                        //    ServiceName = $"Url: {endpoint.Url}-hubGroup: {hubGroup}-socketName: {socketName}",
                        //    IsRunning = true,
                        //    Date = DateTime.Now,
                        //    StatusMessage = "Service is Running"
                        //};
                        //successList.Add(status);
                    }
                    //else
                    //{
                    //    var status = new ServiceStatusModel()
                    //    {
                    //        ServiceId = endpoint.EndpointId,
                    //        ServiceName = $"Url: {endpoint.Url}---hubGroup: {hubGroup}---socketName: {socketName}",
                    //        IsRunning = false,
                    //        Date = DateTime.Now,
                    //        StatusMessage = response.Message
                    //    };
                    //    errorList.Add(status);
                    //}
                    
                }// Other request methods will be herein else if (else if(endpoint.Method.ToUpper() == EMethods.POST.ToString()))
            }

            var successCollection = clientDb.GetDatabase("Status").GetCollection<ServiceStatusModel>("StatusSuccess");
            var failureCollection = clientDb.GetDatabase("Status").GetCollection<ServiceStatusModel>("StatusFailure");

            try
            {
                if (successList.Count > 0)
                {
                    successCollection.InsertMany(successList);
                }
                if (errorList.Count > 0)
                {
                    failureCollection.InsertMany(errorList);
                }

                foreach (var error in errorList)
                {
                    await _messageService.SendTelegramMessage(error);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse()
                {
                    isSuccess = false,
                    Message = ex.Message
                };
            }

            return new ServiceResponse()
            {
                isSuccess = true,
                Message = ""
            };
        }

        public async Task<List<FailedServicesResponse>> GetAllFailedServices() 
        {
            MongoClient clientDb = new MongoClient(_config["MongoDb"]);

            var failureCollection = clientDb.GetDatabase("Status").GetCollection<FailedServicesResponse>("StatusFailure");

            var errorList = await failureCollection.Find(new BsonDocument()).ToListAsync();

            return errorList;
        }

        public async Task<List<FailedServicesResponse>> GetFailedServicesByDate(string start, string end)
        {
            MongoClient clientDb = new MongoClient(_config["MongoDb"]);

            var failureCollection = clientDb.GetDatabase("Status").GetCollection<FailedServicesResponse>("StatusFailure");

            var errorList = await failureCollection.Find(new BsonDocument()).ToListAsync();

            var listToSend = new List<FailedServicesResponse>();

            var a = Convert.ToDateTime(start);

            foreach (var error in errorList)
            {
                if(error.Date >= Convert.ToDateTime(start) && error.Date < Convert.ToDateTime(end))
                {
                    listToSend.Add(error);
                }
            }

            return listToSend;
        }

        public async Task<ServiceResponse> Login()
        {

            var user = new LoginModel()
            {
                Email = _config["Login:Username"],
                Password = _config["Login:Password"]
            };
            var userJson = $"{{\"grant_type\": \"password\",\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}";

            var client = new RestClient(_config["Login:OtpUrl"]);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddJsonBody(userJson);
 
            var response = await client.ExecuteAsync(request);

            var modResponse = JsonConvert.DeserializeObject<Dictionary<string,string>>(response.Content);

            var accessToken = modResponse["access_token"];

            return new ServiceResponse()
            {
                isSuccess = true,
                Message = accessToken
            };
        }

        


        public async Task<ServiceResponse> AddEndpoint(EndpointModel endpoint)
        {
            try
            {
                endpoint.EndpointId = Guid.NewGuid().ToString();

                MongoClient clientDb = new MongoClient(_config["MongoDb"]);
                var urlCollection = clientDb.GetDatabase("Status").GetCollection<EndpointModel>("EndpointList");
                urlCollection.InsertOne(endpoint);

                return new ServiceResponse()
                {
                    isSuccess = true,
                    Message = "Endpoint Added To Endpoint List."
                };
            }
            catch(Exception ex)
            {
                return new ServiceResponse()
                {
                    isSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse> ClearStatusHistory()
        {
            MongoClient clientDb = new MongoClient(_config["MongoDb"]);
            var failureCollection = clientDb.GetDatabase("Status").GetCollection<FailedServicesResponse>("StatusFailure");
            var successCollection = clientDb.GetDatabase("Status").GetCollection<ServiceStatusModel>("StatusSuccess");

            try
            {
                failureCollection.DeleteMany(new BsonDocument());
                successCollection.DeleteMany(new BsonDocument());
                return new ServiceResponse()
                {
                    isSuccess = true,
                    Message = ""
                };
            }
            catch(Exception ex)
            {
                return new ServiceResponse()
                {
                    isSuccess= false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse> PingSocket(SocketInfoModel info)
        {
            // sample Url = "https://apiv2.coinpara.com/CoreHub"
            // sample hubGroup = "JoinOrderHubLiteGroupAsync"
            //sample id = "DED88BB9-6A4B-464B-BAB8-B2C6AEECC97F,DED88BB9-6A4B-464B-BAB8-B2C6AEECC97F"
            // sample conn.On<object, object, object>("notifyOrderDashBoard", (asks, bids, depth)
            try
            {
                string isSocketWorkin = null;

                HubConnection conn = new HubConnectionBuilder()
                    .WithUrl(info.Url)
                    //.WithConsoleLogger()
                    .Build();
                await conn.StartAsync().ContinueWith(t => {
                    if (t.IsFaulted)
                        Console.WriteLine(t.Exception.GetBaseException());
                    //else
                    //    Console.WriteLine("Connected to Hub");
                });
                try
                {
                    var id = info.IdList[0];

                    await conn.InvokeAsync(info.HubGroup, id, default);
                }
                catch(Exception ex)
                {
                    isSocketWorkin = ex.Message;
                }

                System.Threading.Thread.Sleep(3000);

                if (info.ReturnNum == 1)
                {
                    conn.On<object>(info.SocketName, (asks) => {
                        //var time = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
                        //Console.WriteLine(time);

                        //Console.WriteLine(asks);

                        var asksN = asks == null ? String.Empty : asks;

                        isSocketWorkin = asks.ToString();
                    });

                }else if(info.ReturnNum == 3)
                {
                    await conn.InvokeAsync(info.HubGroup, info.IdList[0], default);
                    List<object> returnParams = new List<object>();
                    returnParams.Capacity = 2;

                    conn.On<object,object,object>(info.SocketName, (asks,bids,depth) => {
                        var time = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
                        //Console.WriteLine(time);

                        //Console.WriteLine(asks);

                        var asksN = asks == null ? String.Empty : asks;
                        var bidsN = bids == null ? String.Empty : bids;
                        var depthN = depth == null ? String.Empty : depth;

                        isSocketWorkin = asks.ToString();
                    });
                }

                System.Threading.Thread.Sleep(10000);

                await conn.StopAsync();

                if (isSocketWorkin != null)
                {
                    return new ServiceResponse()
                    {
                        isSuccess = true,
                        Message = "Socket Working"
                    };
                }
                else                               
                {
                    return new ServiceResponse()
                    {
                        isSuccess = false,
                        Message = "Socket Doesn'T Working!!!"
                    };
                }
            }
            catch(Exception ex)
            {
                return new ServiceResponse()
                {
                    isSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<List<EndpointDbModel>> GetAllEndpoints()
        {
            MongoClient clientDb = new MongoClient(_config["MongoDb"]);

            var endpointCollection = clientDb.GetDatabase("Status").GetCollection<EndpointDbModel>("EndpointList");

            var urlCollection = await endpointCollection.Find(new BsonDocument()).ToListAsync();

            return urlCollection;
        }

        public async Task<List<SocketStatus>> GetAllSockets()
        {
            MongoClient clientDb = new MongoClient(_config["MongoDb"]);

            var endpointCollection = clientDb.GetDatabase("Status").GetCollection<SocketStatus>("SocketStatus");

            var SocketCollection = await endpointCollection.Find(new BsonDocument()).ToListAsync();

            return SocketCollection;
        }

    }
}
