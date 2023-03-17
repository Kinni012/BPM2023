using System;
using System.Text;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining
{
    public class QualityMetrics : ICloneable
    {
        /// <summary>
        /// The number of traces in the log that contain the current local process model divided by the number of all traces.
        /// Imagine a net with places Start - A - End.
        /// With the following 3 traces:
        /// A B C
        /// C D E
        /// D A E
        /// Two of the three traces contain A and therefore the support is 66.6%
        /// </summary>
        public double Support { get; set; }

        /// <summary>
        /// The harmonic mean of all explainable place occurrences divided by unexplainable
        /// </summary>
        public double Confidence { get; set; }
        /// <summary>
        /// Number of observed paths divided by the number of possible paths
        /// </summary>
        public double LanguageFit { get; set; }
        /// <summary>
        /// Number of transitions activated divided by the number of available transitions
        /// </summary>
        public double Determinism { get; set; }

        /// <summary>
        /// Number of events on nodes of the petri net / number of all events
        /// </summary>
        public double Coverage { get; set; }
        public QualityMetrics() { }

        public QualityMetrics(double support, double confidence, double languageFit, double determinism, double coverage)
        {
            Support = support;
            Confidence = confidence;
            LanguageFit = languageFit;
            Determinism = determinism;
            Coverage = coverage;
        }

        public double MetricSum => Support + Confidence + Determinism + LanguageFit + Coverage;


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Support => Support describes the number of traces that can be replayed on a given net"+ Environment.NewLine);
            sb.Append("Support: " + Math.Round(Support, 3) + Environment.NewLine);
            sb.Append("Confidence => The harmonic mean of all explainable place occurrences divided by unexplainable " + Environment.NewLine);
            sb.Append("Confidence: " + Math.Round(Confidence, 3) + Environment.NewLine);
            sb.Append("Language fit => Number of observed paths divided by the number of possible paths" + Environment.NewLine);
            sb.Append("LanguageFit: " + Math.Round(LanguageFit, 3) + Environment.NewLine);
            sb.Append("Determinism => Number of transitions activated divided by the number of available transitions" + Environment.NewLine);
            sb.Append("Determinism: " + Math.Round(Determinism, 3) + Environment.NewLine);
            sb.Append("Coverage => Number of events on nodes of the petri net / number of all events" + Environment.NewLine);
            sb.Append("Coverage: " + Math.Round(Coverage, 3) + Environment.NewLine);
            return sb.ToString();
        }

        public object Clone()
        {
            QualityMetrics result = new QualityMetrics();
            result.Confidence = Confidence;
            result.Coverage = Coverage;
            result.Determinism = Determinism;
            result.LanguageFit = LanguageFit;
            result.Support = Support;
            return result;
        }
    }
}
