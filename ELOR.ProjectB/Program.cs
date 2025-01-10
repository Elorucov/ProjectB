using ELOR.ProjectB.API;
using Microsoft.AspNetCore.Http.Json;

namespace ELOR.ProjectB {
    public class Program {
        public static BTConfig Config { get; private set; }

        public static void Main(string[] args) {
            Task apiTask = Task.Factory.StartNew(() => StartApi(args));
            Task.WaitAll(apiTask);
        }

        private static void Init(WebApplication app) {
            app.Use((context, next) => {
                if (Config == null) Config = app.Configuration.Get<BTConfig>();
                return next();
            });
        }

        private static void StartApi(string[] args) {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<JsonOptions>(o => {
                o.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
            builder.Configuration.AddJsonFile("btconfig.json");
            builder.WebHost.UseUrls("http://localhost:7575");
            var app = builder.Build();

            Init(app);
            APIMain.Initialize(app);

            app.Run();
        }
    }
}