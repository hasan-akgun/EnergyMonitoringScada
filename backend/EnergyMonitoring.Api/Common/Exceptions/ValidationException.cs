namespace EnergyMonitoring.Api.Common.Exceptions
{
    public sealed class ValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(string field, string message) : this(new Dictionary<string, string[]>
                                                                            {
                                                                                [field] = new[] { message }
                                                                            })
        {
        }

        public ValidationException(IReadOnlyDictionary<string, string[]> errors) : base("Bir veya daha fazla doğrulama hatası oluştu.")
        {
            Errors = errors;
        }
    }
}
