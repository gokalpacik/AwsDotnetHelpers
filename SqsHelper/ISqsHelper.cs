using Amazon.SQS.Model;

namespace SqsHelper
{
    public interface ISqsHelper
    {
        Task<SendMessageResponse> SendMessageAsync(string messageBody);
        Task<SendMessageResponse> SendMessageWithAttributesAsync(string messageBody, Dictionary<string, MessageAttributeValue> attributeValues);
        Task<SendMessageBatchResponse> SendMessageBatchAsync(IEnumerable<SendMessageBatchRequestEntry> messages);
        Task<ReceiveMessageResponse> ReceiveMessagesAsync(bool deleteAfterRead, int maxNumberOfMessages, int WaitTimeSeconds);
        Task<PurgeQueueResponse> DeleteAllMessages();
        Task DeleteMessages(List<Message> messages);
        Task DeleteBatchMessages(List<Message> messages);
        Task<int> NumberOfMessagesInQueue();
    }
}