using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace IDEA.GAGMVVM.Converters
{
    /// <summary>
    /// Pokud je value == true, tak se vrátí první parametr z parameter[]
    /// Pokud je value == false, tak se vrátí druhý parameter z parameter[]
    /// Pokud value není boolean, vrátí se Visibility.Visible
    /// </summary>
    /// <example>
    /// Přidat do definice window:
    ///    xmlns:sys="clr-namespace:System;assembly=mscorlib"
    ///    xmlns:gc="clr-namespace:IDEA.GAGMVVM.Converters;assembly=GAGMVVM"
    ///
    /// Přidat do definicí resurces:
    ///    <gc:BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter"/>
    /// 
    /// Příslušnému prvku nastavit visibility konvertor:
    ///    <TextBlock.Visibility>
    ///        <Binding Path = "IsNew" Converter="{StaticResource BooleanToVisibilityConverter}">
    ///            <Binding.ConverterParameter>
    ///                <x:Array Type = "{x:Type sys:Object}">
    ///                    <Visibility> Visible </Visibility>
    ///                    <Visibility> Collapsed </Visibility>
    ///                </x:Array>
    ///            </Binding.ConverterParameter>
    ///        </Binding>
    ///    </TextBlock.Visibility>
    /// </example>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("BooleanToVisibilityConverter");

            if (value is bool booleanValue)
            {
                if (parameter is object[] paramArray && paramArray.Length == 2)
                {
                    if (paramArray[0] is Visibility trueVisibility && paramArray[1] is Visibility falseVisibility)
                    {
                        if (booleanValue)
                            return trueVisibility;

                        return falseVisibility;
                    }
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
