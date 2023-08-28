namespace SqsHelper.Models
{
    public class AwsSqsOptions
    {
        public string QueueUrl { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string UniqueKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Region)) throw new ArgumentException("Region should not be null or whitespace.");
            if (string.IsNullOrWhiteSpace(QueueUrl)) throw new ArgumentException("QueueUrl should not be null or whitespace.");
            if (string.IsNullOrWhiteSpace(AccessKeyId)) throw new ArgumentException("AccessIdKey should not be null or whitespace.");
            if (string.IsNullOrWhiteSpace(SecretAccessKey)) throw new ArgumentException("SecretAccessKey should not be null or whitespace.");
        }
    }
}
