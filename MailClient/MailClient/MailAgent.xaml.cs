using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Mail;

using OpenPop.Mime;
using OpenPop.Pop3;
using OpenPop.Common;

namespace MailClient
{
    /// <summary>
    /// Логика взаимодействия для MailAgent.xaml
    /// </summary>
    public partial class MailAgent : Window
    {
        public MailAgent()
        {
            InitializeComponent();
        }
        List<OpenPop.Mime.Message> messages = new List<OpenPop.Mime.Message>();
        private string _login, _password;
        int count;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string richText = new TextRange
                (Body.Document.ContentStart,
                Body.Document.ContentEnd).Text;

            SmtpClient client = new SmtpClient("smtp.yandex.ru", 587);
            client.Credentials = new NetworkCredential(_login, _password);
            client.EnableSsl = true;
            client.Send(
                _login,
                To.Text,
                Subject.Text,
                richText);
        }

        void CheckNewMessages()
        {
            while (true)
            {
                Pop3Client client = new Pop3Client();
                client.Connect("pop.yandex.ru", 995, true);
                client.Authenticate(_login, _password);
                if (count < client.GetMessageCount())
                {
                    MessageBox.Show("У вас новое письмо!");
                   Dispatcher.Invoke( Refresh);
                }
                Task.Delay(10000);
            }
        }
        private void MessagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpenPop.Mime.Message message = messages[MessagesListBox.SelectedIndex];
            TextRange textRange = new TextRange(Body.Document.ContentStart,
                Body.Document.ContentEnd);

            textRange.Text = message.MessagePart.GetBodyAsText();
            Browser.NavigateToString( message.MessagePart.GetBodyAsText());
        }

        public void Refresh ()
        {
            MessagesListBox.Items.Clear();
            messages.Clear();
            Pop3Client client = new Pop3Client();
            client.Connect("pop.yandex.ru", 995, true);
            client.Authenticate(_login, _password);
            count = client.GetMessageCount();
            for (int i = 1; i < 20; i++)
            {
                messages.Add(client.GetMessage(i));
                MessagesListBox.Items.Add(messages[i - 1].Headers.Subject);
            }
            client.Disconnect();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public MailAgent(string Login, string Password)
        {
            _login = Login;
            _password = Password;
            InitializeComponent();

            Refresh();
            Task.Factory.StartNew(CheckNewMessages);
        }
    }
}
