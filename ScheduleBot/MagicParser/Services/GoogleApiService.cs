using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using MagicParser.Helpers;
using MagicParser.Models;

namespace MagicParser.Services
{
    public class GoogleApiService
    {
        public string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private const string ApplicationName = "itis-api";
        private const string SpreadsheetId = "1DHir9K8KO8a2AX3AfPiE422HXgf_7AKgSOSS-UOMt_A";
        private const string TimeCoordinates = "C3:C9";

        private static UserCredential Auth(string[] scopes)
        {
            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                return credential;
            }
        }

        public List<TmpObject> SendRequest(int course, int day)
        {
            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = Auth(Scopes),
                ApplicationName = ApplicationName
            });
            var spreadsheetId = SpreadsheetId;
            var request
                = service.Spreadsheets.Values.BatchGet(spreadsheetId);
            request.Ranges = GetDailyCoordinates(course, day);
            return Sort(request.Execute(), course);
        }

        public BatchGetValuesResponse SendRequest()
        {
            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = Auth(Scopes),
                ApplicationName = ApplicationName
            });
            var spreadsheetId = SpreadsheetId;
            var request
                = service.Spreadsheets.Values.BatchGet(spreadsheetId);
            request.Ranges = GetWeeklyCoordinates();
            return request.Execute();
        }

        private List<TmpObject> Sort(BatchGetValuesResponse googleResponse, int course)
        {
            var subjectsResponse = googleResponse.ValueRanges[1].Values == null
                ? new List<IList<object>>()
                : googleResponse.ValueRanges[1].Values;
            var unsortedObjects = googleResponse.ValueRanges[0].Values
                ?.Zip(subjectsResponse, (x, y) => new { Time = x, Subjects = y })
                ?.Where(x => x.Subjects.Count > 0)
                .ToList();
            var sortedSubjects = new List<TmpObject>();
            for (var i = 0; i < unsortedObjects.Count; i++)
                for (var j = 0; j < unsortedObjects[i].Subjects.Count; j++)
                    if (unsortedObjects[i].Subjects[j].ToString().Length > 1)
                        sortedSubjects.Add(new TmpObject
                        {
                            Content = unsortedObjects[i].Subjects[j].ToString(),
                            Group = $"11-{Converter.NormalizeGroupNumber(course)}0{j + 1}",
                            Time = unsortedObjects[i].Time.FirstOrDefault().ToString(),
                            Course = course
                        });
            return sortedSubjects;
        }

        private Repeatable<string> GetDailyCoordinates(int course, int day)
        {
            var coordinates = new List<string> { "D3:L9", "N3:U9", "V3:AC9", "AD3:AK9" };
            int cNew1 = 3, cNew2 = 9;
            var coords = coordinates[course - 1];
            if (day > 1)
            {
                cNew1 = 3 + 7 * (day - 1);
                cNew2 = 9 + 7 * (day - 1);
            }

            if (course == 1 && day == 6)
                cNew2 = cNew2 - 1;
            coords = coords.Replace("3", cNew1.ToString());
            coords = coords.Replace("9", cNew2.ToString());

            return new Repeatable<string>(new[] { TimeCoordinates, coords });
        }

        private Repeatable<string> GetWeeklyCoordinates()
        {
            return new Repeatable<string>(new[]
            {
                TimeCoordinates,
                "D3:L9",
                "D10:L16",
                "D17:L23",
                "D24:L30",
                "D31:L37",
                "D38:L43",
                "N3:U9",
                "N10:U16",
                "N17:U23",
                "N24:U30",
                "N31:U37",
                "N38:U44",
                "V3:AC9",
                "V10:AC16",
                "V17:AC23",
                "V24:AC30",
                "V31:AC37",
                "V38:AC44",
                "AD3:AK9",
                "AD10:AK16",
                "AD17:AK23",
                "AD24:AK30",
                "AD31:AK37",
                "AD38:AK44"
            });
        }
    }
}
