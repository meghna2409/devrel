using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MeghnaSchedulerFunction02
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public async static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            using (var httpClient = new HttpClient())
            {
                string geturl = $"https://schedulebotwebapp.azurewebsites.net/GetSchedules";
                var response = await httpClient.GetAsync(new Uri(geturl));
                List<Schedule> schedules = new List<Schedule>();

                if (response.IsSuccessStatusCode)
                {
                    schedules = JsonConvert.DeserializeObject<List<Schedule>>(await response.Content.ReadAsStringAsync());
                }

                foreach (var schedule in schedules)
                {
                    // Your Account SID from twilio.com/console
                    var accountSid = "ACf835a4e932cb6f65ca491d83e0081ce2";
                    // Your Auth Token from twilio.com/console
                    var authToken = "a254751b5d8e9dc9ae576af29c4b83f9";

                    TwilioClient.Init(accountSid, authToken);

                    var message = MessageResource.Create(
                        to: new PhoneNumber("+12248043095"),
                        from: new PhoneNumber("+16304736963"),
                        body: $"Reminder from SchedulerBot. {schedule.EventName} at {schedule.EventDate}");

                    string updateurl = $"https://schedulebotwebapp.azurewebsites.net/CompleteSchedule";
                    var content = new StringContent(JsonConvert.SerializeObject(schedule), Encoding.UTF8, "application/json");

                    using (var _httpClient = new HttpClient())
                    {
                        var r = await _httpClient.PostAsync(updateurl, content);
                    }

                }
            }

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
