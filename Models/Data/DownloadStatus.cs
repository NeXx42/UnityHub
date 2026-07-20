using System.ComponentModel;

namespace Models.Data;

public class DownloadStatus : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public float currentValue
    {
        protected set
        {
            m_CurrentValue = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
        get => m_CurrentValue;
    }
    private float m_CurrentValue;

    public bool isDone { protected set; get; }

    public string getPercentageName => $"{Math.Round(currentValue * 100)}%";

}
