using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using TableRules.Core;

namespace GoogleSheetsSchedulesProvider.Services
{
    public class GoogleApiService
    {
        public string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        protected readonly string ApplicationName;
        protected readonly string SpreadsheetId;
        protected readonly string TimeCoordinates = "C3:C9";

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

        public GoogleApiService(string appName, string spreadsheetId)
        {
            ApplicationName = appName;
            SpreadsheetId = spreadsheetId;
        }
        public List<(string CellValue, TableContext Context)> SendRequest(int course, int day)
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

        private static readonly Regex SpacesRemover = new Regex("[ ]{2,}");

        private List<(string CellValue, TableContext Context)> Sort(BatchGetValuesResponse googleResponse, int course)
        {
            var subjectsResponse = googleResponse.ValueRanges[1].Values == null
                ? new List<IList<object>>()
                : googleResponse.ValueRanges[1].Values;
            var unsortedObjects = googleResponse.ValueRanges[0].Values
                ?.Zip(subjectsResponse, (x, y) => new { Time = x, Subjects = y })
                ?.Where(x => x.Subjects.Count > 0)
                .ToList();
            var sortedSubjects = new List<(string CellValue, TableContext Context)>();
            for (var i = 0; i < unsortedObjects.Count; i++)
                for (var j = 0; j < unsortedObjects[i].Subjects.Count; j++)
                    if (unsortedObjects[i].Subjects[j].ToString().Length > 1)
                        sortedSubjects.Add(
                            (SpacesRemover.Replace(unsortedObjects[i].Subjects[j].ToString(), " "),
                                new TableContext()
                                {
                                    CurrentTimeLabel = unsortedObjects[i].Time.FirstOrDefault().ToString().Trim(),
                                    CurrentGroupLabel = $"11-{OldNormalizeGroupNumber(course)}0{j + 1}"
                                }));
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
        public static int NormalizeGroupNumber(int course)
        {
            switch (course)
            {
                case 1:
                    return 8;
                case 2:
                    return 7;
                case 3:
                    return 6;
                case 4:
                    return 5;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static int OldNormalizeGroupNumber(int course)
        {
            switch (course)
            {
                case 1:
                    return 7;
                case 2:
                    return 6;
                case 3:
                    return 5;
                case 4:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
