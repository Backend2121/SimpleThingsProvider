using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

public interface IExtension
{
    string Name { get; set; }
    string ExtensionVersion { get; set; }
    public Window extensionWindow { get; set; }
    public Window getExtensionWindow();
    public bool startFunction(object[] args);
    public List<UIElement> getElements(ListView lv, Label ol);
    public List<DockPanel> getSettings();
    public void showWindow();
    public void enableButton(Label outputLabel);
    public void disableButton(Label outputLabel);
    public void saveSettings();
    public void checkUpdate();
}