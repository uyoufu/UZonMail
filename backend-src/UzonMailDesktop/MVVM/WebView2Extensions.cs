using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UZonMailDesktop.MVVM
{
    /// <summary>
    /// WebView2 扩展类
    /// </summary>
    public class WebView2Extensions
    {
        public static readonly DependencyProperty BindableSourceProperty =
        DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebView2Extensions), new PropertyMetadata(null, OnBindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        private static void OnBindableSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView2 webView)
            {
                webView.Source = new Uri((string)e.NewValue);
            }
        }
    }
}
