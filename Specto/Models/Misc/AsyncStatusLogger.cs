using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Specto
{
    public class AsyncStatusLogger : INotifyPropertyChanged, ILogger
    { 
        private string text;
        private float displayTime;

        // Property used by UI.
        public string Text
        {
            get { return text; }
            private set
            {
                text = value;
                NotifyPropertyChanged();
            }
        }

        public AsyncStatusLogger(float displayTime)
        {
            this.displayTime = displayTime;
        }

        public void Clear() => Text = "";

        public void Log(string text)
        {
            Text = text;
            Task.Run(async () =>
            {
                await Task.Delay((int)(displayTime * 1000f));
                Clear();
            });
        } 

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
