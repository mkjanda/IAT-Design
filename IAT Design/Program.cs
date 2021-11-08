using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;

namespace IATClient
{
    static class Program
    {
        /// <summary>
        /// The maximum number of loop iterations before an infinite loop is suspected
        /// </summary>
        public const int MaxIterations = 1000;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {/*
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {

                String resourceName = "AssemblyLoadingAndReflection." +

                   new AssemblyName(args.Name).Name + ".dll";

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {

                    Byte[] assemblyData = new Byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);

                }C:\Users\Michael Janda\OneDrive\Documents\IAT Design\IAT Design\RegExDetails.Designer.cs
            };*/
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new IATConfigMainForm());
        }
    }
}
