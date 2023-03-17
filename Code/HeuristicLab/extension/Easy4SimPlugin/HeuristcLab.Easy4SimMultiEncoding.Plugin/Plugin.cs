using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [Plugin("HeuristicLab.Easy4SimMultiEncoding.Plugin", "Easy4Sim multi encoding", "3.3.16.17184")]
    [PluginFile("HeuristicLab.Easy4SimMultiEncoding.Plugin-3.3.dll", PluginFileType.Assembly)]
    [PluginFile("Easy4SimFramework.dll", PluginFileType.Assembly)]
    [PluginFile("DataStore.dll", PluginFileType.Assembly)]
    [PluginDependency("HeuristicLab.Attic", "1.0")]
    [PluginDependency("HeuristicLab.Collections", "3.3")]
    [PluginDependency("HeuristicLab.Common", "3.3")]
    [PluginDependency("HeuristicLab.Common.Resources", "3.3")]
    [PluginDependency("HeuristicLab.Core", "3.3")]
    [PluginDependency("HeuristicLab.Core.Views", "3.3")]
    [PluginDependency("HeuristicLab.Data", "3.3")]
    [PluginDependency("HeuristicLab.Random", "3.3")]
    [PluginDependency("HeuristicLab.Encodings.BinaryVectorEncoding", "3.3")]
    [PluginDependency("HeuristicLab.Encodings.IntegerVectorEncoding", "3.3")]
    [PluginDependency("HeuristicLab.Encodings.PermutationEncoding", "3.3")]
    [PluginDependency("HeuristicLab.Encodings.RealVectorEncoding", "3.3")]
    [PluginDependency("HeuristicLab.MainForm", "3.3")]
    [PluginDependency("HeuristicLab.Optimization", "3.3")]
    [PluginDependency("HeuristicLab.Parameters", "3.3")]
    [PluginDependency("HeuristicLab.Persistence", "3.3")]
    [PluginDependency("HeuristicLab.Problems.ParameterOptimization", "3.3")]
    public class Plugin : PluginBase{}
}
