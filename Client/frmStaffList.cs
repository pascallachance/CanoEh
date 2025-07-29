namespace BaseClient
{
    public partial class frmStaffList : Form
    {
        private string jwt = "";
        public frmStaffList()
        {
            InitializeComponent();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            Login();
            //txtStaffList.Text = "";
            //List<Staff>? staffData = await GetStaffData();
            // foreach (Staff staff in staffData)
            //{
            //    txtStaffList.Text += staff.FirstName + " " + staff.LastName + "\r\n";
            //}
        }

        private void Login()
        {
            if (string.IsNullOrEmpty(jwt))
            {
                FrmLogin frmLogin = new FrmLogin();
                frmLogin.ShowDialog();
                jwt = frmLogin.Jwt;
            }
        }
    }
}
