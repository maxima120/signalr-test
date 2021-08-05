using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace signalr_test.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> logger;
        private IHubContext<NewsHub> hub;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHubContext<NewsHub> hub)
        {
            this.hub = hub;
            this.logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("{connectionId}")] // defaulthublifetimemanager
        public IEnumerable<WeatherForecast> GetForSignalR(string connectionId)
        {
            SurrogateAuth(connectionId);

            // NB: in real app - send particular data to particular users (by connection)
            var timerManager = new TimerManager(() =>
            {
                Trace.WriteLine($"Server start: {DateTime.Now.TimeOfDay}");

                var id = NewsHub.Connected.Keys.First();
                var msg = "go!123.344!22";
                var sw = Stopwatch.StartNew();

                //Parallel.For(0, 20, (x) =>
                //{
                    for (int i = 0; i < 1000; i++)
                    {
                        var p = hub.Clients.Client(id);
                        SendAsync(p, msg);
                    }
                //});

                // 735ms - 597ms - single thread
                // 260 ms - parallel for :-O
                // without client logging: 80ms from server start to client finish, pure time on client - 75ms (seems 5ms to prepare something or the other)
                //    same as above - second run: 32ms from server start to client finish, pure time on client - 30ms

                Debug.WriteLine($"done: {sw.Elapsed}");

                //hub.Clients.Client(NewsHub.Connected.Keys.First()).SendAsync("servermessage", DateTime.Now.Ticks.ToString());
            });

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        async Task SendAsync(IClientProxy p, string msg)
        {
            await p.SendAsync("servermessage", msg);
        }

        private void SurrogateAuth(string connectionId)
        {
            var userId = GetApiUserSimple(this.HttpContext);

            var x = NewsHub.Connected.Values.SingleOrDefault(i => i.UserId == userId);
            if (x != null)
            {
                NewsHub.Connected.Remove(userId);
                Debug.WriteLine($"News Hub surrogate auth: {userId}. Previous connect removed.");
                // TODO : ask on github to implement force disconnect a client
            }

            if (!NewsHub.Connected.ContainsKey(connectionId))
            {
                throw new ApplicationException($"News Hub surrogate auth: connection not found {connectionId}.");
            }

            NewsHub.Connected[connectionId].UserId = userId;

            Debug.WriteLine($"News Hub surrogate auth: {userId}");
        }

        public static string GetApiUserSimple(HttpContext httpContext)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = httpContext.User;
            var userId = currentUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }
    }

    public class TimerManager
    {
        private Timer timer;
        private Action action;
        public TimerManager(Action action)
        {
            this.action = action;
            timer = new Timer(Execute);
            timer.Change(1000, 1000);
        }
        public void Execute(object stateInfo)
        {
            // stop for now - cant debug with dotpeek
            timer.Change(-1, -1);

            action();
        }
    }
}
