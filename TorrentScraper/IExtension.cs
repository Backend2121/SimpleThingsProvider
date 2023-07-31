using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

public interface IExtension
{
    string name { get; set; }
    public Window extensionWindow { get; set; }
    public Window getExtensionWindow();
    public bool startFunction(object[] args);
    public List<UIElement> getElements(ListView lv, Label ol);
    public void showWindow();
    public void enableButton();
    public void disableButton();
}