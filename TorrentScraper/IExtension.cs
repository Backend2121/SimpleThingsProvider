using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public interface IExtension
{
    string Name { get; set; }
    public Window extentionWindow { get; set; }
    public void setParameters(object[] args);
    public void startFunction();
}