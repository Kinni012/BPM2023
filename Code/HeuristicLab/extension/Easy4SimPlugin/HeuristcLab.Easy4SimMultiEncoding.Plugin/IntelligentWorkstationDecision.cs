namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    public class IntelligentWorkstationDecision
    {
        public double CostFactor  { get; set; }
        public double SpeedFactor { get; set; }
        public int Index { get; set; }
        public int Probability { get; set; }
        public int RelativId { get; set; }

        public double CostTimesSpeed => CostFactor * SpeedFactor;


        public IntelligentWorkstationDecision()
        {
                
        }

        public IntelligentWorkstationDecision(double costFactor, double speedFactor, int index, int relativId)
        {
            CostFactor = costFactor;
            SpeedFactor = speedFactor;
            Index = index;
            RelativId = relativId;
            Probability = 0;
        }
    }
}
