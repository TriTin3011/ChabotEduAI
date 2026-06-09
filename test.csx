var key = "AQ.Ab8RN6Kb9PnfHm6u7RVmCL9wNTpgx2h420LmsTN4m1dFlEh_dw";
var url = $"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={key}";
var content = new StringContent("{\"model\":\"models/text-embedding-004\",\"content\":{\"parts\":[{\"text\":\"Hello\"}]}}", System.Text.Encoding.UTF8, "application/json");
using var client = new HttpClient();
var response = await client.PostAsync(url, content);
Console.WriteLine((int)response.StatusCode);
Console.WriteLine(await response.Content.ReadAsStringAsync());
