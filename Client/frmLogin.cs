using System.Net.Http.Headers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Newtonsoft.Json;

namespace BaseClient
{
    public partial class FrmLogin : Form
    {
        public string Jwt { get; private set; } = string.Empty; 

        public FrmLogin()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            LoginRequest loginRequest = new()
            {
                Username = txtUsername.Text,
                Password = txtPassword.Text
            };
            Jwt = Login(loginRequest).Result;
            this.Close();
        }

        private static async Task<string> Login(LoginRequest loginRequest)
        {
            var handler = new HttpClientHandler();

            HttpClient client = new(handler)
            {
                BaseAddress = new Uri("https://localhost:7182/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.PostAsJsonAsync("Login", loginRequest).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                LoginResponse? loginResponse = JsonConvert.DeserializeObject<LoginResponse>(data);

                if (loginResponse?.Token != null)
                {
                    return loginResponse.Token;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
