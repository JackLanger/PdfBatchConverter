using System;
using System.Globalization;
using System.Windows.Data;

namespace PdfConverterWizard.converter
{
    public class EnableConvertButtonConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            String name = (String)values[0];
            int size = (int)values[1];
            if (size > 0 && name.Equals("Convert"))
                return true;
            else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
