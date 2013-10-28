﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TLM.Core;
using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;

namespace TLM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Net net;

        public MainWindow()
        {
            InitializeComponent();
            var signal = ILMath.ones<float>(10, 10);
            var scene = new ILScene {
                new ILPlotCube(twoDMode: false) {
                    //new ILPoints {
                    //    Positions = signal,
                    //    Color = null,
                    //    Colors = signal,
                    //    Size = 2,
                    //}
                    new ILSurface(signal) {
                        Wireframe = { Color = System.Drawing.Color.FromArgb(50, 60, 60, 60) },
                        Colormap = Colormaps.Summer, 
                    }
                }
            };
            scene.First<ILPlotCube>().Rotation = Matrix4.Rotation(new Vector3(1f, 0.23f, 1), 0.7f);
            ilPanel.Scene.Add(scene);
        }

        public void CreateNet()
        {
            double sizeX = Convert.ToDouble(TBSizeX.Text, System.Globalization.CultureInfo.InvariantCulture);
            double sizeY = Convert.ToDouble(TBSizeY.Text, System.Globalization.CultureInfo.InvariantCulture);
            double sigma = Convert.ToDouble(TBSigma.Text, System.Globalization.CultureInfo.InvariantCulture);
            double dL = Convert.ToDouble(TBdL.Text, System.Globalization.CultureInfo.InvariantCulture);
            double z0 = Convert.ToDouble(TBZ0.Text, System.Globalization.CultureInfo.InvariantCulture);
            double Er = Convert.ToDouble(TBEr.Text, System.Globalization.CultureInfo.InvariantCulture);
            double f0 = Convert.ToDouble(TBFreq.Text, System.Globalization.CultureInfo.InvariantCulture);
            double C = Convert.ToDouble(TBC.Text, System.Globalization.CultureInfo.InvariantCulture);
            int N = Convert.ToInt32(TBN.Text, System.Globalization.CultureInfo.InvariantCulture);
            double bTop = Convert.ToDouble(TBBoundTop.Text, System.Globalization.CultureInfo.InvariantCulture);
            double bLeft = Convert.ToDouble(TBBoundLeft.Text, System.Globalization.CultureInfo.InvariantCulture);
            double bBot = Convert.ToDouble(TBBoundBot.Text, System.Globalization.CultureInfo.InvariantCulture);
            double bRight = Convert.ToDouble(TBBoundRight.Text, System.Globalization.CultureInfo.InvariantCulture);
            
            this.net = new Net(sizeX, sizeY, sigma, dL, z0, Er, f0, C, N, new Boundaries(bTop, bBot, bLeft, bRight));
        }
        

        private void BTCreateNet_Click(object sender, RoutedEventArgs e)
        {
            CreateNet();
            Designer.WorkingNet = this.net;
            Designer.DrawNet();
        }
    }
}
