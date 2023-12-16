using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYCordReader.Models
{
    public class StoreXYZ(decimal absX, decimal absY, decimal absZ) : NotifyPropertyChangedBase
    {
        public decimal AbsX
        {
            get => _AbsX;
            set => SetValue(ref _AbsX, value);
        }

        private decimal _AbsX = absX;

        public decimal AbsY
        {
            get => _AbsY;
            set => SetValue(ref _AbsY, value);
        }

        private decimal _AbsY = absY;

        public decimal AbsZ
        {
            get => _AbsZ;
            set => SetValue(ref _AbsZ, value);
        }

        private decimal _AbsZ = absZ;

        public decimal RelX
        {
            get => _RelX;
            set => SetValue(ref _RelX, value);
        }

        private decimal _RelX = 0;

        public decimal RelY
        {
            get => _RelY;
            set => SetValue(ref _RelY, value);
        }

        private decimal _RelY = 0;

        public decimal RelZ
        {
            get => _RelZ;
            set => SetValue(ref _RelZ, value);
        }

        private decimal _RelZ = 0;

        public void SetZero(decimal xZero, decimal yZero, decimal zZero)
        {
            RelX = _AbsX - xZero; 
            RelY = _AbsY - yZero; 
            RelZ = _AbsZ - zZero;
        }
    }
}
