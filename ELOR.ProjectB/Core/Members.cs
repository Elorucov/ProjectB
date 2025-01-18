using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace ELOR.ProjectB.Core {
    public static class Members {
        public static async Task<List<MemberDTO>> GetByIdAsync(List<uint> ids) {
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

            resp.Close();
            await resp.DisposeAsync();
            return members;
        }

        public static async Task<Tuple<MemberDTO, uint?, string>> GetByIdAsync(uint id) {
            string sql = $"SELECT m.*, i.created_by AS invited_by, im.user_name AS invited_by_user_name FROM members m LEFT JOIN invites i ON i.invited_member_id = @id LEFT JOIN members im ON im.id = i.created_by WHERE m.id  = @id";

            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@id", id);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            MemberDTO member = null;
            uint? invitedBy = null;
            string invitedByUserName = null;
            if (resp.HasRows) {
                while (resp.Read()) {
                    uint memberId = (uint)resp.GetDecimal(0);
                    string userName = resp.GetString(1);
                    string firstName = resp.GetString(2);
                    string lastName = resp.GetString(3);
                    member = new MemberDTO {
                        Id = memberId,
                        UserName = userName,
                        FirstName = firstName,
                        LastName = lastName
                    };
                    if (!resp.IsDBNull(4)) invitedBy = (uint)resp.GetDecimal(4);
                    if (!resp.IsDBNull(5)) invitedByUserName = resp.GetString(5);
                    break; // because we need 1 row.
                }
            }

            resp.Close();
            await resp.DisposeAsync();
            return new Tuple<MemberDTO, uint?, string>(member, invitedBy, invitedByUserName);
        }
    }
}
