using System;
using Easy4SimFrameworkExcelReader;
using Easy4SimFramework;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.ParameterOptimization;
using HEAL.Attic;
using DoubleMatrix = HeuristicLab.Data.DoubleMatrix;

namespace HeuristicLab.Easy4Sim.Plugin
{
    [Item("Easy4Sim parameter optimization problem", "Parameter Optimization for a Easy4Sim Problem")]
    [StorableType]
    [Creatable(CreatableAttribute.Categories.ExternalEvaluationProblems, Priority = 110)]
    public class Easy4SimProblem : ParameterOptimizationProblem
    {
        private const string Easy4SimParameterName = "Easy4SimFile";
        private const string Easy4SimRuntimeName = "Easy4SimRuntime";

        private const string Easy4SimQualityParameterName = "Easy4SimQualityVariable";
        private const string Easy4SimParameterSettingsName = "Easy4SimQualityParameters";

        public IValueLookupParameter Easy4SimQualityParameter => (IValueLookupParameter<DoubleValue>)Parameters[Easy4SimQualityParameterName];
        public IValueLookupParameter Easy4SimParameterSettings => (IValueLookupParameter<StringValue>)Parameters[Easy4SimParameterSettingsName];


        private SolverSettings _solverSettings;



        public FileValue Easy4SimSimulationFile { get; set; }

        #region parameters
        public ValueLookupParameter<Easy4SimFileValue> Easy4SimParameter
        {
            get => (ValueLookupParameter<Easy4SimFileValue>)Parameters[Easy4SimParameterName];
            set => Parameters.Add(value);
        }
        public ValueParameter<LongValue> Easy4SimRuntime
        {
            get => (ValueParameter<LongValue>)Parameters[Easy4SimRuntimeName];
        }
        #endregion


        #region properties
        public Easy4SimFileValue Easy4SimEvaluationScript
        {
            get => Easy4SimParameter.Value;
            set => Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(value.Value,
                "The path to the Easy4Sim simulation file.", new Easy4SimFileValue());
        }
        #endregion


        #region ParameterChanged

        private void SimulationRuntimeChanged(object sender, EventArgs e)
        {

            if (_solverSettings == null) return;
            if (!(sender is ValueParameter<LongValue> longSender)) return;
            _solverSettings.Environment.FinishSimulationTime = longSender.Value.Value;
            if (Evaluator is Easy4SimParameterEvaluator evaluator)
                evaluator.Settings = _solverSettings;
        }

        private void FileValueChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("Sample message", "Entered FileValueChanged", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ExcelReader reader = new ExcelReader("..\\..\\..\\Easy4SimModels", Easy4SimEvaluationScript.Value);
            _solverSettings = reader.ReadFile(out _, out _, out _, out _, out _);
            _solverSettings.Environment.Delay = 0;
            _solverSettings.Environment.IncreaseDelayTime = 100000;


            Parameters.Remove(Easy4SimRuntimeName);
            Parameters.Add(new ValueParameter<LongValue>(Easy4SimRuntimeName,
                "The runtime of the Easy4Sim simulation.", new LongValue(_solverSettings.Environment.FinishSimulationTime)));

            Parameters[Easy4SimRuntimeName].ToStringChanged += SimulationRuntimeChanged;
            if (Evaluator is Easy4SimParameterEvaluator evaluator)
                evaluator.Settings = _solverSettings;
            //evaluator.Settings = _solverSettings;
            ProblemSize = _solverSettings.SimulationObjects.ParameterOptimizations.Count;
            Bounds = new DoubleMatrix(ProblemSize, 3);
            int i = 0;
            foreach (ParameterOptimization parameterOptimization in _solverSettings.SimulationObjects.ParameterOptimizations)
            {
                ParameterNames[i] = parameterOptimization.NodeName + "/" + parameterOptimization.NodeParameter;
                if (parameterOptimization is ParameterOptimization<int> intParameterOptimization)
                {
                    Bounds[i, 0] = intParameterOptimization.Minimum;
                    Bounds[i, 1] = intParameterOptimization.Maximum;
                    Bounds[i, 2] = (double)SolutionCreatorTypes.IntValue;
                }
                else if (parameterOptimization is ParameterOptimization<double> doubleParameterOptimization)
                {
                    Bounds[i, 0] = doubleParameterOptimization.Minimum;
                    Bounds[i, 1] = doubleParameterOptimization.Maximum;
                    Bounds[i, 2] = (double)SolutionCreatorTypes.DoubleValue;
                }
                else if (parameterOptimization is ParameterOptimization<bool> boolParameterOptimization)
                {
                    Bounds[i, 0] = 0;
                    Bounds[i, 1] = 1;
                    Bounds[i, 2] = (double)SolutionCreatorTypes.BoolValue;
                }

                i++;
            }
            Bounds.RowNames = ParameterNames;
        }


        #endregion

        public Easy4SimProblem() : base(new Easy4SimParameterEvaluator())
        {
               
            Maximization = new BoolValue(true);
            Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimParameterName,
                    "The path to the Easy4Sim simulation file.", new Easy4SimFileValue());

            ValueParameter<LongValue> simulationRuntime = new ValueParameter<LongValue>(Easy4SimRuntimeName,
                "The runtime of the Easy4Sim simulation.", new LongValue(0));
            Parameters.Add(simulationRuntime);
            
            Parameters.Add(new ValueLookupParameter<DoubleValue>(Easy4SimQualityParameterName, "The name of the quality variable name."));
            Parameters.Add(new ValueLookupParameter<StringValue>(Easy4SimParameterSettingsName, "The name of the quality parameter name."));

            Easy4SimEvaluationScript.FileDialogFilter = @"Easy4Sim Excel files|*.xlsx";
            Easy4SimEvaluationScript.ToStringChanged += FileValueChanged;

            //SolutionCreator = new Easy4SimSolutionCreator();
        }
        
        

        [StorableConstructor]
        public Easy4SimProblem(StorableConstructorFlag deserializing) : base(deserializing){}
        public Easy4SimProblem(Easy4SimProblem original, Cloner cloner) : base(original, cloner)
        {
            if (Evaluator is Easy4SimParameterEvaluator evaluator &&
                original.Evaluator is Easy4SimParameterEvaluator baseEvaluator)
                evaluator.Settings = baseEvaluator.Settings;
        }

        public Easy4SimProblem(IParameterVectorEvaluator evaluator) : base(evaluator){}

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimProblem(this, cloner);
        }
    }
}
