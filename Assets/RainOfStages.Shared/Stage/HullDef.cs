namespace RainOfStages.Stage
{
    internal struct HullDef
    {
        private static HullDef[] hullDefs = new HullDef[3];
        public float height;
        public float radius;

        static HullDef()
        {
            HullDef.hullDefs[0] = new HullDef()
            {
                height = 2f,
                radius = 0.5f
            };
            HullDef.hullDefs[1] = new HullDef()
            {
                height = 8f,
                radius = 1.8f
            };
            HullDef.hullDefs[2] = new HullDef()
            {
                height = 20f,
                radius = 5f
            };
        }

        public static HullDef Find(HullClassification hullClassification)
        {
            return HullDef.hullDefs[(int)hullClassification];
        }
    }
}
