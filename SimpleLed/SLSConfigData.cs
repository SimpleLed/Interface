namespace SimpleLed
{
    /// <summary>
    /// Base model for driver config details, designed to be inherited.
    /// </summary>
    public class SLSConfigData : BaseViewModel
    {
        /// <summary>
        /// Is this data dirty and in need of a save?
        /// </summary>
        public bool DataIsDirty { get; set; }
    }
}

