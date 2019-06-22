using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineNotifyForFamily
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            LineNotify.SendMessage(Common.MakeWeeklyMessage());
            LineNotify.SendMessage(Common.MakeWeatherMessage());
        }
    }

    public static class LineNotify
    {
        static string token = TokenClass.token;

        public static void SendMessage(string message)
        {
            var result = sendMessage(message);
            result.Wait();
        }

        private static async Task sendMessage(string message)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "message", message },
                });

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.PostAsync("https://notify-api.line.me/api/notify", content);
            }
        }
    }

    public static class Common
    {
        private const string todayMessage = "今日は{0}です。";
        private const string HolidayMessage = "休んだり遊んだり、好きに過ごそう(/・ω・)/";
        private const string weekDayMessage = "適度に休みつつ働こう(｀・ω・´)";
        private const string weatherMessage = "天気は{0}です。";

        public static string MakeWeeklyMessage()
        {
            var dayOfWeek = DateTime.Today.DayOfWeek;
            var weather = new Weather();

            StringBuilder message = new StringBuilder();

            message.AppendFormat(todayMessage, DateTime.Today.ToString("dddd"));
            message.AppendLine();

            if (isHoliday)
            {
                message.AppendFormat(HolidayMessage);
            }
            else
            {
                message.AppendFormat(weekDayMessage);
            }

            return message.ToString();
        }

        public static string MakeWeatherMessage()
        {
            var weather = new Weather();

            StringBuilder message = new StringBuilder();

            message.AppendFormat(weatherMessage, weather.TodayWeather);

            return message.ToString();
        }

        private static bool isHoliday
        {
            get
            {
                return DateTime.Today.DayOfWeek == DayOfWeek.Sunday || DateTime.Today.DayOfWeek == DayOfWeek.Saturday;
            }
        }

    }

    public class Weather
    {
        private JObject jobj;

        public Weather()
        {
            string baseUrl = "http://weather.livedoor.com/forecast/webservice/json/v1";
            string cityname = "200010";

            string url = $"{baseUrl}?city={cityname}";
            string json = new HttpClient().GetStringAsync(url).Result;
            jobj = JObject.Parse(json);
        }

        public string TodayWeather
        {
            get { return (string)((jobj["forecasts"][0]["telop"] as JValue).Value); }
        }
    }
}
