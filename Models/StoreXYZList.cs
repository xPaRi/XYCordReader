using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYCordReader.Models
{
    public class StoreXYZList : ObservableCollection<StoreXYZ>
    {
        public void SetZero(decimal x, decimal y, decimal z)
        {
            this.ForEach(it => it.SetZero(x, y, z));
        }
    }
}
