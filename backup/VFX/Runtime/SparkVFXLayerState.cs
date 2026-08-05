namespace ProjectSpark.UI.VFX
{
    public sealed class SparkVFXLayerState
    {
        public SparkVFXBaseState BaseState =
            SparkVFXBaseState.Normal;


        public SparkVFXLoopType Loop =
            SparkVFXLoopType.None;


        public SparkVFXOverrideType Override =
            SparkVFXOverrideType.None;


        public bool HasOverride
        {
            get
            {
                return Override !=
                       SparkVFXOverrideType.None;
            }
        }


        public void ResetOverride()
        {
            Override =
                SparkVFXOverrideType.None;
        }
    }
}