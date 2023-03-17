using System;
using Easy4SimFrameworkExcelReader;
using Easy4SimFramework;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.ParameterOptimization;

namespace HeuristicLab.Easy4Sim.Plugin
{
    public sealed class Easy4SimParameterEvaluator : ParameterVectorEvaluator
    {



        public SolverSettings Settings { get; set; }
        public Easy4SimParameterEvaluator() : base()
        {

        }
        public Easy4SimParameterEvaluator(Easy4SimParameterEvaluator original, Cloner cloner) : base(original, cloner)
        {
        }

        public override IOperation Apply()
        {
            SolverSettings currentSimulationSettings = Settings.Clone();
            currentSimulationSettings.Logger = null; //deactivate logger for first tests
            string ParameterSettings = "";
            for (int i = 0; i < ProblemSizeParameter.ActualValue.Value; i++)
            {
                string[] names = ParameterNamesParameter.ActualValue[i].Split('/');
                int type = 2;
                foreach (ParameterOptimization optimization in currentSimulationSettings.SimulationObjects.ParameterOptimizations)
                {
                    if (optimization.NodeName == names[0] && optimization.NodeParameter == names[1])
                    {
                        if (optimization is ParameterOptimization<bool>)
                            type = 0;
                        else if (optimization is ParameterOptimization<int>)
                            type = 1;
                    }

                }

                switch (type)
                {
                    case 0: //bool 
                        ExcelReader.SetParameterForObject(names[0],
                            names[1],
                            ParameterVectorParameter.ActualValue[i] < 0.5 ? "False" : "True",
                            currentSimulationSettings.SimulationObjects);
                        if (ParameterSettings != "")
                            ParameterSettings += ";";
                        ParameterSettings += ParameterVectorParameter.ActualValue[i] < 0.5 ? "False" : "True";
                        continue;
                    case 1: //int
                        ExcelReader.SetParameterForObject(names[0],
                            names[1],
                            Convert.ToInt32(ParameterVectorParameter.ActualValue[i]).ToString(),
                            currentSimulationSettings.SimulationObjects);
                        if (ParameterSettings != "")
                            ParameterSettings += ";";
                        ParameterSettings += Convert.ToInt32(ParameterVectorParameter.ActualValue[i]).ToString();
                        continue;
                    case 2: //double
                        ExcelReader.SetParameterForObject(names[0],
                            names[1],
                            ParameterVectorParameter.ActualValue[i].ToString(),
                            currentSimulationSettings.SimulationObjects);
                        if (ParameterSettings != "")
                            ParameterSettings += ";";
                        ParameterSettings += ParameterVectorParameter.ActualValue[i].ToString();
                        continue;
                }
            }


            Solver.RunSimulation(currentSimulationSettings);

            QualityParameter.ActualValue = new DoubleValue(currentSimulationSettings.Statistics.Fitness);




            ValueLookupParameter<DoubleValue> valueLookup = null;
            ValueLookupParameter<StringValue> parameterLookup = null;

            if (ExecutionContext.Parent.Parent.Parameters.ContainsKey("Easy4SimQualityVariable"))
                if (ExecutionContext.Parent.Parent.Parameters["Easy4SimQualityVariable"] is ValueLookupParameter<DoubleValue> valueLookup1)
                    valueLookup = valueLookup1;


            if (ExecutionContext.Parent.Parent.Parameters.ContainsKey("Easy4SimQualityParameters"))
                if (ExecutionContext.Parent.Parent.Parameters["Easy4SimQualityParameters"] is ValueLookupParameter<StringValue> parameterLookup1)
                    parameterLookup = parameterLookup1;

            if (valueLookup == null)
                if (ExecutionContext.Parent.Parent.Parent.Parameters.ContainsKey("Easy4SimQualityVariable"))
                    if (ExecutionContext.Parent.Parent.Parent.Parameters["Easy4SimQualityVariable"] is ValueLookupParameter<DoubleValue> valueLookup2)
                        valueLookup = valueLookup2;


            if (parameterLookup == null)
                if (ExecutionContext.Parent.Parent.Parent.Parameters.ContainsKey("Easy4SimQualityParameters"))
                    if (ExecutionContext.Parent.Parent.Parent.Parameters["Easy4SimQualityParameters"] is ValueLookupParameter<StringValue> parameterLookup2)
                        parameterLookup = parameterLookup2;

            if (valueLookup == null || parameterLookup == null)
                return base.Apply();

            if (valueLookup.Value == null)
                valueLookup.Value = QualityParameter.ActualValue;


            if (parameterLookup.Value == null)
                parameterLookup.Value = new StringValue(ParameterSettings);


            if (valueLookup.Value == null ||
                parameterLookup.Value == null ||
                !(valueLookup.Value.Value < QualityParameter.ActualValue.Value)) return base.Apply();

            valueLookup.Value.Value = QualityParameter.ActualValue.Value;
            parameterLookup.Value.Value = ParameterSettings;


            return base.Apply();
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimParameterEvaluator(this, cloner);
        }
    }
}
