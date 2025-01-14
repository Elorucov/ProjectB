using ELOR.ProjectB.API.Methods;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.DataBase;
using Microsoft.AspNetCore.Diagnostics;

namespace ELOR.ProjectB.API {
    public static class APIMain {
        public static void Initialize(WebApplication app) {
            app.UseExceptionHandler(c => c.Run(async context => {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                var resp = APIResponse<object>.GetForError(exception.ToAPIObject(out var httpCode));
                context.Response.ContentType = "text/json";
                context.Response.StatusCode = httpCode;
                await context.Response.WriteAsJsonAsync(resp);
            }));

            app.Map("/auth.signIn", AuthAPI.SignInAsync);
            app.Map("/auth.signUp", AuthAPI.SignUpAsync);

            app.Map("/invites.create", InvitesAPI.CreateAsync);

            app.Map("/products.create", ProductsAPI.CreateAsync);
            app.Map("/products.get", ProductsAPI.GetAsync);
            app.Map("/products.setAsFinished", ProductsAPI.SetAsFinishedAsync);

            app.Map("/reports.changeSeverity", ReportsAPI.ChangeSeverityAsync);
            app.Map("/reports.changeStatus", ReportsAPI.ChangeStatusAsync);
            app.Map("/reports.create", ReportsAPI.CreateAsync);
            app.Map("/reports.delete", ReportsAPI.DeleteAsync);
            app.Map("/reports.get", ReportsAPI.GetAsync);

            app.Map("/server.getEnumStrings", ServerAPI.GetEnums);
            app.Map("/server.init", ServerAPI.InitDBAsync);
            // app.Map("/server.testDBError", ServerAPI.TestDBProcedureErrorAsync);

            app.Map("/{method}", () => {
                throw new UnknownMethodException();
            });
            app.Map("/", () => {
                throw new UnknownMethodException();
            });
        }
    }
}
