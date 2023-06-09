﻿using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis {
  [StorableType("AAB32034-FA4A-4D70-937F-EF026C1D508A")]
  [Item("Exponential Transformation", "f(x) = b ^ x | Represents a exponential transformation.")]
  public class ExponentialTransformation : Transformation<double> {
    protected const string BaseParameterName = "Base";

    #region Parameters
    public IValueParameter<DoubleValue> BaseParameter {
      get { return (IValueParameter<DoubleValue>)Parameters[BaseParameterName]; }
    }
    #endregion

    #region properties
    public override string ShortName {
      get { return "Exp"; }
    }
    public double Base {
      get { return BaseParameter.Value.Value; }
    }
    #endregion

    [StorableConstructor]
    protected ExponentialTransformation(StorableConstructorFlag _) : base(_) { }

    protected ExponentialTransformation(ExponentialTransformation original, Cloner cloner)
      : base(original, cloner) {
    }

    public ExponentialTransformation(IEnumerable<string> allowedColumns)
      : base(allowedColumns) {
      Parameters.Add(new ValueParameter<DoubleValue>(BaseParameterName, "b | Base of exp-function", new DoubleValue(Math.E)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new ExponentialTransformation(this, cloner);
    }

    public override IEnumerable<double> Apply(IEnumerable<double> data) {
      return data.Select(d => Math.Pow(Base, d));
    }

    public override bool Check(IEnumerable<double> data, out string errorMsg) {
      errorMsg = "";
      return true;
    }
  }
}
