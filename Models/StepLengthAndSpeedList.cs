using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XYCordReader.Models
{
    public class StepLengthAndSpeedList : ObservableCollection<StepLengthAndSpeed>
    {
        public StepLengthAndSpeedList() 
        {
            AddDefault();
        }

        public void AddDefault()
        {
            this.Add(new StepLengthAndSpeed(ModifierKeys.None,                           1.00m, 5000));
            this.Add(new StepLengthAndSpeed(ModifierKeys.Control,                        0.10m,  500));
            this.Add(new StepLengthAndSpeed(ModifierKeys.Shift,                         10.00m, 5000));
            this.Add(new StepLengthAndSpeed(ModifierKeys.Control| ModifierKeys.Shift,   20.00m, 5000));
        }

    }
}
