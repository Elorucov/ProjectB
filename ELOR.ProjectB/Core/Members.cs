using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace ELOR.ProjectB.Core {
    public static class Members {
        public static async Task<List<MemberDTO>> GetAsync(List<uint> ids) {
            string idstr = string.Join(",", ids);
            string sql = $"SELECT * FROM members WHERE id IN ({idstr})";

            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            List<MemberDTO> members = new List<MemberDTO>();
            if (resp.HasRows) {
                while (resp.Read()) {
                    uint memberId = (uint)resp.GetDecimal(0);
                    string userName = resp.GetString(1);
                    string firstName = resp.GetString(2);
                    string lastName = resp.GetString(3);
                    members.Add(new MemberDTO { 
                        Id = memberId,
                        UserName = userName,
                        FirstName = firstName,
                        LastName = lastName
                    });
                }
            }

            await resp.DisposeAsync();
            return members;
        }
    }
}
