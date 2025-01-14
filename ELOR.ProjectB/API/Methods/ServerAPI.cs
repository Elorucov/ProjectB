using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.DataBase;

namespace ELOR.ProjectB.API.Methods {
    public static class ServerAPI {
        public static IResult GetEnums(HttpRequest request) {
            return Results.Json(new APIResponse<ServerEnumsDTO>(new ServerEnumsDTO {
                Severities = StaticValues.SeverityList.Select(kv => new EnumInfoDTO(kv.Key, kv.Value.Item1, kv.Value.Item2, kv.Value.Item3)).ToList(),
                ProblemTypes = StaticValues.ProblemTypesList.Select(kv => new EnumInfoDTO(kv.Key, kv.Value.Item1, kv.Value.Item2, kv.Value.Item3)).ToList(),
                ReportStatuses = StaticValues.BugreportStatuses.Select(kv => new EnumInfoDTO(kv.Key, kv.Value.Item1, kv.Value.Item2, kv.Value.Item3)).ToList()
            }));
        }

        public static async Task<IResult> TestDBProcedureErrorAsync(HttpRequest request) {
            await DBClient.TestDBProcedure();
            return Results.Json(new APIResponse<bool>(true));
        }

        public static async Task<IResult> InitDBAsync(HttpRequest request) {
            bool result = await DBClient.SetupDatabaseAsync();
            return Results.Json(new APIResponse<bool>(result));
        }
    }
}