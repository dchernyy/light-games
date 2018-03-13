using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gamez
{
    internal class VModel : INotifyPropertyChanged
    {
        Random r = new Random();
        private ImageSource _coin;
        public ImageSource Coin
        {
            get { return _coin; }
            set
            {
                _coin = value;
                OnPropertyChanged("Coin");
            }
        }
        public Command Flip { get; set; } = new Command();
        public Command Game { get; set; } = new Command();
        private bool _canFlip;
        public bool CanFlip
        {
            get { return _canFlip; }
            set
            {
                _canFlip = value;
                OnPropertyChanged("CanFlip");
            }
        }
        private string _gameResult;
        public string GameResult
        {
            get { return _gameResult; }
            set
            {
                _gameResult = value;
                OnPropertyChanged("GameResult");
            }
        }
        private string _comp;
        public string Comp
        {
            get { return _comp;  }
            set
            {
                _comp = value;
                OnPropertyChanged("Comp");
            }
        }
        public VModel()
        {
            Flip.Function = DoFlip;
            Game.Function = Play;
            CanFlip = true;
        }

        private void Play(object obj)
        {
            if (obj == null || !(obj is string)) return;
            int id = int.Parse((string)obj);
            int cid = r.Next(3);
            // 0 -- камень, 1 -- ножницы, 2 -- бумага
            switch (cid)
            {
                case 0:
                    Comp = "Камень";
                    break;
                case 1:
                    Comp = "Ножницы";
                    break;
                case 2:
                    Comp = "Бумага";
                    break;
            }
            GameResult = null;
            if (id == cid) GameResult = "Ничья";
            if ((id == 0 && cid == 1 || id == 1 && cid == 2 || id == 2 && cid == 0) && GameResult == null) GameResult = "Победа";
            if (GameResult == null) GameResult = "Поражение";
            SQL($"insert into KNB ([state], [timestamp]) values (\'{GameResult}\', \'{DateTime.Now.Ticks}\')");
        }

        private async void DoFlip(object obj)
        {
            CanFlip = false;
            if (r.Next(2) == 1)
            {
                Coin = new BitmapImage(new Uri("Images/coin1.png", UriKind.Relative));
                SQL($"insert into Coin ([state], [timestamp]) values (\'obverse\', \'{DateTime.Now.Ticks.ToString()}\')");
            }
            else
            {
                Coin = new BitmapImage(new Uri("Images/coin2.png", UriKind.Relative));
                SQL($"insert into Coin ([state], [timestamp]) values (\'reverse\', \'{DateTime.Now.Ticks.ToString()}\')");
            }
                await Task.Delay(3 * 60 * 60);
            Coin = null;
            CanFlip = true;
        }

        private async void SQL(string query)
        {
            using (SqlConnection database = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=Game;Integrated Security=True"))
            {
                SqlCommand cmd = new SqlCommand(query, database);
                try
                {
                    await database.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    database.Close();
                }
                catch
                {
                    MessageBox.Show("Проблема с бд");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string a)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(a));
        }
    }

    internal class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public Action<object> Function { get; set; }
        public void Execute(object parameter)
        {
            Function(parameter);
        }
    }
}