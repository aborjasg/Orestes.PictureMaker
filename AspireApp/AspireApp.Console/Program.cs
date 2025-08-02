// See https://aka.ms/new-console-template for more information

using AspireApp.ApiService.DataAccess;
using AspireApp.Libraries;
using AspireApp.Libraries.Models;
using AspireApp.ServiceDefaults.Shared;
using ConsoleApp;
using IdentityModel.OidcClient;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

/*
var appSettings = @"C:\Redlen\github\AspireApp\AspireApp.ApiService\PlotTemplates.json";
using (StreamReader r = new StreamReader(appSettings))
{
    string json = r.ReadToEnd();
    var output = JsonConvert.DeserializeObject <PictureTemplate[]>(json);
    //Console.WriteLine("Count = {0}", output!.PlotImages.Length);
    Console.WriteLine("JSON: OK");
}
Console.WriteLine("Hello, World!");
*/

Utils.EventLog("Information", "Start Program...");

/*
            string s="aba";
            long n = 10;
            long r = repeatedString(s, n);            

            Console.WriteLine("string: {0}, n={1} -> Result={2}", s, n, r);
            */
/*
System.IO.StreamReader file = new System.IO.StreamReader(@"C:\ABorjas\Documents\HackerRank-C#\Source.txt");  
string p = file.ReadLine();
//string p = "bababa";
string v = file.ReadLine();
virusIndices(p, v);
*/
/*
int[] arr = {256741038, 623958417, 467905213, 714532089, 938071625};
miniMaxSum(arr);
*/
/*
string str = "!m-rB`-oN!.W`cLAcVbN/CqSoolII!SImji.!w/`Xu`uZa1TWPRq`uRBtok`xPT`lL-zPTc.BSRIhu..-!.!tcl!-U";
//string str = "www.abc.xy"; // OOK
int k = 62;
Console.WriteLine("{0}", caesarCipher(str, k));
*/

/*
System.IO.StreamReader file = new System.IO.StreamReader(@"files\migratoryBirds.txt");  
string n = file.ReadLine();
string str = file.ReadLine();
List<int> arr = str.Split(' ').Select(int.Parse).ToList();
Console.WriteLine("res = {0}", migratoryBirds(arr));
*/

/*
int year = 2016;
Console.WriteLine("date: {0}", dayOfProgrammer(year));
*/

/*
System.IO.StreamReader file = new System.IO.StreamReader(@"files\FrequencyQueries.txt");  

List<List<int>> queries = new List<List<int>>();
var q = file.ReadLine().ToString();
Console.WriteLine("q={0}", q);

string line = "";

while ((line = file.ReadLine()) != null) {
    queries.Add(line.TrimEnd().Split(' ').ToList().Select(queriesTemp => Convert.ToInt32(queriesTemp)).ToList());
}

List<int> ans = freqQuery(queries);
Console.WriteLine("{0}", string.Join("", ans));
Console.WriteLine("Count: {0}, Count(1):{1}", ans.Count, ans.Count(x => x == 1)); // 33246, 4918 (!)
*/

/*
string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString.NpgsqlConnection")!;
var obj = new TableAccess<RunImage>(connectionString);

// Invoke API Plotter:
using (var httpClient = new HttpClient())
{
    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://localhost:7583/plotter"))
    {
        try
        {
            //var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes("test@test.com:testPassword"));
            //request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

            request.Content = new StringContent("{\"Id\": 1}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Utils.EventLog("Information", content);

        }
        catch (Exception ex)
        {            
            Utils.EventLog("Warning", ex.Message);
        }
    }
}


// Save record to DB:

//var record = new RunImage() { Metadata= "{\"Name\": \"Combined_NCP\"}", Image = content, CreatedDate = DateTime.Now };
//int result = obj.SaveRow(record);
//Utils.EventLog("Information", $"Row saved -> Result={result}");


// Read record from DB:

var id = 1;
var row = obj.GetRow(id);
Utils.EventLog("Information", $"Row Id={id} -> Got the record");
Utils.EventLog("Information", $"Row Id={id} -> Serialized: {JsonConvert.SerializeObject(row)}");
*/

//var result = FakeData.GetLineChartData();

// OptimizeServers [2025-04-08]
/*
var power = new List<int> { 1, 2, 3 }; // { 1, 4, 6, 3 }
var cost = new List<int> { 1, 4, 2 }; // { 1, 2, 4, 3 }
*/
//Console.WriteLine($"OptimizeServers: Result={Functions.OptimizeServers(power, cost)}");

// ParkingBill [2025-04-09]
/*
ParkingBill("10:00", "13:21") -> $17
ParkingBill("09:42", "11:42") -> $9
*/
//Console.WriteLine($"ParkingBill: Result={Functions.ParkingBill("09:42", "11:42")}");


// ParityDegree [2025-04-09]
/*
ParityDegree(24) -> 3
*/
//Utils.EventLog("Information", $"ParityDegree: Result={Functions.ParityDegree(24)}");

// TransformJson [2025-04-17]
/*
 * Remove all keys with a value of "N/A" from the given JSON object.
 */
/*
//HttpClient client = new HttpClient();
//string s = await client.GetStringAsync("https://coderbyte.com/api/challenges/json/json-cleaning");
string s = "{\r\n\t\"name\": {\r\n\t\t\"first\": \"Robert\",\r\n\t\t\"middle\": \"\",\r\n\t\t\"last\": \"Smith\"\r\n\t},\r\n\t\"age\": 25,\r\n\t\"DOB\": \"-\",\r\n\t\"hobbies\": [\r\n\t\t\"running\",\r\n\t\t\"coding\",\r\n\t\t\"-\"\r\n\t],\r\n\t\"education\": {\r\n\t\t\"highschool\": \"N/A\",\r\n\t\t\"college\": \"Yale\"\r\n\t}\r\n}";
var node = JsonNode.Parse(s)!;
Utils.EventLog("Information", $"Before: {node}");
Functions.TransformJson(ref node);
Utils.EventLog("Information", $"After: {node}");
*/

Utils.EventLog("Information", "End of Program");
