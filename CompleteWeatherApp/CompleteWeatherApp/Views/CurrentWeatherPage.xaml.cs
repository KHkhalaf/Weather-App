using CompleteWeatherApp.Helper;
using CompleteWeatherApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CompleteWeatherApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentWeatherPage : ContentPage
    {
        const string Url = "http://api.openweathermap.org/data/2.5/{0}?lat={1}&lon={2}&units=metric&appid=b95b8a4cd75481aa3bdc09a572e816cc";
        public CurrentWeatherPage()
        {
            InitializeComponent();

            GetWeatherInfo();
        }

        private async void GetWeatherInfo()
        {
            
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != Xamarin.Essentials.PermissionStatus.Granted)
                {
                    await DisplayAlert("Weather Info", "Coudn't access to openweathermap.org to get the weather information ", "OK");
                    return;
                }
            }
            try
            {
                Location position = await Geolocation.GetLastKnownLocationAsync();
                if (position == null)
                {
                    position = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromMinutes(10)
                    });
                }
                var url = string.Format(Url, "weather", position.Latitude, position.Longitude);

                var result = await ApiCaller.Get(url);
                if(result.Successful)
                {
                    try
                    {
                        var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);
                        descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();
                        iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                        cityTxt.Text = weatherInfo.name.ToUpper();
                        temperatureTxt.Text = weatherInfo.main.temp.ToString("0");
                        humidityTxt.Text = $"{weatherInfo.main.humidity}%";
                        pressureTxt.Text = $"{weatherInfo.main.pressure} hpa";
                        windTxt.Text = $"{weatherInfo.wind.speed} m/s";
                        cloudinessTxt.Text = $"{weatherInfo.clouds.all}%";

                        var dt = new DateTime().ToUniversalTime().AddSeconds(weatherInfo.dt);
                        dateTxt.Text = dt.ToString("dddd, MMM dd").ToUpper();

                        GetForecast(position.Latitude, position.Longitude);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Weather Info", ex.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Weather Info", "No weather information found", "OK");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Weather Info", ex.Message + " ... enable your location and try again", "OK");
            }
        }

        private async void GetForecast(double latitude, double longitude)
        {
            var url = string.Format(Url, "forecast", latitude, longitude);
            var result = await ApiCaller.Get(url);

            if (result.Successful)
            {
                try
                {
                    var forcastInfo = JsonConvert.DeserializeObject<ForecastInfo>(result.Response);
                    List<List> allList = new List<List>();

                    foreach (var list in forcastInfo.list)
                    {
                        //var date = DateTime.ParseExact(list.dt_txt, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
                        var date = DateTime.Parse(list.dt_txt);

                        if (date > DateTime.Now && date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                            allList.Add(list);
                    }

                    dayOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dddd");
                    dateOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dd MMM");
                    iconOneImg.Source = $"w{allList[0].weather[0].icon}";
                    tempOneTxt.Text = allList[0].main.temp.ToString("0");

                    dayTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dddd");
                    dateTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dd MMM");
                    iconTwoImg.Source = $"w{allList[1].weather[0].icon}";
                    tempTwoTxt.Text = allList[1].main.temp.ToString("0");

                    dayThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dddd");
                    dateThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dd MMM");
                    iconThreeImg.Source = $"w{allList[2].weather[0].icon}";
                    tempThreeTxt.Text = allList[2].main.temp.ToString("0");

                    dayFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dddd");
                    dateFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dd MMM");
                    iconFourImg.Source = $"w{allList[3].weather[0].icon}";
                    tempFourTxt.Text = allList[3].main.temp.ToString("0");

                }
                catch (Exception ex)
                {
                    await DisplayAlert("Weather Info", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Weather Info", "No forecast information found", "OK");
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            GetWeatherInfo();
        }
    }
}