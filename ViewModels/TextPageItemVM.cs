// TextPageItemVM.cs
using System.ComponentModel;
using Windows.UI.Xaml.Data;

namespace MemoriesDemo.ViewModels
{
    public partial class TextPageItemVM : INotifyPropertyChanged
    {
        private string _caption = "";

        public string Caption
        {
            get { return this._caption; }
            set
            {
                this._caption = value;
                this.OnPropertyChanged("Caption");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}