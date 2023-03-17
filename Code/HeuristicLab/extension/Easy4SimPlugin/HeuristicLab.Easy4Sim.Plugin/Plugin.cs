using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Easy4Sim.Plugin
{
    [Plugin("HeuristicLab.Easy4Sim.Plugin", "Provides an implementation of the easy4sim Framework", "3.3.15.15587")]
    [PluginFile("HeuristicLab.Easy4Sim.Plugin-3.3.dll", PluginFileType.Assembly)]
    [PluginDependency("HeuristicLab.Attic", "1.0")]
    [PluginDependency("HeuristicLab.Common", "3.3")]
    [PluginDependency("HeuristicLab.Common.Resources", "3.3")]
    [PluginDependency("HeuristicLab.Core", "3.3")]
    [PluginDependency("HeuristicLab.Core.Views", "3.3")]
    [PluginDependency("HeuristicLab.Data", "3.3")]
    [PluginDependency("HeuristicLab.MainForm", "3.3")]
    [PluginDependency("HeuristicLab.MainForm.WindowsForms", "3.3")]
    public class Plugin : PluginBase
    {
    }
}
