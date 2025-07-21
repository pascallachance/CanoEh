using System.Net.Http.Headers;
using Domain.Models;
using Newtonsoft.Json;

namespace BaseClient
{
    public partial class frmLogin : Form
    {
        public string Jwt { get; private set; }

        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            LoginRequest loginRequest = new LoginRequest 
            { 
                Username = txtUsername.Text,
                Password= txtPassword.Text
            };
            Jwt = Login(loginRequest).Result;
            this.Close();
        }
        private async Task<string> Login(LoginRequest loginRequest)
        {
            var handler = new HttpClientHandler();

            HttpClient client = new HttpClient(handler);

            client.BaseAddress = new Uri("https://localhost:7182/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.PostAsJsonAsync("Login", loginRequest).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();


                LoginResponse? loginResponse = JsonConvert.DeserializeObject<LoginResponse>(data);
                return loginResponse.Token;
            }
            else
            {
                return "null";
            }
        }
    }
}
