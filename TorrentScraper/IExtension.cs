using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public interface IExtension
{
    string Name { get; set; }
    public Window extensionWindow { get; set; }
    public Window getExtensionWindow();
    public void startFunction(object[] args);
}