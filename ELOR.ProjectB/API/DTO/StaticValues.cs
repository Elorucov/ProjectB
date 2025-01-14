using System.Collections.ObjectModel;

namespace ELOR.ProjectB.API.DTO {
    public static class StaticValues {
        public static readonly ReadOnlyDictionary<byte, Tuple<string, string, bool>> SeverityList = new ReadOnlyDictionary<byte, Tuple<string, string, bool>>(new Dictionary<byte, Tuple<string, string, bool>> {
            { 1, new Tuple<string, string, bool>("Low", "Bugs that don't violate business logic, with an insignificant effect on the product overall, problems reflecting elements and data on the screen, grammatical and spelling mistakes.", true) },
            { 2, new Tuple<string, string, bool>("Medium", "The bug doesn't critically affect the product but causes a major inconvenience. The feature doesn't work correctly, but there is a workaround.", true) },
            { 3, new Tuple<string, string, bool>("High", "A feature isn't working properly or at all. For example, messages can't be sent, or photos can't be deleted.", true) },
            { 4, new Tuple<string, string, bool>("Critical", "Bugs that inhibit any further work from the app or further testing; crashes, freezing, loss of or damage to user data.", true) },
            { 5, new Tuple<string, string, bool>("Vulnerability", "Such reports visible only for the report creator and the product owner.", true) },
        });

        public static readonly ReadOnlyDictionary<byte, Tuple<string, string, bool>> ProblemTypesList = new ReadOnlyDictionary<byte, Tuple<string, string, bool>>(new Dictionary<byte, Tuple<string, string, bool>> {
            { 1, new Tuple<string, string, bool>("Suggestion", "Changes which you think should be made to improve the user experience.", true) },
            { 2, new Tuple<string, string, bool>("App crashes", "The app crashes, inhibiting any further work or testing.", true) },
            { 3, new Tuple<string, string, bool>("App froze", "The app freezes, inhibiting any further work or testing.", true) },
            { 4, new Tuple<string, string, bool>("Function not working", "Function not working or improperly working. For example, not sending messages or not deleting photos.", true) },
            { 5, new Tuple<string, string, bool>("Data damage", "User data fully or partially lost or corrupted.", true) },
            { 6, new Tuple<string, string, bool>("Performance", "User action response time.", true) },
            { 7, new Tuple<string, string, bool>("Aesthetic discrepancies", "Problems with element and data display on the screen.", true) },
            { 8, new Tuple<string, string, bool>("Typo", "Grammatical, orthographic or syntactical mistakes, or problems in localization.", true) }
        });

        public static readonly ReadOnlyDictionary<byte, Tuple<string, string, bool>> BugreportStatuses = new ReadOnlyDictionary<byte, Tuple<string, string, bool>>(new Dictionary<byte, Tuple<string, string, bool>> {
            { 0, new Tuple<string, string, bool>("Open", null, false) },
            { 1, new Tuple<string, string, bool>("In progress", "The developer has begun solving this issue.", true) },
            { 2, new Tuple<string, string, bool>("Fixed", "The problem has been fixed. Beta-testers currently aren't able to check this. The changes will appear in the newest version.", true) },
            { 3, new Tuple<string, string, bool>("Declined", "The report is denied due to the problem being entered for the wrong product or not being a bug.", true) },
            { 4, new Tuple<string, string, bool>("Under review", "A decision about this report will be made later.", true) },
            { 5, new Tuple<string, string, bool>("Closed", "The developer indicated that the issue has been resolved.", false) },
            { 6, new Tuple<string, string, bool>("Blocked", "The problem isn't related to the code of the tested product and occurs on the operating system API, library, side.", true) },
            { 7, new Tuple<string, string, bool>("Reopened", "The problem hasn't been fixed completely or at all. Please return to the report review.", true) },
            { 8, new Tuple<string, string, bool>("Cannot reproduce", "The problem isn't reproduced when following the steps in the report and according to the conditions described.", true) },
            { 9, new Tuple<string, string, bool>("Deferred", "The report has been accepted, but the issue described in it will be fixed much later.", true) },
            { 10, new Tuple<string, string, bool>("Needs correction", "There is insufficient information to localize the problem. The report wasn't created according to the rules.", true) },
            { 11, new Tuple<string, string, bool>("Ready for testing", "The problem has been fixed in the current version. The author of the report must check that this is accurate.", true) },
            { 12, new Tuple<string, string, bool>("Verified", "The issue has been fixed in the latest version.", true) },
            { 13, new Tuple<string, string, bool>("Won't be fixed", "The report describes a problem that can't be solved due to certain reasons.", true) },
            { 14, new Tuple<string, string, bool>("Outdated", "The report describes a problem that was temporary or that was eliminated after a redesign, refactoring or other work.", true) },
            { 15, new Tuple<string, string, bool>("Duplicate", "The report is a duplicate of an earlier bug report. The issue has already been described.", true) },
        });

        public static EnumInfoDTO ToAPIObject(this ReadOnlyDictionary<byte, Tuple<string, string, bool>> enums, byte key, bool extended = false) {
            var enumVal = enums.Where(k => k.Key == key).Select(kv => new EnumInfoDTO(kv.Key, kv.Value.Item1, extended ? kv.Value.Item2 : null, kv.Value.Item3)).FirstOrDefault();
            return enumVal != null ? enumVal : new EnumInfoDTO(key, null, null, false);
        }

        public static byte[] GetKeys(this ReadOnlyDictionary<byte, Tuple<string, string, bool>> enums) {
            return enums.Where(kv => kv.Value.Item3).Select(kv => kv.Key).ToArray();
        }
    }
}
