using FeaturesProviderExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleThingsProvider
{
    partial class FeatureProviderExtension : IExtension
    {
        public string name { get { return "Feature Provider"; } set { } }
        public Window extensionWindow { get; set; }
        private Window dw;
        public FeatureProviderExtension()
        {
            extensionWindow = new FeatureProviderWindow();
            dw = (FeatureProviderWindow)extensionWindow;
        }

        public void disableButton(Label outputLabel)
        {
            return;
        }

        public void enableButton(Label outputLabel)
        {
            return;
        }

        public List<UIElement> getElements(ListView lv, Label ol)
        {
            List<UIElement> list = new List<UIElement>();
            return list;
        }

        public Window getExtensionWindow()
        {
            if (dw == null)
            {
                //dw = new FeatureProviderExtension();
            }
            return dw;
        }

        public List<DockPanel> getSettings()
        {
            throw new NotImplementedException();
        }

        public void saveSettings()
        {
            throw new NotImplementedException();
        }

        public void showWindow()
        {
            throw new NotImplementedException();
        }

        public bool startFunction(object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
