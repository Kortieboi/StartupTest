using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace GradingSystem
{
    public partial class AccessWindow : Form
    {
        public static List<User> UserRequests { get; private set; } = new List<User>();

        public AccessWindow()
        {
            InitializeComponent();
            DisplayUserRequests(); // Call DisplayUserRequests to display the user requests when the form loads
        }

        public AccessWindow(List<User> users) : this()
        {
            UserRequests.AddRange(users);
        }

        private void DisplayUserRequests()
        {
            foreach (var user in UserRequests)
            {
                var panel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Width = this.Width,
                    AutoSize = true
                };

                var lblUserInfo = new Label
                {
                    Text = $"{user.FirstName} {user.MiddleName} {user.LastName} ({user.Email})",
                    AutoSize = true
                };

                var btnConfirm = new Button
                {
                    Text = "Confirm",
                    Tag = user,
                    AutoSize = true
                };
                btnConfirm.Click += BtnConfirm_Click;

                panel.Controls.Add(lblUserInfo);
                panel.Controls.Add(btnConfirm);

                this.Controls.Add(panel);
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var user = button.Tag as User;

            if (user != null)
            {
                ConfirmAccount(user);
            }
        }

        private void ConfirmAccount(User user)
        {
            string username = GenerateUsername(user);
            string password = GeneratePassword();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            SaveUsernameAndPasswordToDatabase(user.Email, username, hashedPassword);
            SendEmailWithCredentials(user.Email, username, password);

            MessageBox.Show($"Account for {user.FirstName} {user.LastName} has been confirmed and credentials emailed.");
        }

        private string GenerateUsername(User user)
        {
            // Simple username generation: first initial + last name
            return $"{user.FirstName[0]}{user.LastName}".ToLower();
        }

        private string GeneratePassword()
        {
            // Simple password generation
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void SaveUsernameAndPasswordToDatabase(string email, string username, string hashedPassword)
        {
            string connectionString = "server=localhost;user=root;database=userdb;port=3306;password=Kortieboi;";
            string updateQuery = "UPDATE users SET username = @Username, password = @Password WHERE email = @Email";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@Username", username);
                    updateCommand.Parameters.AddWithValue("@Password", hashedPassword);
                    updateCommand.Parameters.AddWithValue("@Email", email);

                    try
                    {
                        updateCommand.ExecuteNonQuery();
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }

                connection.Close();
            }
        }

        private void SendEmailWithCredentials(string email, string username, string password)
        {
            // Your email sending code here
            // Use your preferred method or library to send emails
        }
    }
}
